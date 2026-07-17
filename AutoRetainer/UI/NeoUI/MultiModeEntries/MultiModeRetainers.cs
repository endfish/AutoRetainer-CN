namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeRetainers : NeoUIEntry
{
    public override string Path => "Multi Mode/Retainers".Loc();

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Multi Mode - Retainers".Loc())
        .Checkbox("Wait For Venture Completion".Loc(), () => ref C.MultiModeRetainerConfiguration.MultiWaitForAll, "AutoRetainer will wait for all retainers to return before cycling to the next character in multi mode operation.".Loc())
        .DragInt(60f, "Advance Relog Threshold".Loc(), () => ref C.MultiModeRetainerConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300)
        .SliderInt(100f, "Minimum inventory slots to continue operation".Loc(), () => ref C.MultiMinInventorySlots.ValidateRange(2, 9999), 2, 30)
        .Checkbox("Synchronise Retainers (one time)".Loc(), () => ref MultiMode.Synchronize, "AutoRetainer will wait until all enabled retainers have completed their ventures. After that this setting will be disabled automatically and all characters will be processed.".Loc())
        .Checkbox("Enforce Full Character Rotation".Loc(), () => ref C.CharEqualize, "Recommended for users with > 15 characters, forces multi mode to make sure ventures are processed on all characters in order before returning to the beginning of the cycle.".Loc())
        .Indent()
        .Checkbox("Order characters by venture completion time".Loc(), () => ref C.LongestVentureFirst, "Characters that have completed ventures longer time ago will be checked first".Loc())
        .Checkbox("Order characters by retainer level and cap".Loc(), () => ref C.CappedLevelsLast, "Characters with retainers that can be levelled up will be done first; then, characters with retainers at max level; and then characters with retainers less than max level and level capped.".Loc())
        .Unindent();
}
