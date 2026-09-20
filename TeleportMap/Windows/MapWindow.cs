using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Windowing;

namespace TeleportMap.Windows;

public class MapWindow : Window, IDisposable
{
    // =====================================================================
    //  EINSTELLUNGEN: hier deine eigenen Werte aus der alten Datei eintragen
    // =====================================================================

    // Koordinatenrahmen der Karte in Kartenpixeln
    private static readonly Vector2 CanvasSize = new(3700, 3700);

    private const float IconSize = 42f;        // Icon-Größe in Kartenpixeln, wächst mit dem Zoom
    private const float ZoomStep = 0.15f;      // 15 % pro Mausrad-Raste
    private const float MaxZoom = 4.5f;        // 250 % der eingepassten Ansicht
    private const float FadeDuration = 0.5f;   // Ein-/Ausblenden in Sekunden

    private const int GlowPart = 21;           // Schimmer-Index im NaviMap-Atlas
    private const float GlowScale = 1.6f;      // Schimmer-Größe im Verhältnis zum Icon
    private const float GlowSpeed = 0.8f;      // Drehung in Bogenmaß pro Sekunde

    // Kartenebene: Ätheryt zum Freischalten (0 = immer), Bilddatei,
    // Mittelpunkt in Kartenkoordinaten, Skalierung um den Mittelpunkt (1 = 1 Bildpixel pro Kartenpixel)
    private record MapLayer(uint AetheryteId, string File, Vector2 Center, float Scale = 1f);

    // Hauptkarte: nur die höchste freigeschaltete Stufe wird gezeichnet
    private static readonly MapLayer[] MainStages =
    [
        new(0,   "map_eorzea.png",   new Vector2(1370, 1180),0.62f),   // Eorzea
        new(111, "map_east.png",     new Vector2(2360, 900),0.741f),   // Kugane: Osten
        new(216, "map_tural.png",    new Vector2(1850, 1100), 1.875f),   // Tuliyollal
        new(217, "map_xaktural.png", new Vector2(1850, 1250)),   // Solution Nine
    ];

    // Unterkarten: jede erscheint, sobald ihr Ätheryt freigeschaltet ist
    private static readonly MapLayer[] SubMaps =
    [
        new(133, "map_norvrandt.png", new Vector2(550, 2750),1.4f),    // The Crystarium
        new(174, "map_shard.png",     new Vector2(1580, 2565)),   // Sinus Lacrimarum: Sea of Stars
        new(176, "map_shard.png",     new Vector2(1960, 2565),1.2f),   // Anagnorisis: World Unsundered
        new(213, "map_shard.png",     new Vector2(2320, 2565)),   // Leynode Mnemo: Unlost World
    ];

    // =====================================================================
    //  AB HIER PROGRAMMLOGIK
    // =====================================================================

    private const string TeleportTexPath = "ui/uld/Teleport.tex";
    private const string NaviMapTexPath = "ui/uld/NaviMap.tex";

    private readonly string mapDir = Plugin.PluginInterface.AssemblyLocation.DirectoryName!;
    private readonly List<(string Path, Vector2 Center, float Scale)> visibleLayers = new();

    private readonly UldWrapper teleportUld;
    private readonly UldWrapper naviMapUld;
    private readonly Dictionary<int, IDalamudTextureWrap?> uldParts = new();
    private readonly Dictionary<uint, string> names = new();
    private readonly HashSet<uint> unlocked = new();
    private IDalamudTextureWrap? glow;
    private bool glowLoaded;

    private float scale = 1f;              // Einpassung auf den Bildschirm
    private float zoom = 1f;               // Zoom durch den Nutzer
    private Vector2 pan = Vector2.Zero;    // Verschiebung in Bildschirmpixeln
    private bool dragging;
    private float alpha;                   // 0 = unsichtbar, 1 = voll sichtbar
    private bool closing;                  // true während des Ausblendens

    public MapWindow()
        : base("TeleportMap##Overlay",
            ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoScrollWithMouse)
    {
        RespectCloseHotkey = false; // ESC behandeln wir selbst, damit auch dabei ausgeblendet wird
        teleportUld = Plugin.PluginInterface.UiBuilder.LoadUld("ui/uld/Teleport.uld");
        naviMapUld = Plugin.PluginInterface.UiBuilder.LoadUld("ui/uld/NaviMap.uld");
    }

    public void Dispose()
    {
        foreach (var wrap in uldParts.Values)
            wrap?.Dispose();
        glow?.Dispose();
        teleportUld.Dispose();
        naviMapUld.Dispose();
    }

    // ---------------------------------------------------------------------
    //  Öffnen und Schließen
    // ---------------------------------------------------------------------

    public override void OnOpen()
    {
        // Immer in der Gesamtansicht starten
        zoom = 1f;
        pan = Vector2.Zero;
        dragging = false;
        alpha = 0f;

        // Freigeschaltete Ätheryten einmal pro Öffnen prüfen statt jedes Frame
        unlocked.Clear();
        foreach (var point in TeleportPoints.All)
            if (TeleportService.IsUnlocked(point.AetheryteId))
                unlocked.Add(point.AetheryteId);

        // Höchste freigeschaltete Hauptkarte, danach alle freigeschalteten Unterkarten
        visibleLayers.Clear();
        var mainStage = MainStages[0];
        foreach (var stage in MainStages)
            if (stage.AetheryteId == 0 || TeleportService.IsUnlocked(stage.AetheryteId))
                mainStage = stage;
        visibleLayers.Add((Path.Combine(mapDir, mainStage.File), mainStage.Center, mainStage.Scale));

        foreach (var sub in SubMaps)
            if (TeleportService.IsUnlocked(sub.AetheryteId))
                visibleLayers.Add((Path.Combine(mapDir, sub.File), sub.Center, sub.Scale));
    }

    public void Show()
    {
        closing = false;
        IsOpen = true;
    }

    public void Hide() => closing = true;

    public void ToggleFade()
    {
        if (IsOpen && !closing)
            Hide();
        else
            Show();
    }

    // ---------------------------------------------------------------------
    //  Zeichnen
    // ---------------------------------------------------------------------

    public override void PreDraw()
    {
        // Transparenz pro Frame Richtung Ziel bewegen
        var step = ImGui.GetIO().DeltaTime / FadeDuration;
        alpha = Math.Clamp(alpha + (closing ? -step : step), 0f, 1f);
        if (closing && alpha <= 0f)
        {
            IsOpen = false;
            closing = false;
        }

        // Fenster bildschirmfüllend, Karte auf 90 % eingepasst
        var viewport = ImGui.GetMainViewport();
        scale = MathF.Min(viewport.Size.X / CanvasSize.X, viewport.Size.Y / CanvasSize.Y) * 0.9f;
        ImGui.SetNextWindowPos(viewport.Pos, ImGuiCond.Always);
        ImGui.SetNextWindowSize(viewport.Size, ImGuiCond.Always);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, alpha);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleVar(2);
    }

    public override void Draw()
    {
        // ESC selbst abfangen und dem Spiel wegnehmen, sonst reagiert es zusätzlich darauf
        if (!closing && Plugin.KeyState[VirtualKey.ESCAPE])
        {
            Plugin.KeyState[VirtualKey.ESCAPE] = false;
            Hide();
        }

        var origin = ImGui.GetWindowPos();
        ClampPan(ImGui.GetWindowSize());
        var mapOrigin = origin + pan;
        var mapScale = scale * zoom;
        var drawList = ImGui.GetWindowDrawList();

        // Hauptkarte unten, Unterkarten darüber, jeweils um ihren Mittelpunkt skaliert
        foreach (var (path, center, layerScale) in visibleLayers)
        {
            var tex = Plugin.TextureProvider.GetFromFile(path).GetWrapOrDefault();
            if (tex == null)
                continue;
            var size = tex.Size * layerScale;
            var min = mapOrigin + (center - size / 2f) * mapScale;
            drawList.AddImage(tex.Handle, min, min + size * mapScale, Vector2.Zero, Vector2.One, ImGui.GetColorU32(Vector4.One));
        }

        // Schimmer zuerst, damit er hinter allen Icons liegt
        foreach (var point in TeleportPoints.All)
            if (point.UldPart != null && unlocked.Contains(point.AetheryteId))
                DrawGlow(mapOrigin, mapScale, point);

        foreach (var point in TeleportPoints.All)
            DrawPoint(mapOrigin, mapScale, point);

        // Eingaben zuletzt auswerten, damit bekannt ist, ob die Maus über einem Icon liegt
        HandleZoomAndDrag(origin);
    }

    private void DrawPoint(Vector2 mapOrigin, float mapScale, TeleportPoint point)
    {
        // Gesperrte Punkte gar nicht zeichnen (Spoilerschutz)
        if (!unlocked.Contains(point.AetheryteId))
            return;

        var size = new Vector2(IconSize * mapScale);
        ImGui.SetCursorScreenPos(mapOrigin + point.Position * mapScale - size / 2f);

        var icon = GetIcon(point);
        ImGui.Image(icon.Handle, size);

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(GetName(point.AetheryteId));

        if (!closing && ImGui.IsItemClicked() && TeleportService.TeleportTo(point.AetheryteId, point.SubIndex))
            Hide();
    }

    private void DrawGlow(Vector2 mapOrigin, float mapScale, TeleportPoint point)
    {
        // Einmal laden und merken, auch wenn es fehlschlägt
        if (!glowLoaded)
        {
            glow = naviMapUld.Valid ? naviMapUld.LoadTexturePart(NaviMapTexPath, GlowPart) : null;
            glowLoaded = true;
        }
        if (glow == null)
            return;

        var center = mapOrigin + point.Position * mapScale;
        var half = IconSize * GlowScale * mapScale;
        var (sin, cos) = MathF.SinCos((float)ImGui.GetTime() * GlowSpeed);

        // Ecke (x, y) aus dem Bereich -1..1 um den Mittelpunkt gedreht
        Vector2 Corner(float x, float y) => center + new Vector2(x * cos - y * sin, x * sin + y * cos) * half;

        ImGui.GetWindowDrawList().AddImageQuad(glow.Handle,
            Corner(-1, -1), Corner(1, -1), Corner(1, 1), Corner(-1, 1),
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
            ImGui.GetColorU32(Vector4.One));
    }

    // ---------------------------------------------------------------------
    //  Daten nachschlagen
    // ---------------------------------------------------------------------

    // Name aus den Spieldaten, einmal nachgeschlagen und dann zwischengespeichert
    private string GetName(uint aetheryteId)
    {
        if (!names.TryGetValue(aetheryteId, out var name))
        {
            name = Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Aetheryte>().TryGetRow(aetheryteId, out var row)
                ? row.PlaceName.Value.Name.ToString()
                : $"Ätheryt {aetheryteId}";
            names[aetheryteId] = name;
        }
        return name;
    }

    // Stadt-Icon aus dem Teleport-Atlas, sonst normales Ätheryt-Icon
    private IDalamudTextureWrap GetIcon(TeleportPoint point)
    {
        if (point.UldPart is int part)
        {
            if (!uldParts.TryGetValue(part, out var wrap))
            {
                wrap = teleportUld.Valid ? teleportUld.LoadTexturePart(TeleportTexPath, part) : null;
                uldParts[part] = wrap;
            }
            if (wrap != null)
                return wrap;
        }

        return Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(point.IconId)).GetWrapOrEmpty();
    }

    // ---------------------------------------------------------------------
    //  Zoom und Verschieben
    // ---------------------------------------------------------------------

    private void HandleZoomAndDrag(Vector2 origin)
    {
        var io = ImGui.GetIO();
        var hovered = ImGui.IsWindowHovered();

        // Zoom per Mausrad, der Punkt unter dem Mauszeiger bleibt stehen
        if (hovered && io.MouseWheel != 0)
        {
            var oldZoom = zoom;
            zoom = Math.Clamp(zoom * (1f + io.MouseWheel * ZoomStep), 1f, MaxZoom);
            var mouseRel = io.MousePos - origin - pan;
            pan -= mouseRel * (zoom / oldZoom - 1f);
        }

        // Ziehen startet nur auf freier Fläche, nicht auf einem Icon
        if (hovered && !ImGui.IsAnyItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            dragging = true;
        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
            dragging = false;
        if (dragging)
            pan += io.MouseDelta;
    }

    // Kleiner als der Bildschirm: zentrieren. Größer: nur bis zum Bildschirmrand verschiebbar.
    private void ClampPan(Vector2 viewSize)
    {
        var mapSize = CanvasSize * scale * zoom;

        pan.X = mapSize.X <= viewSize.X
            ? (viewSize.X - mapSize.X) / 2f
            : Math.Clamp(pan.X, viewSize.X - mapSize.X, 0f);

        pan.Y = mapSize.Y <= viewSize.Y
            ? (viewSize.Y - mapSize.Y) / 2f
            : Math.Clamp(pan.Y, viewSize.Y - mapSize.Y, 0f);
    }
}
