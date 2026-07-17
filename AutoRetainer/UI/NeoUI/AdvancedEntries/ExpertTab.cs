using ECommons.Configuration;
using ECommons.Reflection;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries;
public class ExpertTab : NeoUIEntry
{
    public override string Path => "Advanced/Expert Settings".Loc();

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Behavior".Loc())
        .EnumComboFullWidth(null, "Action on accessing retainer bell if no ventures available:".Loc(), () => ref C.OpenBellBehaviorNoVentures)
        .EnumComboFullWidth(null, "Action on accessing retainer bell if any ventures available:".Loc(), () => ref C.OpenBellBehaviorWithVentures)
        .EnumComboFullWidth(null, "Task completion behavior after accessing bell:".Loc(), () => ref C.TaskCompletedBehaviorAccess)
        .EnumComboFullWidth(null, "Task completion behavior after manual enabling:".Loc(), () => ref C.TaskCompletedBehaviorManual)
        .EnumComboFullWidth(null, "Task completion behavior during plugin operation:".Loc(), () => ref C.TaskCompletedBehaviorAuto)
        .TextWrapped(ImGuiColors.DalamudGrey, "\"Close retainer list and disable plugin\" option for 3 previous settings is enforced during MultiMode operation.".Loc())
        .Checkbox("Stay in retainer menu if there are retainers to finish ventures within 5 minutes or less".Loc(), () => ref C.Stay5, "This option is enforced during MultiMode operation.".Loc())
        .Checkbox("Auto-disable plugin when closing retainer list".Loc(), () => ref C.AutoDisable, "Only applies when you exit menu by yourself. Otherwise, settings above apply.".Loc())
        .Checkbox("Do not show plugin status icons".Loc(), () => ref C.HideOverlayIcons)
        .Checkbox("Display multi mode type selector".Loc(), () => ref C.DisplayMMType)
        .Checkbox("Display deployables checkbox in workshop".Loc(), () => ref C.ShowDeployables)
        .Checkbox("Enable bailout module".Loc(), () => ref C.EnableBailout)
        .InputInt(150f, "Timeout before AutoRetainer will attempt to unstuck, seconds".Loc(), () => ref C.BailoutTimeout)

        .Section("Settings".Loc())
        .Checkbox("Allow operating on retainers without a job".Loc(), () => ref C.AllowUnemployed)
        .Widget("Skip Inn Login Cutscene".Loc(), text =>
        {
            ImGui.SetNextItemWidth(200);
            if(ImGuiEx.EnumCombo(text, ref C.CutsceneSkipMode))
            {
                S.InnCutsceneSkip.RefreshAccordingToConfig();
            }
            ImGuiEx.HelpMarker("Cutscene skip is detectable server-side and increases chance of ban".Loc(), EColor.RedBright, FontAwesomeIcon.ExclamationTriangle.ToIconString());
        })
        .Checkbox("Disable sorting and collapsing/expanding".Loc(), () => ref C.NoCurrentCharaOnTop)
        .Checkbox("Show MultiMode checkbox on plugin UI bar".Loc(), () => ref C.MultiModeUIBar)
        .SliderIntAsFloat(100f, "Retainer menu delay, seconds".Loc(), () => ref C.RetainerMenuDelay.ValidateRange(0, 2000), 0, 2000)
        .Checkbox("Allow venture timer to display negative values".Loc(), () => ref C.TimerAllowNegative)
        .Checkbox("Do not error check venture planner".Loc(), () => ref C.NoErrorCheckPlanner2)
        .Checkbox("Enable Manual relogs character postprocess".Loc(), () => ref C.AllowManualPostprocess, "Allow manual command invocation while AutoRetainer locked in postprocess. ".Loc())
        .Widget("Market Cooldown Overlay".Loc(), (x) =>
        {
            if(ImGui.Checkbox(x, ref C.MarketCooldownOverlay))
            {
                if(C.MarketCooldownOverlay)
                {
                    P.Memory.OnReceiveMarketPricePacketHook?.Enable();
                }
                else
                {
                    P.Memory.OnReceiveMarketPricePacketHook?.Disable();
                }
            }
        })

        .Section("Integrations".Loc())
        .Checkbox("Artisan integration".Loc(), () => ref C.ArtisanIntegration, "Automatically enables AutoRetainer while Artisan is Pauses Artisan operation when ventures are ready to be collected and a retainer bell is within range. Once ventures have been dealt with Artisan will be enabled and resume whatever it was doing.".Loc())

        .Section("Server Time".Loc())
        .Checkbox("Use server time instead of PC time".Loc(), () => ref C.UseServerTime)

        .Section("Utility".Loc())
        .Widget("Cleanup ghost retainers".Loc(), (x) =>
        {
            if(ImGui.Button(x))
            {
                var i = 0;
                foreach(var d in C.OfflineData)
                {
                    i += d.RetainerData.RemoveAll(x => x.Name == "");
                }
                DuoLog.Information("Cleaned ?? entries".Loc(i.ToString()));
            }
        })

        .Section("Import/Export".Loc())
        .Widget(() =>
        {
            if(ImGui.Button("Export without character data".Loc()))
            {
                var clone = C.JSONClone();
                clone.OfflineData = null;
                clone.AdditionalData = null;
                clone.FCData = null;
                clone.SelectedRetainers = null;
                clone.Blacklist = null;
                clone.AutoLogin = "";
                Copy(EzConfig.DefaultSerializationFactory.Serialize(clone, false));
            }
            if(ImGui.Button("Import and merge with character data".Loc()))
            {
                try
                {
                    var c = EzConfig.DefaultSerializationFactory.Deserialize<Config>(Paste());
                    c.OfflineData = C.OfflineData;
                    c.AdditionalData = C.AdditionalData;
                    c.FCData = C.FCData;
                    c.SelectedRetainers = C.SelectedRetainers;
                    c.Blacklist = C.Blacklist;
                    c.AutoLogin = C.AutoLogin;
                    if(c.GetType().GetFieldPropertyUnions().Any(x => x.GetValue(c) == null)) throw new NullReferenceException();
                    EzConfig.SaveConfiguration(C, $"Backup_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.json");
                    P.SetConfig(c);
                }
                catch(Exception e)
                {
                    e.LogDuo();
                }
            }
        });
}
