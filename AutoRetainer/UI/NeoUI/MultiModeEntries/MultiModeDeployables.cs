using ECommons.Throttlers;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeDeployables : NeoUIEntry
{
    public override string Path => "Multi Mode/Deployables".Loc();

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Multi Mode - Deployables".Loc())
        .Checkbox("Wait For Voyage Completion".Loc(), () => ref C.MultiModeWorkshopConfiguration.MultiWaitForAll, "When enabled, AutoRetainer will wait for all deployables to return before logging into the character. If you're already logged in for another reason, it will still resend completed submarines—unless the global setting \"Wait even when already logged in\" is also turned on.".Loc())
        .Indent()
        .Checkbox("Wait even when already logged in".Loc(), () => ref C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn, "Changes the behavior of \"Wait for Voyage Completion\" (both global and per-character) so that AutoRetainer no longer resends individual submarines while already logged in. Instead, it will wait until all submarines have returned before taking action.".Loc())
        .InputInt(120f, "Maximum Wait, minutes".Loc(), () => ref C.MultiModeWorkshopConfiguration.MaxMinutesOfWaiting.ValidateRange(0, 9999), 10, 60, "If waiting for other deployables to return would exceed this number of minutes, AutoRetainer will ignore both the \"Wait for Voyage Completion\" and \"Wait even when already logged in\" settings.".Loc())
        .Unindent()
        .DragInt(60f, "Advance Relog Threshold, seconds".Loc(), () => ref C.MultiModeWorkshopConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300, "The number of seconds AutoRetainer should log in early before submarines on this character are ready to be resent.".Loc())
        .DragInt(120f, "Retainer venture processing cutoff, minutes".Loc(), () => ref C.DisableRetainerVesselReturn.ValidateRange(0, 60), "If set to a value greater than 0, AutoRetainer will stop processing any retainers this number of minutes before any character is scheduled to redeploy submarines, taking all previous settings into account.".Loc())
        .Checkbox("Sell items from Unconditional sell list right after deployment (requires retainers)".Loc(), () => ref C.VendorItemAfterVoyage)
        .Checkbox("Periodically check FC chest for gil upon entering workshop".Loc(), () => ref C.FCChestGilCheck, "Periodically checks the Free Company chest when entering the Workshop to keep the gil counter up to date.".Loc())
        .Indent()
        .SliderInt(150f, "Check frequency, hours".Loc(), () => ref C.FCChestGilCheckCd, 0, 24 * 5)
        .Widget("Reset cooldowns".Loc(), (x) =>
        {
            if(ImGuiEx.Button(x, C.FCChestGilCheckTimes.Count > 0)) C.FCChestGilCheckTimes.Clear();
        })
        .Unindent()
        .Checkbox("Shutdown the game after all deployables have been processed".Loc(), () => ref C.ShutdownOnSubExhaustion)
        .Indent()
        .SliderFloat(150f, "Don't shutdown if there are deployables that return within this amount of hours".Loc(), () => ref C.HoursForShutdown, 0f, 10f)
        .Widget(() =>
        {
            ImGuiEx.HelpMarker("Currently: ??\nRemaining for force shutdown: ??".Loc(
                (Utils.CanShutdownForSubs() ? "Can shutdown" : "Can NOT shutdown").Loc(),
                EzThrottler.GetRemainingTime("ForceShutdownForSubs")));
        })
        .Unindent()
            .TextWrapped("Auto-buy Ceruleum Tanks after entering Workshop:".Loc())
        .Indent()
        .Widget(() =>
        {
            if(Data != null)
            {
                ImGui.Checkbox("Enable on ??".Loc(Data.NameWithWorldCensored), ref Data.AutoFuelPurchase);
            }
                ImGuiEx.TextWrapped("In order to enable/disable fuel purchase for other characters, navigate to Functions, Exclusions, Order section.".Loc());
        })
        .InputInt(150f, "Tanks remaining to trigger purchase".Loc(), () => ref C.AutoFuelPurchaseLow.ValidateRange(100, 99999))
        .InputInt(150f, "Buy until this amount in inventory".Loc(), () => ref C.AutoFuelPurchaseMax)
        .Checkbox("Only buy when workstation is unlocked".Loc(), () => ref C.AutoFuelPurchaseOnlyWsUnlocked)
        .Unindent()
        .Checkbox("Exit the game upon deployable completion".Loc(), () => ref C.ExitOnSubCompletion, "Important: when activated, your multi mode will be set to do deployables only, no retainers.".Loc())
        .Indent()
        .InputInt(150f, "Maximum time to wait for sub return, minutes".Loc(), () => ref C.ExitOnSubCompletionTime)
        .Unindent()
        ;
}
