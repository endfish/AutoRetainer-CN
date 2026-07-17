namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeFPSLimiter : NeoUIEntry
{
    public override string Path => "Multi Mode/FPS Limiter".Loc();

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("FPS Limiter".Loc())
        .TextWrapped("FPS Limiter is only active when Multi Mode is enabled".Loc())
        .Widget("Target frame rate when idling".Loc(), (x) =>
        {
            ImGui.SetNextItemWidth(100f);
            UIUtils.SliderIntFrameTimeAsFPS(x, ref C.TargetMSPTIdle, C.ExtraFPSLockRange ? 1 : 10);
        })
        .Widget("Target frame rate when idling".Loc(), (x) =>
        {
            ImGui.SetNextItemWidth(100f);
            UIUtils.SliderIntFrameTimeAsFPS("Target frame rate when operating".Loc(), ref C.TargetMSPTRunning, C.ExtraFPSLockRange ? 1 : 20);
        })
        .Checkbox("Release FPS lock when game is active".Loc(), () => ref C.NoFPSLockWhenActive)
        .Checkbox("Allow extra low FPS limiter values".Loc(), () => ref C.ExtraFPSLockRange, "No support is provided if you enable this and run into ANY errors in Multi Mode".Loc())
        .Checkbox("Limiter active only when shutdown timer is set".Loc(), () => ref C.FpsLockOnlyShutdownTimer);
}
