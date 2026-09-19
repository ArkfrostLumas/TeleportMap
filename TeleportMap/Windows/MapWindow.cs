using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Textures.TextureWraps;
using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Keys;

namespace TeleportMap.Windows;

public class MapWindow : Window, IDisposable
{
    // Größe deiner map.png in Pixeln
    private static readonly Vector2 CanvasSize = new(2000, 2000);
    private const float IconSize = 28f;     // in Bildpixeln bei Zoom 1
    private const float ZoomStep = 0.15f;   // 15 % pro Mausrad-Raste
    private const float MaxZoom = 4.0f;     // 400 % der eingepassten Ansicht
    private const float FadeDuration = 0.5f; // Sekunden

    private readonly string mapImagePath;

    private float scale = 1f;              // Einpassung auf den Bildschirm
    private float zoom = 1f;               // Zoom durch den Nutzer
    private Vector2 pan = Vector2.Zero;    // Verschiebung in Bildschirmpixeln
    private bool dragging;
    private float alpha;     // 0 = unsichtbar, 1 = voll sichtbar
    private bool closing;    // true während des Ausblendens
    private readonly Dictionary<uint, string> names = new();
    private const string TeleportTexPath = "ui/uld/Teleport.tex";
    private readonly UldWrapper teleportUld;
    private readonly Dictionary<int, IDalamudTextureWrap?> uldParts = new();
    private readonly HashSet<uint> unlocked = new();

    public MapWindow(string mapImagePath)
        : base("TeleportMap##Overlay",
            ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.mapImagePath = mapImagePath;
        RespectCloseHotkey = false; // ESC behandeln wir selbst, damit auch dabei ausgeblendet wird
        teleportUld = Plugin.PluginInterface.UiBuilder.LoadUld("ui/uld/Teleport.uld");
    }

    public void Dispose()
    {
        foreach (var wrap in uldParts.Values)
            wrap?.Dispose();
        teleportUld.Dispose();
    }
    // Beim Öffnen immer in der Gesamtansicht starten
    public override void OnOpen()
    {
        zoom = 1f;
        pan = Vector2.Zero;
        dragging = false;
        alpha = 0f;
        // Einmal pro Öffnen prüfen statt jedes Frame
        unlocked.Clear();
        foreach (var point in TeleportPoints.All)
            if (TeleportService.IsUnlocked(point.AetheryteId))
                unlocked.Add(point.AetheryteId);
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


        var map = Plugin.TextureProvider.GetFromFile(mapImagePath).GetWrapOrDefault();
        if (map != null)
            drawList.AddImage(map.Handle, mapOrigin, mapOrigin + CanvasSize * mapScale, Vector2.Zero, Vector2.One, ImGui.GetColorU32(Vector4.One));

        foreach (var point in TeleportPoints.All)
            DrawPoint(mapOrigin, mapScale, point);

        // Eingaben zuletzt auswerten, damit bekannt ist, ob die Maus über einem Icon liegt
        HandleZoomAndDrag(origin);
    }

    private void DrawPoint(Vector2 mapOrigin, float mapScale, TeleportPoint point)
    {
        var size = new Vector2(IconSize * mapScale);
        ImGui.SetCursorScreenPos(mapOrigin + point.Position * mapScale - size / 2f);

        var isUnlocked = unlocked.Contains(point.AetheryteId);
        var icon = GetIcon(point);

        // Gesperrte Punkte blass zeichnen
        if (!isUnlocked)
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.35f);
        ImGui.Image(icon.Handle, size);
        if (!isUnlocked)
            ImGui.PopStyleVar();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(isUnlocked ? GetName(point.AetheryteId) : $"{GetName(point.AetheryteId)} (nicht freigeschaltet)");

        if (isUnlocked && !closing && ImGui.IsItemClicked() && TeleportService.TeleportTo(point.AetheryteId, point.SubIndex))
            Hide();
    }

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
    // Regions-Icon aus dem Teleport-Atlas, sonst normales Game-Icon
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

    private void HandleZoomAndDrag(Vector2 origin)
    {
        var io = ImGui.GetIO();
        var hovered = ImGui.IsWindowHovered();

        // Zoom per Mausrad, der Punkt unter dem Mauszeiger bleibt stehen
        if (hovered && io.MouseWheel != 0)
        {
            var maxZoom = MaxZoom; // nicht über native Bildauflösung
            var oldZoom = zoom;
            zoom = Math.Clamp(zoom * (1f + io.MouseWheel * ZoomStep), 1f, maxZoom);
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
