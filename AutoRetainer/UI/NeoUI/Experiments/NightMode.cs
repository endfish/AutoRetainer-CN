namespace AutoRetainer.UI.NeoUI.Experiments;

internal class NightMode : ExperimentUIEntry
{
    public override string Name => "Night Mode";
    public override void Draw()
    {
        ImGuiEx.TextWrapped(("Night mode:\n" +
                "- Wait on login screen option is forcefully enabled\n" +
                "- Built-in FPS limiter restrictions forcefully applied\n" +
                "- While unfocused and awaiting, game is limited to 0.2 FPS\n" +
                "- It may look like game hung up, but let it up to 5 seconds to wake up after you reactivate game window.\n" +
                "- By default, only Deployables are enabled in Night mode\n" +
                "- After disabling Night mode, Bailout manager will activate to relog you back to the game.").Loc());
        if(ImGui.Checkbox("Activate night mode".Loc(), ref C.NightMode)) MultiMode.BailoutNightMode();
        ImGui.Checkbox("Show Night mode checkbox".Loc(), ref C.ShowNightMode);
        ImGui.Checkbox("Do retainers in Night mode".Loc(), ref C.NightModeRetainers);
        ImGui.Checkbox("Do deployables in Night mode".Loc(), ref C.NightModeDeployables);
        ImGui.Checkbox("Make night mode status persistent".Loc(), ref C.NightModePersistent);
        ImGui.Checkbox("Make shutdown command activate night mode instead of shutting down the game".Loc(), ref C.ShutdownMakesNightMode);
    }
}
