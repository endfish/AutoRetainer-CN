using AutoRetainerAPI.Configuration;
using ECommons.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public static unsafe class InventoryCleanupCommon
{
    public static Guid SelectedPlanGuid = Guid.Empty;

    public static InventoryManagementSettings SelectedPlan
    {
        get
        {
            if(SelectedPlanGuid == Guid.Empty)
            {
                return C.DefaultIMSettings;
            }
            else
            {
                var planIndex = C.AdditionalIMSettings.IndexOf(x => x.GUID == SelectedPlanGuid);
                if(planIndex == -1)
                {
                    SelectedPlanGuid = Guid.Empty;
                    return C.DefaultIMSettings;
                }
                else
                {
                    return C.AdditionalIMSettings[planIndex];
                }
            }
        }
    }

    public static NuiBuilder CreateCleanupHeaderBuilder()
    {
        return new NuiBuilder().Section("Inventory Cleanup Plan Selection".Loc()).Widget(DrawPlanSelector);
    }

    public static void DrawPlanSelector()
    {
        var selectedPlan = C.AdditionalIMSettings.FirstOrDefault(x => x.GUID == SelectedPlanGuid);
        ImGuiEx.InputWithRightButtonsArea(() =>
        {
            if(ImGui.BeginCombo("##selimplan", selectedPlan?.DisplayName ?? "Default Plan".Loc()))
            {
                if(ImGui.Selectable("Default Plan".Loc() + "###Default Plan", selectedPlan == null)) SelectedPlanGuid = Guid.Empty;
                ImGui.Separator();
                foreach(var x in C.AdditionalIMSettings)
                {
                    ImGui.PushID(x.ID);
                    if(ImGui.Selectable(x.DisplayName)) SelectedPlanGuid = x.GUID;
                    ImGui.PopID();
                }
                ImGui.EndCombo();
            }
        }, () =>
        {
            if(ImGuiEx.IconButton(FontAwesomeIcon.Plus))
            {
                var newPlan = new InventoryManagementSettings()
                {
                    AllowSellFromArmory = C.DefaultIMSettings.AllowSellFromArmory,
                    IMEnableContextMenu = C.DefaultIMSettings.IMEnableContextMenu,
                    IMEnableCofferAutoOpen = C.DefaultIMSettings.IMEnableCofferAutoOpen,
                    IMSkipVendorIfRetainer = C.DefaultIMSettings.IMSkipVendorIfRetainer,
                    IMEnableAutoVendor = C.DefaultIMSettings.IMEnableAutoVendor,
                    IMEnableNpcSell = C.DefaultIMSettings.IMEnableNpcSell,
                };
                C.AdditionalIMSettings.Add(newPlan);
                SelectedPlanGuid = newPlan.GUID;
            }
            ImGuiEx.Tooltip("Add new plan".Loc());
            ImGui.SameLine(0, 1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Copy))
            {
                var clone = (selectedPlan ?? C.DefaultIMSettings).DSFClone();
                clone.GUID = Guid.Empty;
                Copy(EzConfig.DefaultSerializationFactory.Serialize(clone));
            }
            ImGuiEx.Tooltip("Copy".Loc());
            ImGui.SameLine(0, 1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Paste))
            {
                try
                {
                    var newPlan = EzConfig.DefaultSerializationFactory.Deserialize<InventoryManagementSettings>(Paste()) ?? throw new NullReferenceException();
                    newPlan.GUID.Regenerate();
                    C.AdditionalIMSettings.Add(newPlan);
                    SelectedPlanGuid = newPlan.GUID;
                }
                catch(Exception e)
                {
                    e.Log();
                    Notify.Error(e.Message);
                }
            }
            ImGuiEx.Tooltip("Paste".Loc());
            if(selectedPlan != null)
            {
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.ArrowsUpToLine, enabled: ImGuiEx.Ctrl && selectedPlan != null))
                {
                    C.DefaultIMSettings = selectedPlan.DSFClone();
                    C.DefaultIMSettings.GUID.Regenerate();
                    C.DefaultIMSettings.Name = "";
                    new TickScheduler(() => C.AdditionalIMSettings.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("Make this plan default. Current default plan will be overwritten. Hold CTRL and click.".Loc());
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, enabled: ImGuiEx.Ctrl && selectedPlan != null))
                {
                    new TickScheduler(() => C.AdditionalIMSettings.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("Delete this plan. Hold CTRL and click.".Loc());
            }
        });
        if(selectedPlan != null)
        {
            ImGuiEx.SetNextItemFullWidth();
            ImGui.InputTextWithHint("##name", "Enter plan name".Loc(), ref selectedPlan.Name, 100);

            if(Data != null)
            {
                if(Data.InventoryCleanupPlan == SelectedPlanGuid)
                {
                    ImGuiEx.Text(ImGuiColors.ParsedGreen, UiBuilder.IconFont, FontAwesomeIcon.Check.ToIconString());
                    ImGui.SameLine();
                    ImGuiEx.Text(ImGuiColors.ParsedGreen, "Used by current character".Loc());
                    ImGui.SameLine();
                    if(ImGui.SmallButton("Unassign".Loc() + "###Unassign"))
                    {
                        Data.InventoryCleanupPlan = Guid.Empty;
                    }
                }
                else
                {
                    ImGuiEx.Text(ImGuiColors.DalamudOrange, UiBuilder.IconFont, FontAwesomeIcon.ExclamationTriangle.ToIconString());
                    ImGui.SameLine();
                    ImGuiEx.Text(ImGuiColors.DalamudOrange, "Not used by current character".Loc());
                    ImGui.SameLine();
                    if(ImGui.SmallButton("Assign".Loc() + "###Assign"))
                    {
                        Data.InventoryCleanupPlan = selectedPlan.GUID;
                    }
                }
                ImGui.SameLine();
            }

            var charas = C.OfflineData.Where(x => x.ExchangePlan == selectedPlan.GUID).ToArray();
            if(charas.Length > 0)
            {
                ImGuiEx.Text("Used by ?? characters in total".Loc(charas.Length));
                ImGuiEx.Tooltip($"{charas.Select(x => x.NameWithWorldCensored)}");
            }
            else
            {
                ImGuiEx.Text("Not used by any characters".Loc());
            }

            ImGuiEx.Text("Combine this plan's lists with default plan:".Loc());
            ImGui.Indent();
            ImGui.Checkbox("Combine Quick Venture sell list".Loc(), ref selectedPlan.AdditionModeSoftSellList);
            ImGuiEx.HelpMarker("Items retrieved from quick ventures included into both this plan and default plan will be sold.".Loc());
            ImGui.Checkbox("Combine Unconditional sell list".Loc(), ref selectedPlan.AdditionModeHardSellList);
            ImGuiEx.HelpMarker("Items included into both this plan and default plan will be sold. If included into both default and current plan, stack size bypass option from current plan will be honored. \"Maximum stack size to be sold\" option from current plan will override default plan's option. ".Loc());
            ImGui.Checkbox("Combine Discard list".Loc(), ref selectedPlan.AdditionModeDiscardList);
            ImGuiEx.HelpMarker("Items included into both this plan and default plan will be discarded. If included into both default and current plan, stack size bypass option from current plan will be honored. \"Maximum stack size to be discarded\" option from current plan will override default plan's option. ".Loc());
            ImGui.Checkbox("Combine Protection list".Loc(), ref selectedPlan.AdditionModeProtectList);
            ImGuiEx.HelpMarker("Items included into both this plan and default plan will not be sold automatically or exchanged to Grand Company, even if included into any lists.".Loc());
            ImGui.Unindent();
        }
    }
}
