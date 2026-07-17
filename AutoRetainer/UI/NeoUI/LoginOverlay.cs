namespace AutoRetainer.UI.NeoUI;
public class LoginOverlay : NeoUIEntry
{
    public override string Path => "Login Overlay".Loc();

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
            .Section("Login Overlay".Loc())
            .Checkbox("Display Login Overlay".Loc(), () => ref C.LoginOverlay)
            .Widget("Login overlay scale multiplier".Loc(), (x) =>
            {
                ImGuiEx.SetNextItemWidthScaled(150f);
                if(ImGuiEx.SliderFloat(x, ref C.LoginOverlayScale.ValidateRange(0.1f, 5f), 0.2f, 2f)) P.LoginOverlay.bWidth = 0;
            })
            .Widget("Login overlay button padding".Loc(), (x) =>
            {
                ImGuiEx.SetNextItemWidthScaled(150f);
                if(ImGuiEx.SliderFloat(x, ref C.LoginOverlayBPadding.ValidateRange(0.5f, 5f), 1f, 1.5f)) P.LoginOverlay.bWidth = 0;
            })
        .Checkbox("Display hidden characters when searching".Loc(), () => ref C.LoginOverlayAllSearch)
        .SliderInt(150f, "Number of columns".Loc(), () => ref C.NumLoginOverlayCols.ValidateRange(1, 10), 1, 10)
        .SliderFloat(150f, "Overlay height, %".Loc(), () => ref C.LoginOverlayPercent.ValidateRange(20f, 100f), 20f, 100f);
}
