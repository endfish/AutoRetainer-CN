namespace AutoRetainer.Modules.GcHandin;

internal static class AutoGCHandinUI
{
    internal static void Draw()
    {
        ImGui.Checkbox("Tray notification upon handin completion (requires NotificationMaster)".Loc(), ref C.GCHandinNotify);
    }
}
