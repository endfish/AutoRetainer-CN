namespace AutoRetainer.UI.NeoUI;
public class MainSettings : NeoUIEntry
{
    public override string Path => "General".Loc();

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Delays".Loc())
        .Widget(100f, "Time Desynchronization Compensation".Loc(), (x) => ImGuiEx.SliderInt(x, ref C.UnsyncCompensation.ValidateRange(-60, 0), -10, 0), "Additional amount of seconds that will be subtracted from venture ending time to help mitigate possible issues of time desynchronization between the game and your PC.".Loc())
        .Widget(100f, "Additional Interaction Delay, frames".Loc(), (x) => ImGuiEx.SliderInt(x, ref C.ExtraFrameDelay.ValidateRange(-10, 100), 0, 50), "The lower this value is the faster plugin will use actions. When dealing with low FPS or high latency you may want to increase this value. If you want the plugin to operate faster you may decrease it.".Loc())
        .Widget("Extra Logging".Loc(), (x) => ImGui.Checkbox(x, ref C.ExtraDebug), "This option enables excessive logging for debugging purposes. It will spam your log and cause performance issues while enabled. This option will disable itself upon plugin reload or game restart.".Loc())

        .Section("Operation".Loc())
        .Widget("Assign + Reassign".Loc(), (x) =>
        {
            if(ImGui.RadioButton(x, C.EnableAssigningQuickExploration && !C._dontReassign))
            {
                C.EnableAssigningQuickExploration = true;
                C.DontReassign = false;
            }
        }, "Automatically assigns enabled retainers to a Quick Venture if they have none already in progress and reassigns current venture.".Loc())
        .Widget("Collect".Loc(), (x) =>
        {
            if(ImGui.RadioButton(x, !C.EnableAssigningQuickExploration && C._dontReassign))
            {
                C.EnableAssigningQuickExploration = false;
                C.DontReassign = true;
            }
        }, "Only collect venture rewards from the retainer, and will not reassign them.\nHold CTRL when interacting with the Summoning Bell to apply this mode temporarily.".Loc())
        .Widget("Reassign".Loc(), (x) =>
        {
            if(ImGui.RadioButton(x, !C.EnableAssigningQuickExploration && !C._dontReassign))
            {
                C.EnableAssigningQuickExploration = false;
                C.DontReassign = false;
            }
        }, "Only reassign ventures that retainers are undertaking.".Loc())
        .Widget("RetainerSense".Loc(), (x) => ImGui.Checkbox(x, ref C.RetainerSense), "AutoRetainer will automatically enable itself when the player is within interaction range of a Summoning Bell. You must remain stationary or the activation will be cancelled.".Loc())
        .Widget(200f, "Activation Time".Loc(), (x) => ImGuiEx.SliderIntAsFloat(x, ref C.RetainerSenseThreshold, 1000, 100000));


}
