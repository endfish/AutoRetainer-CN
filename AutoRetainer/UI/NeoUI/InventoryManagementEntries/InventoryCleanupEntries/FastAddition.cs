using AutoRetainerAPI.Configuration;
using ECommons.Automation;
using ECommons.ExcelServices;
using ECommons.Throttlers;
using ECommons.WindowsFormsReflector;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public unsafe class FastAddition : InventoryManagementBase
{
    public override string Name { get; } = "Inventory Cleanup/Fast Addition and Removal";

    private FastAddition()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
        .Section(Name.Loc())
        .Widget(() =>
        {
            var selectedSettings = InventoryCleanupCommon.SelectedPlan;
        ImGuiEx.TextWrapped(GradientColor.Get(EColor.RedBright, EColor.YellowBright), "While this text is visible, hover over items while holding:".Loc());
        ImGuiEx.Text(!ImGui.GetIO().KeyShift ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, "Shift - add to Quick Venture Sell List".Loc());
        ImGuiEx.Text("* Items that already in Unconditional Sell List or Discard List WILL NOT BE ADDED to Quick Venture Sell List".Loc());
        ImGuiEx.Text(!ImGui.GetIO().KeyCtrl ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, "Ctrl - add to Unconditional Sell List".Loc());
        ImGuiEx.Text("* Items that already in other lists WILL BE MOVED to Unconditional Sell List".Loc());
        ImGuiEx.Text(!IsKeyPressed(Keys.Tab) ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, "Tab - add to Discard List".Loc());
        ImGuiEx.Text("* Items that already in other lists WILL BE MOVED to Discard List".Loc());
            //ImGuiEx.Text(IsKeyPressed(Keys.Space) ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Space - add to Desynthesis List");
            //ImGuiEx.Text($"* Items that already in other lists WILL BE MOVED to Desynthesis List");
        ImGuiEx.Text(!ImGui.GetIO().KeyAlt ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, "Alt - delete from any list".Loc());
        ImGuiEx.Text("\nItems that are protected are unaffected by these actions".Loc());
            if(Svc.GameGui.HoveredItem > 0)
            {
                var id = (uint)(Svc.GameGui.HoveredItem % 1000000);
                if(ImGui.GetIO().KeyShift)
                {
                    if(!selectedSettings.IMProtectList.Contains(id) 
                    && !selectedSettings.IMAutoVendorSoft.Contains(id)
                    && !selectedSettings.IMAutoVendorHard.Contains(id)
                    && !selectedSettings.IMDiscardList.Contains(id)
                    && !selectedSettings.IMDesynth.Contains(id)
                    )
                    {
                        if(selectedSettings.AddItemToList(IMListKind.SoftSell, id, out var error))
                        {
                    Notify.Success("Added ?? to Quick Venture Sell List".Loc(ExcelItemHelper.GetName(id)));
                        }
                        else
                        {
                            if(EzThrottler.Throttle($"Error_{error}", 1000)) Notify.Error(error);
                        }
                    }
                }
                if(ImGui.GetIO().KeyCtrl)
                {
                    if(!selectedSettings.IMProtectList.Contains(id) && !selectedSettings.IMAutoVendorHard.Contains(id) && !selectedSettings.IMAutoVendorSoft.Contains(id))
                    {
                        if(selectedSettings.AddItemToList(IMListKind.HardSell, id, out var error))
                        {
                    Notify.Success("Added ?? to Unconditional Sell List".Loc(ExcelItemHelper.GetName(id)));
                        }
                        else
                        {
                            if(EzThrottler.Throttle($"Error_{error}", 1000)) Notify.Error(error);
                        }
                    }
                }
                if(!CSFramework.Instance()->WindowInactive && IsKeyPressed(Keys.Tab))
                {
                    if(!selectedSettings.IMProtectList.Contains(id) && !selectedSettings.IMDiscardList.Contains(id))
                    {
                        if(selectedSettings.AddItemToList(IMListKind.Discard, id, out var error))
                        {
                    Notify.Success("Added ?? to Discard List".Loc(ExcelItemHelper.GetName(id)));
                        }
                        else
                        {
                            if(EzThrottler.Throttle($"Error_{error}", 1000)) Notify.Error(error);
                        }
                    }
                }
                /*if(!CSFramework.Instance()->WindowInactive && IsKeyPressed(Keys.Space))
                {
                    if(!selectedSettings.IMProtectList.Contains(id) && !selectedSettings.IMDesynth.Contains(id))
                    {
                        if(selectedSettings.AddItemToList(IMListKind.Desynth, id, out var error))
                        {
                    Notify.Success("Added ?? to Desynthesis List".Loc(ExcelItemHelper.GetName(id)));
                        }
                        else
                        {
                            if(EzThrottler.Throttle($"Error_{error}", 1000)) Notify.Error(error);
                        }
                    }
                }*/
                if(ImGui.GetIO().KeyAlt)
                {
                if(selectedSettings.IMAutoVendorSoft.Remove(id)) Notify.Info("Removed ?? from Quick Venture Sell List".Loc(ExcelItemHelper.GetName(id)));
                if(selectedSettings.IMAutoVendorHard.Remove(id)) Notify.Info("Removed ?? from Unconditional Sell List".Loc(ExcelItemHelper.GetName(id)));
                if(selectedSettings.IMDiscardList.Remove(id)) Notify.Info("Removed ?? from Discard List".Loc(ExcelItemHelper.GetName(id)));
                if(selectedSettings.IMDesynth.Remove(id)) Notify.Info("Removed ?? from Desynthesis List".Loc(ExcelItemHelper.GetName(id)));
                }
            }
        });
        DisplayPriority = -10;
    }
}
