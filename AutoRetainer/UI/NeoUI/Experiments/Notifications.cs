namespace AutoRetainer.UI.NeoUI.Experiments;
public class Notifications : ExperimentUIEntry
{
    public override void Draw()
    {
        ImGui.Checkbox("Display overlay notification if one of retainers has completed a venture".Loc(), ref C.NotifyEnableOverlay);
        ImGui.Checkbox("Do not display overlay in duty or combat".Loc(), ref C.NotifyCombatDutyNoDisplay);
        ImGui.Checkbox("Include other characters".Loc(), ref C.NotifyIncludeAllChara);
        ImGui.Checkbox("Ignore other characters that have not been enabled in MultiMode".Loc(), ref C.NotifyIgnoreNoMultiMode);
        ImGui.Checkbox("Display notification in game chat".Loc(), ref C.NotifyDisplayInChatX);
        ImGuiEx.Text("If game is inactive: (requires NotificationMaster to be installed and enabled)".Loc());
        ImGui.Checkbox("Send desktop notification on retainers available".Loc(), ref C.NotifyDeskopToast);
        ImGui.Checkbox("Flash taskbar".Loc(), ref C.NotifyFlashTaskbar);
        ImGui.Checkbox("Do not notify if AutoRetainer is enabled or MultiMode is running".Loc(), ref C.NotifyNoToastWhenRunning);
    }
}
