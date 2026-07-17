using AutoRetainerAPI.Configuration;
using System.Collections.Frozen;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeContingency : NeoUIEntry
{
    private static readonly FrozenDictionary<WorkshopFailAction, string> WorkshopFailActionNames = new Dictionary<WorkshopFailAction, string>()
    {
        [WorkshopFailAction.StopPlugin] = "Halt all plugin operation".Loc(),
        [WorkshopFailAction.ExcludeVessel] = "Exclude deployable from operation".Loc(),
        [WorkshopFailAction.ExcludeChar] = "Exclude captain from multi mode rotation".Loc(),
    }.ToFrozenDictionary();

    public override string Path => "Multi Mode/Contingency".Loc();

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Contingency".Loc())
        .TextWrapped("Here you can apply various fallback actions to perform in the case of some common failure states or potential operation errors.".Loc())
        .EnumComboFullWidth(null, "Ceruleum Tanks Expended".Loc(), () => ref C.FailureNoFuel, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "Applies selected fallback action in the case of insufficient Ceruleum Tanks to deploy vessel on a new voyage.".Loc())
        .EnumComboFullWidth(null, "Unable to Repair Deployable".Loc(), () => ref C.FailureNoRepair, null, WorkshopFailActionNames, "Applies selected fallback action in the case of insufficient Magitek Repair Materials to repair a vessel.".Loc())
        .EnumComboFullWidth(null, "Inventory at Capacity".Loc(), () => ref C.FailureNoInventory, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "Applies selected fallback action in the case of the captain's inventory having insufficient space to receive voyage rewards.".Loc())
        .EnumComboFullWidth(null, "Critical Operation Failure".Loc(), () => ref C.FailureGeneric, (x) => x != WorkshopFailAction.ExcludeVessel, WorkshopFailActionNames, "Applies selected fallback action in the case of any unknown or miscellaneous error.".Loc())
        .Widget("Jailed by the GM".Loc(), (x) =>
        {
            ImGui.BeginDisabled();
            ImGuiEx.SetNextItemFullWidth();
            if(ImGui.BeginCombo("##jailsel", "Terminate the game".Loc())) { ImGui.EndCombo(); }
            ImGui.EndDisabled();
        }, "Applies selected fallback action in the case if you got jailed by the GM while plugin is running. Good luck!".Loc());
}
