using AutoRetainerAPI.Configuration;
using ECommons.Configuration;
using ECommons.ExcelServices;
using ECommons.Reflection;
using ECommons.Throttlers;
using Lumina.Excel.Sheets;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries;
public class EntrustManager : InventoryManagementBase
{
    public override string Name { get; } = "Entrust Manager";
    private Guid SelectedGuid = Guid.Empty;
    private string Filter = "";
    private InventoryManagementCommon InventoryManagementCommon = new();

    public override void Draw()
    {
        ImGuiEx.TextWrapped("Use advanced entrust manager to entrust specific items to specific retainers. In this window you can configure specific plans; then, you can assign entrust plans to your retainers in retainer configuration window.".Loc());
        ImGui.Checkbox("Enable".Loc(), ref C.EnableEntrustManager);
        ImGui.Checkbox("Output entrusted items into chat".Loc(), ref C.EnableEntrustChat);
        var selectedPlan = C.EntrustPlans.FirstOrDefault(x => x.Guid == SelectedGuid);

        ImGuiEx.InputWithRightButtonsArea(() =>
        {
            if(ImGui.BeginCombo($"##select", selectedPlan?.Name ?? "Select plan...".Loc(), ImGuiComboFlags.HeightLarge))
            {
                for(var i = 0; i < C.EntrustPlans.Count; i++)
                {
                    var plan = C.EntrustPlans[i];
                    ImGui.PushID(plan.Guid.ToString());
                    if(ImGui.Selectable(plan.Name, plan == selectedPlan))
                    {
                        SelectedGuid = plan.Guid;
                    }
                    ImGui.PopID();
                }
                ImGui.EndCombo();
            }
        }, () =>
        {
            if(ImGuiEx.IconButton(FontAwesomeIcon.Plus))
            {
                var plan = new EntrustPlan();
                C.EntrustPlans.Add(plan);
                SelectedGuid = plan.Guid;
                plan.Name = "Entrust plan ??".Loc(C.EntrustPlans.Count);
            }
            ImGui.SameLine();
            if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, enabled: selectedPlan != null && ImGuiEx.Ctrl))
            {
                C.EntrustPlans.Remove(selectedPlan);
            }
            ImGuiEx.Tooltip("Hold CTRL and click".Loc());
            ImGui.SameLine();
            if(ImGuiEx.IconButton(FontAwesomeIcon.Copy, enabled: selectedPlan != null))
            {
                Copy(EzConfig.DefaultSerializationFactory.Serialize(selectedPlan, false));
            }
            ImGui.SameLine();
            if(ImGuiEx.IconButton(FontAwesomeIcon.Paste, enabled: EzThrottler.Check("ImportPlan")))
            {
                try
                {
                    var plan = EzConfig.DefaultSerializationFactory.Deserialize<EntrustPlan>(Paste()) ?? throw new NullReferenceException();
                    plan.Guid = Guid.NewGuid();
                    if(plan.GetType().GetFieldPropertyUnions(ReflectionHelper.AllFlags).Any(x => x.GetValue(plan) == null)) throw new NullReferenceException();
                    C.EntrustPlans.Add(plan);
                    SelectedGuid = plan.Guid;
                    Notify.Success("Imported plan from clipboard".Loc());
                    EzThrottler.Throttle("ImportPlan", 2000, true);
                }
                catch(Exception e)
                {
                    DuoLog.Error(e.Message);
                }
            }
        });
        if(selectedPlan != null)
        {
            ImGuiEx.SetNextItemFullWidth();
            ImGui.InputTextWithHint($"##name", "Plan name".Loc(), ref selectedPlan.Name, 100);
            ImGui.Checkbox("Entrust Duplicates".Loc(), ref selectedPlan.Duplicates);
            ImGuiEx.HelpMarker("Mimics vanilla entrust duplicates option: entrusts any items that already present in retainer's inventory up until your retainer fills up it's stack of items. Does not affects crystals. Items and categories that are explicitly added into the list below will be excluded from being processed by this option.".Loc());
            ImGui.Indent();
            ImGui.Checkbox("Allow going over stack".Loc(), ref selectedPlan.DuplicatesMultiStack);
            ImGuiEx.HelpMarker("Allows entrust duplicates to create new stacks of items that already exist in the selected retainer.".Loc());
            ImGui.Unindent();
            ImGui.Checkbox("Allow entrusting from Armory Chest".Loc(), ref selectedPlan.AllowEntrustFromArmory);
            ImGui.Checkbox("Manual execution only".Loc(), ref selectedPlan.ManualPlan);
            ImGuiEx.HelpMarker("Mark this plan for manual execution only. This plan will only be processed upon manual \"Entrust Items\" button click and never automatically.".Loc());
            ImGui.Checkbox("Exclude items present in protection list".Loc(), ref selectedPlan.ExcludeProtected);
            ImGui.Separator();
            ImGuiEx.TreeNodeCollapsingHeader("Entrust categories (?? selected)".Loc(selectedPlan.EntrustCategories.Count) + "###ecats", () =>
            {
                ImGuiEx.TextWrapped("Here you can select item categories that will be entrusted as a whole. Individual items that are selected below will be excluded from these rules.".Loc());
                if(ImGui.BeginTable("EntrustTable", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.BordersInner))
                {
                    ImGui.TableSetupColumn("##1");
                    ImGui.TableSetupColumn("Item name".Loc(), ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("Amount to keep".Loc());
                    ImGui.TableHeadersRow();
                    foreach(var x in Svc.Data.GetExcelSheet<ItemUICategory>())
                    {
                        if(x.Name == "" || x.RowId == 39) continue;
                        var contains = selectedPlan.EntrustCategories.Any(s => s.ID == x.RowId);
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        if(ThreadLoadImageHandler.TryGetIconTextureWrap(x.Icon, true, out var icon))
                        {
                            ImGui.Image(icon.Handle, new(ImGui.GetFrameHeight()));
                        }
                        ImGui.TableNextColumn();
                        if(ImGui.Checkbox(x.Name.ToString(), ref contains))
                        {
                            if(contains)
                            {
                                selectedPlan.EntrustCategories.Add(new() { ID = x.RowId });
                            }
                            else
                            {
                                selectedPlan.EntrustCategories.RemoveAll(s => s.ID == x.RowId);
                            }
                        }
                        ImGui.TableNextColumn();
                        if(selectedPlan.EntrustCategories.TryGetFirst(s => s.ID == x.RowId, out var result))
                        {
                            ImGui.SetNextItemWidth(130f);
                            ImGui.InputInt($"##amtkeep{result.ID}", ref result.AmountToKeep);
                        }
                    }
                    ImGui.EndTable();
                }
            });
            ImGuiEx.TreeNodeCollapsingHeader("Entrust individual items (?? selected)".Loc(selectedPlan.EntrustItems.Count) + "###eitems", () =>
            {
                InventoryManagementCommon.DrawListNew(
                    itemId => selectedPlan.EntrustItems.Add(itemId), 
                    itemId => selectedPlan.EntrustItems.Remove(itemId), 
                    selectedPlan.EntrustItems, (x) =>
                {
                    var amount = selectedPlan.EntrustItemsAmountToKeep.SafeSelect(x);
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(130f);
                    if(ImGui.InputInt($"##amtkeepitem{x}", ref amount))
                    {
                        selectedPlan.EntrustItemsAmountToKeep[x] = amount;
                    }
                    ImGuiEx.Tooltip("Amount to keep in your inventory".Loc());
                });
            });
            ImGuiEx.TreeNodeCollapsingHeader("Fast addition/removal".Loc(), () =>
            {
                ImGuiEx.TextWrapped(GradientColor.Get(EColor.RedBright, EColor.YellowBright), "While this text is visible, hover over items while holding:".Loc());
                ImGuiEx.Text(!ImGui.GetIO().KeyShift ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, "Shift - add to entrust plan".Loc());
                ImGuiEx.Text(!ImGui.GetIO().KeyAlt ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, "Alt - delete from entrust plan".Loc());
                if(Svc.GameGui.HoveredItem > 0)
                {
                    var id = (uint)(Svc.GameGui.HoveredItem % 1000000);
                    if(ImGui.GetIO().KeyShift)
                    {
                        if(!selectedPlan.EntrustItems.Contains(id))
                        {
                            selectedPlan.EntrustItems.Add(id);
                            Notify.Success("Added ?? to entrust plan ??".Loc(ExcelItemHelper.GetName(id), selectedPlan.Name));
                        }
                    }
                    if(ImGui.GetIO().KeyAlt)
                    {
                        if(selectedPlan.EntrustItems.Contains(id))
                        {
                            selectedPlan.EntrustItems.Remove(id);
                            Notify.Success("Removed ?? from entrust plan ??".Loc(ExcelItemHelper.GetName(id), selectedPlan.Name));
                        }
                    }
                }
            });
        }
    }
}
