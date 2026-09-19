using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace TeleportMap;

public static unsafe class TeleportService
{
    private const uint TeleportActionId = 5;

    public static bool TeleportTo(uint aetheryteId, byte subIndex = 0)
    {
        if (Control.GetLocalPlayer() == null)
            return false;
        if (!IsUnlocked(aetheryteId))
            return false;

        var status = ActionManager.Instance()->GetActionStatus(ActionType.Action, TeleportActionId);
        if (status != 0)
        {
            Plugin.ChatGui.PrintError($"Teleport gerade nicht möglich (Status {status}).");
            return false;
        }

        return Telepo.Instance()->Teleport(aetheryteId, subIndex);
    }
    public static bool IsUnlocked(uint aetheryteId)
    => UIState.Instance()->IsAetheryteUnlocked(aetheryteId);
}
