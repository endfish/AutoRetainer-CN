namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeCommon : NeoUIEntry
{
    public override string Path => "Multi Mode/Common Settings".Loc();

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Common Settings".Loc())
        .Checkbox("Wait on login screen".Loc(), () => ref C.MultiWaitOnLoginScreen, "If no character is available for ventures, you will be logged off until any character is available again. Title screen movie will be disabled while this option and MultiMode are enabled.".Loc())
        .Checkbox("Disable Multi Mode on Manual Login".Loc(), () => ref C.MultiDisableOnRelog, "Upon relogging via AutoRetainer's UI or command, disable Multi Mode.".Loc())
        .Checkbox("Do not reset Preferred Character on Manual Login".Loc(), () => ref C.MultiNoPreferredReset, "Upon relogging via AutoRetainer's UI or command, do not reset preferred character.".Loc())
        .Checkbox("Allow entering shared houses".Loc(), () => ref C.SharedHET)
        .Checkbox("Attempt to enter house on login even when Multi Mode is disabled".Loc(), () => ref C.HETWhenDisabled)
        .Checkbox("Do not teleport or enter house for retainers when already next to bell".Loc(), () => ref C.NoTeleportHetWhenNextToBell)

        .Section("Game startup".Loc())
        .Checkbox("Enable Multi Mode on Game Boot".Loc(), () => ref C.MultiAutoStart)
        .Checkbox("Enable Multi Mode on Plugin Startup".Loc(), () => ref C.MultiOnPluginLoad)
        .Indent()
        .SliderInt(150f, "Delay, seconds".Loc(), () => ref C.MultiModeOnPluginLoadDelay, 0, 20)
        .Unindent()
        .Widget("Auto-login on Game Boot".Loc(), (x) =>
        {
            ImGui.SetNextItemWidth(150f);
            var names = C.OfflineData.Where(s => !s.Name.IsNullOrEmpty()).Select(s => $"{s.Name}@{s.World}");
            var dict = names.ToDictionary(s => s, s => Censor.Character(s));
            dict.Add("", "Disabled".Loc());
            dict.Add("~", "Last logged in character".Loc());
            ImGuiEx.Combo(x, ref C.AutoLogin, ["", "~", .. names], names: dict);
        })
        .SliderInt(150f, "Delay".Loc(), () => ref C.AutoLoginDelay.ValidateRange(0, 60), 0, 20, "Set appropriate delay to let plugins fully load before logging in and to allow yourself some time to cancel login if needed".Loc())
        .Checkbox("Preserve Multi Mode state between plugin reloads".Loc(), () => ref C.PreserveMultiModeState)

        .Section("Inventory warnings".Loc())
        .InputInt(100f, "Retainer list: remaining inventory slots warning".Loc(), () => ref C.UIWarningRetSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, "Retainer list: remaining ventures warning".Loc(), () => ref C.UIWarningRetVentureNum.ValidateRange(2, 1000))
        .InputInt(100f, "Deployables list: remaining inventory slots warning".Loc(), () => ref C.UIWarningDepSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, "Deployables list: remaining fuel warning".Loc(), () => ref C.UIWarningDepTanksNum.ValidateRange(20, 1000))
        .InputInt(100f, "Deployables list: remaining repair kit warning".Loc(), () => ref C.UIWarningDepRepairNum.ValidateRange(5, 1000))

        .Section("Teleportation".Loc())
        .Widget(() => ImGuiEx.Text("Lifestream plugin is required".Loc()))
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("Lifestream", new Version("2.2.1.1"))]))
        .TextWrapped("You must register houses in Lifestream plugin for every character you want this option to work or enable Simple Teleport.".Loc())
        .TextWrapped("You can customize these settings per character in character configuration menu.".Loc())
        .Widget(() =>
        {
            if(Data != null && Data.GetAreTeleportSettingsOverriden())
            {
                ImGuiEx.TextWrapped(ImGuiColors.DalamudRed, "For current character teleport options are customized.".Loc());
            }
        })
        .Checkbox("Enabled".Loc(), () => ref C.GlobalTeleportOptions.Enabled)
        .Indent()
        .Checkbox("Teleport for retainers...".Loc(), () => ref C.GlobalTeleportOptions.Retainers)
        .Indent()
            .Checkbox("...to private house".Loc(), () => ref C.GlobalTeleportOptions.RetainersPrivate)
            .Checkbox("...to shared estate".Loc(), () => ref C.GlobalTeleportOptions.RetainersShared)
            .Checkbox("...to free company house".Loc(), () => ref C.GlobalTeleportOptions.RetainersFC)
            .Checkbox("...to apartment".Loc(), () => ref C.GlobalTeleportOptions.RetainersApartment)
            .TextWrapped("If all above are disabled or fail, will be teleported to inn.".Loc())
        .Unindent()
        .Checkbox("Teleport to free company house for deployables".Loc(), () => ref C.GlobalTeleportOptions.Deployables)
        .Checkbox("Enable Simple Teleport".Loc(), () => ref C.AllowSimpleTeleport)
        .Unindent()
        .Widget(() => ImGuiEx.HelpMarker("Allows teleporting to houses without registering them in Lifestream. Note: the Lifestream plugin is still required for teleportation to work.\n\nWarning: This option is less reliable than registering your houses in Lifestream. Use it only if necessary.".Loc(), EColor.RedBright, FontAwesomeIcon.ExclamationTriangle.ToIconString()))

        .Section("Bailout Module".Loc())
        .Checkbox("Auto-close and retry logging in on connection errors".Loc(), () => ref C.ResolveConnectionErrors, "Upon disconnecting, AutoRetainer will attempt to log back in. If the session has expired, no login attempt will be made.".Loc())
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("NoKillPlugin")]));
}
