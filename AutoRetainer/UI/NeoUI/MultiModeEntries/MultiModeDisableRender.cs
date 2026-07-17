using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;

public class MultiModeDisableRender : NeoUIEntry
{
    public override string Path => "Multi Mode/Disable Render".Loc();

    public override NuiBuilder Builder => new NuiBuilder()
        .Section("Disable Render".Loc())
        .Checkbox("Disable Render when in Multi Mode".Loc(), () => ref C.MultiDisableRender, "Disables world rendering while in Multi Mode.".Loc())
        .Checkbox("Only when in Night Mode".Loc(), () => ref C.MultiDisableRenderNightModeOnly)
        .Checkbox("Only when window is not active".Loc(), () => ref C.MultiDisableRenderOnlyInactive);
}
