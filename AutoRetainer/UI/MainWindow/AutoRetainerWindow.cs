using AutoRetainer.Modules.Voyage;
using AutoRetainer.UI.MainWindow.MultiModeTab;
using AutoRetainerAPI;
using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using ECommons.Configuration;
using ECommons.Funding;
using NightmareUI;

namespace AutoRetainer.UI.MainWindow;

internal unsafe class AutoRetainerWindow : Window
{
    private TitleBarButton LockButton;

    public AutoRetainerWindow() : base($"")
    {
        PatreonBanner.IsOfficialPlugin = () => true;
        LockButton = new()
        {
            Click = OnLockButtonClick,
            Icon = C.PinWindow ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen,
            IconOffset = new(3, 2),
            ShowTooltip = () => ImGui.SetTooltip("Lock window position and size".Loc()),
        };
        SizeConstraints = new()
        {
            MinimumSize = new(250, 100),
            MaximumSize = new(9999, 9999)
        };
        P.WindowSystem.AddWindow(this);
        AllowPinning = false;
        TitleBarButtons.Add(new()
        {
            Click = (m) => { if(m == ImGuiMouseButton.Left) S.NeoWindow.IsOpen = true; },
            Icon = FontAwesomeIcon.Cog,
            IconOffset = new(2, 2),
            ShowTooltip = () => ImGui.SetTooltip("Open settings window".Loc()),
        });
        TitleBarButtons.Add(LockButton);
    }

    private Action<string> SomeAction;

    private void OnLockButtonClick(ImGuiMouseButton m)
    {
        SomeAction += (s) => { };
        SomeAction -= (s) => { };
        if(m == ImGuiMouseButton.Left)
        {
            C.PinWindow = !C.PinWindow;
            LockButton.Icon = C.PinWindow ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen;
        }
    }

    public override void PreDraw()
    {
        var prefix = SchedulerMain.PluginEnabled ? $" [{SchedulerMain.Reason}]" : "";
        var tokenRem = TimeSpan.FromMilliseconds(Utils.GetRemainingSessionMiliSeconds());
        WindowName = $"{P.Name} {P.GetType().Assembly.GetName().Version}{prefix} | {FormatToken(tokenRem)}###AutoRetainer";
        if(C.PinWindow)
        {
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(C.WindowPos);
            ImGui.SetNextWindowSize(C.WindowSize);
        }
    }

    private string FormatToken(TimeSpan time)
    {
        if(time.TotalMilliseconds > 0)
        {
            if(time.Days > 0)
            {
                return "Session expires in ?? day(s)".Loc(time.Days) + (time.Hours > 0 ? " " + "?? hours".Loc(time.Hours) : "");
            }
            else
            {
                if(time.Hours > 0)
                {
                    return "Session expires in ?? hours".Loc(time.Hours);
                }
                else
                {
                    return "Session expires in less than an hour".Loc();
                }
            }
        }
        else
        {
            return "Session expired".Loc();
        }
    }
    public override void Draw()
    {
        //ImGuiEx.Text(GradientColor.Get(EColor.RedBright, EColor.YellowBright), "This version MUST NOT BE RUNNING UNATTENDED.");
        try
        {
            if(!C.AcceptedDisclamer)
            {
                new NuiBuilder()
                    .Section("Disclaimer".Loc())
                    .TextWrapped(ImGuiColors.DalamudYellow, "Please note that using AutoRetainer for RMT purposes is strictly prohibited. ".Loc())
                    .TextWrapped(ImGuiColors.DalamudRed, "To avoid unnecessary consequences please follow these rules when using AutoRetainer:".Loc())
                    .TextWrapped("1. Do not acknowledge your use of AutoRetainer in game chat;".Loc())
                    .TextWrapped("2. Do not leave AutoRetainer unattended for prolonged period of time;".Loc())
                    .TextWrapped("3. Ensure that your total playing+AutoRetainer usage time does not exceeds 16 hours per day; ensure that periods of inactivity exist between your retainer/submersible checking routine;".Loc())
                    .TextWrapped("4. Never reply back to players that are trying to do so-called bot checks by trading or chatting with you; instead, blacklist these players immediately;".Loc())
                    .TextWrapped("5. If questioned by game master, always claim that you are doing all actions manually and never admit plugin usage.".Loc())
                    .TextWrapped("Failure to follow these guidelines may put your account at risk.".Loc())
                    .TextWrapped(GradientColor.Get(ImGuiColors.DalamudYellow, ImGuiColors.DalamudRed), "You may not use AutoRetainer for Real Money Trading or other commercial purposes. No support will be provided if you are using it for these purposes.".Loc())
                    .Widget(() =>
                    {
                        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Check, "Accept and continue".Loc()))
                        {
                            C.AcceptedDisclamer = true;
                            EzConfig.Save();
                        }
                    })
                    .Draw();
                return;
            }
            var e = SchedulerMain.PluginEnabledInternal;
            var disabled = MultiMode.Active && !ImGui.GetIO().KeyCtrl;

            if(disabled)
            {
                ImGui.BeginDisabled();
            }
            if(ImGui.Checkbox("Enable ??".Loc(P.Name), ref e))
            {
                P.WasEnabled = false;
                if(e)
                {
                    SchedulerMain.EnablePlugin(PluginEnableReason.Auto);
                }
                else
                {
                    SchedulerMain.DisablePlugin();
                }
            }
            if(C.ShowDeployables && (VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType) || VoyageScheduler.Enabled))
            {
                ImGui.SameLine();
                ImGui.Checkbox("Deployables".Loc(), ref VoyageScheduler.Enabled);
            }
            if(disabled)
            {
                ImGui.EndDisabled();
                ImGuiComponents.HelpMarker("MultiMode controls this option. Hold CTRL to override.".Loc());
            }

            if(P.WasEnabled)
            {
                ImGui.SameLine();
                ImGuiEx.Text(GradientColor.Get(ImGuiColors.DalamudGrey, ImGuiColors.DalamudGrey3, 500), "Paused".Loc());
            }

            ImGui.SameLine();
            if(ImGui.Checkbox("Multi".Loc(), ref MultiMode.Enabled))
            {
                MultiMode.OnMultiModeEnabled();
            }
            Utils.DrawLifestreamAvailabilityIndicator();
            if(C.ShowNightMode)
            {
                ImGui.SameLine();
                if(ImGui.Checkbox("Night".Loc(), ref C.NightMode))
                {
                    MultiMode.BailoutNightMode();
                }
            }
            if(C.DisplayMMType)
            {
                ImGui.SameLine();
                ImGuiEx.SetNextItemWidthScaled(100f);
                ImGuiEx.EnumCombo("##mode", ref C.MultiModeType);
            }
            if(C.CharEqualize && MultiMode.Enabled)
            {
                ImGui.SameLine();
                if(ImGui.Button("Reset counters".Loc()))
                {
                    MultiMode.CharaCnt.Clear();
                }
            }

            Svc.PluginInterface.GetIpcProvider<object>(ApiConsts.OnMainControlsDraw).SendMessage();

            if(IPC.Suppressed)
            {
                ImGuiEx.Text(ImGuiColors.DalamudRed, "Plugin operation is suppressed by other plugin.".Loc());
                ImGui.SameLine();
                if(ImGui.SmallButton("Cancel".Loc()))
                {
                    IPC.Suppressed = false;
                }
            }

            if(P.TaskManager.IsBusy)
            {
                ImGui.SameLine();
                if(ImGui.Button("Abort ?? tasks".Loc(P.TaskManager.NumQueuedTasks)))
                {
                    P.TaskManager.Abort();
                }
            }

            PatreonBanner.DrawRight();
            ImGuiEx.EzTabBar("tabbar", PatreonBanner.Text,
                            ("Retainers".Loc(), MultiModeUI.Draw, null, true),
                            ("Deployables".Loc(), WorkshopUI.Draw, null, true),
                            ("Troubleshooting".Loc(), TroubleshootingUI.Draw, null, true),
                            ("Statistics".Loc(), DrawStats, null, true),
                            ("About".Loc(), CustomAboutTab.Draw, null, true)
                            );
            if(!C.PinWindow)
            {
                C.WindowPos = ImGui.GetWindowPos();
                C.WindowSize = ImGui.GetWindowSize();
            }
        }
        catch(Exception e)
        {
            ImGuiEx.TextWrapped(e.ToStringFull());
        }
    }

    private void DrawStats()
    {
        NuiTools.ButtonTabs([[C.RecordStats ? new("Ventures".Loc(), S.VentureStats.DrawVentures) : null, new("Gil".Loc(), S.GilDisplay.Draw), new("FC Data".Loc(), S.FCData.Draw)]]);
    }

    public override void OnClose()
    {
        EzConfig.Save();
        S.VentureStats.Data.Clear();
        MultiModeUI.JustRelogged = false;
    }

    public override void OnOpen()
    {
        MultiModeUI.JustRelogged = true;
    }
}
