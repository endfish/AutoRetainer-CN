using AutoRetainer.Internal.InventoryManagement;
using ECommons.GameHelpers;
using TerraFX.Interop.Windows;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class GeneralSettings : InventoryManagementBase
{
    public override string Name { get; } = "Inventory Cleanup/General Settings";

    private GeneralSettings()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name.Loc())
            .Checkbox("Auto-open venture coffers".Loc(), () => ref InventoryCleanupCommon.SelectedPlan.IMEnableCofferAutoOpen, "Multi Mode only. Before logging out, all coffers will be opened unless your inventory space is too low.".Loc())
            .Indent()
            .InputInt(100f, "Maximum to open at once".Loc(), () => ref InventoryCleanupCommon.SelectedPlan.MaxCoffersAtOnce)
            .Unindent()
            .Checkbox("Enable selling items to retainer".Loc(), () => ref InventoryCleanupCommon.SelectedPlan.IMEnableAutoVendor, "When AutoRetainer checks resents retainers to ventures, items will be sold according to Inventory Cleanup plan.".Loc())
            .Checkbox("Enable selling items to housing NPC".Loc(), () => ref InventoryCleanupCommon.SelectedPlan.IMEnableNpcSell, "When AutoRetainer enters a house, items will be sold according to the Inventory Cleanup plan. A housing vendor that supports item selling must be placed near the house entrance (not the workshop entrance)—you should be able to interact with the NPC immediately after entering.".Loc())
            .Indent()
            .Checkbox("Ignore NPC if retainer is available".Loc(), () => ref InventoryCleanupCommon.SelectedPlan.IMSkipVendorIfRetainer)
            .Widget("Sell now".Loc(), (x) =>
            {
                if(ImGuiEx.Button(x, Player.Interactable && InventoryCleanupCommon.SelectedPlan.IMEnableNpcSell && NpcSaleManager.GetValidNPC() != null && !IsOccupied() && !P.TaskManager.IsBusy))
                {
                    NpcSaleManager.EnqueueIfItemsPresent(true);
                }
            })
            .Unindent()
            .Checkbox("Auto-desynth items".Loc(), () => ref InventoryCleanupCommon.SelectedPlan.IMEnableItemDesynthesis)
            .Indent()
            .Widget("Armory chest: ".Loc(), t =>
            {
                ImGuiEx.TextV(t);
                ImGui.SameLine();
                ImGuiEx.RadioButtonBool("Desynthese".Loc(), "Skip".Loc(), ref InventoryCleanupCommon.SelectedPlan.IMEnableItemDesynthesisFromArmory, true);
            })
            .Unindent()
            .Checkbox("Enable context menu integration".Loc(), () => ref InventoryCleanupCommon.SelectedPlan.IMEnableContextMenu)
            .Checkbox("Allow selling/discarding items from Armory Chest".Loc(), () => ref InventoryCleanupCommon.SelectedPlan.AllowSellFromArmory)
            .Checkbox("Deliver eligible items into Armoire when in Multi Mode".Loc(), () => ref InventoryCleanupCommon.SelectedPlan.EnableCabinetAutoDelivery, "Items that are not in Armoire will be delivered there. Eligible items also will be excluded from being discarded, desynthesed, entrusted to retainers or delivered into grand company (only while you are running multi mode). This will trigger before Multi Mode Expert Delivery.".Loc())
            .Checkbox("Demo mode".Loc(), () => ref InventoryCleanupCommon.SelectedPlan.IMDry, "Do not sell/discard items, instead print in chat what would be sold".Loc())
            ;
    }
}
