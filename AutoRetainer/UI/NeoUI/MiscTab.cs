namespace AutoRetainer.UI.NeoUI;
public class MiscTab : NeoUIEntry
{
    public override string Path => "Miscellaneous".Loc();

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Statistics".Loc())
        .Checkbox("Record Venture Statistics".Loc(), () => ref C.RecordStats)

        .Section("Automatic Grand Company Expert Delivery".Loc())
        .Checkbox("Tray notification upon handin completion (requires NotificationMaster)".Loc(), () => ref C.GCHandinNotify)

        .Section("Performance".Loc())

        .If(() => Utils.IsBusy)
        .Widget("", (x) => ImGui.BeginDisabled())
        .EndIf()

        .Checkbox("Remove minimized FPS restrictions while plugin is operating".Loc(), () => ref C.UnlockFPS)
        .Checkbox("- Also remove general FPS restriction".Loc(), () => ref C.UnlockFPSUnlimited)
        .Checkbox("- Also pause ChillFrames plugin".Loc(), () => ref C.UnlockFPSChillFrames)
        .Checkbox("Raise FFXIV process priority while plugin is operating".Loc(), () => ref C.ManipulatePriority, "May result other programs slowdown".Loc())

        .If(() => Utils.IsBusy)
        .Widget("", (x) => ImGui.EndDisabled())
        .EndIf();
}
