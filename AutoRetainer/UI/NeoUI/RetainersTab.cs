using AutoRetainerAPI.Configuration;
using ECommons.MathHelpers;
using RetainerDescriptor = (ulong CID, string RetainerName);

namespace AutoRetainer.UI.NeoUI;
public class RetainersTab : NeoUIEntry
{
    public override string Path => "Retainers".Loc();
    private int MassMinLevel = 0;
    private int MassMaxLevel = 100;
    private VenturePlan SelectedVenturePlan;
    private EntrustPlan SelectedEntrustPlan;
    private HashSet<RetainerDescriptor> SelectedRetainers = [];

    public override NuiBuilder Builder { get; init; }

    public RetainersTab()
    {
        Builder = new NuiBuilder()
                 .Section("Mass configuration change".Loc())
                 .Widget(MassConfigurationChangeWidget);
    }

    private void MassConfigurationChangeWidget()
    {
        ImGuiEx.Text("Select retainers:".Loc());
        ImGuiEx.SetNextItemFullWidth();
        if(ImGui.BeginCombo("##sel", "Selected ??".Loc(SelectedRetainers.Count), ImGuiComboFlags.HeightLarge))
        {
            ref var search = ref Ref<string>.Get("Search");
            ImGui.InputTextWithHint("##searchRetainers", "Character search".Loc(), ref search, 100);
            foreach(var x in C.OfflineData)
            {
                if((search.Length > 0 && !(x.Name + "@" + x.World).Contains(search, StringComparison.OrdinalIgnoreCase)) || x.RetainerData.Count <= 0)
                {
                    continue;
                }
                ImGui.PushID(x.CID.ToString());
                ImGuiEx.CollectionCheckbox(Censor.Character(x.Name, x.World), x.RetainerData.Select(r => (x.CID, r.Name)), SelectedRetainers);
                ImGui.Indent();
                foreach(var r in x.RetainerData)
                {
                    ImGuiEx.CollectionCheckbox(Censor.Retainer(r.Name), (x.CID, r.Name), SelectedRetainers);
                }
                ImGui.Unindent();
                ImGui.PopID();
            }
            ImGui.EndCombo();
        }
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61527, "Deselect All".Loc()))
        {
            SelectedRetainers.Clear();
        }
        ImGui.SameLine();
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61525, "Select All".Loc()))
        {
            SelectedRetainers.Clear();
            foreach(var x in C.OfflineData)
            {
                foreach(var v in x.RetainerData)
                {
                    SelectedRetainers.Add((x.CID, v.Name));
                }
            }
        }

        ImGui.Separator();

        ImGuiEx.TextV("By level:".Loc());
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        ImGui.DragInt("##minlevel", ref MassMinLevel, 0.1f);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        ImGui.DragInt("##maxlevel", ref MassMaxLevel, 0.1f);
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61543, "Add retainers by level to selection".Loc()))
        {
            foreach(var x in C.OfflineData)
            {
                foreach(var r in x.RetainerData)
                {
                    if(r.Level.InRange(MassMinLevel, MassMaxLevel, includeEnd: true))
                    {
                        SelectedRetainers.Add((x.CID, r.Name));
                    }
                }
            }
        }

        ImGui.Separator();

        ImGuiEx.Text("Actions:".Loc());
        ImGui.Separator();
        ImGui.SetNextItemWidth(150f);
        if(ImGui.BeginCombo("##ventureplans", SelectedVenturePlan?.Name ?? "None selected".Loc(), (ImGuiComboFlags)8))
        {
            foreach(var plan in C.SavedPlans)
            {
                if(ImGui.Selectable(plan.Name + "##" + plan.GUID))
                {
                    SelectedVenturePlan = plan;
                }
            }
            ImGui.EndCombo();
        }
        ImGui.SameLine();
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)62073, "Enable planner with venture plan".Loc()))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null && SelectedVenturePlan != null)
                {
                    var adata = Utils.GetAdditionalData(x.CID, x.RetainerName);
                    adata.VenturePlan = SelectedVenturePlan;
                    //adata.VenturePlanIndex = (uint)(C.SavedPlans.IndexOf(SelectedVenturePlan) + 1);
                    adata.EnablePlanner = true;
                    num++;
                }
            }
            Notify.Success("Affected ?? retainers".Loc(num));
        }

        ImGui.Separator();

        ImGui.SetNextItemWidth(150f);
        if(ImGui.BeginCombo("##entrustplans", SelectedEntrustPlan?.Name ?? "None selected".Loc(), ImGuiComboFlags.HeightLarge))
        {
            foreach(var plan in C.EntrustPlans)
            {
                if(ImGui.Selectable($"{plan.Name}##{plan.Guid}"))
                {
                    SelectedEntrustPlan = plan;
                }
            }
            ImGui.EndCombo();
        }
        ImGui.SameLine();
        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)62566, "Set entrust plan".Loc()))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null)
                {
                    var adata = Utils.GetAdditionalData(x.CID, x.RetainerName);
                    adata.EntrustPlan = SelectedEntrustPlan.Guid;
                    num++;
                }
            }
            Notify.Success("Affected ?? retainers".Loc(num));
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61526, "Remove entrust plan from selected retainers".Loc()))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null)
                {
                    var adata = Utils.GetAdditionalData(x.CID, x.RetainerName);
                    adata.EntrustPlan = Guid.Empty;
                    num++;
                }
            }
            Notify.Success("Affected ?? retainers".Loc(num));
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61526, "Disable venture planner for selected retainers".Loc()))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata != null)
                {
                    var adata = Utils.GetAdditionalData(x.CID, x.RetainerName);
                    adata.EnablePlanner = false;
                    num++;
                }
            }
            Notify.Success("Affected ?? retainers".Loc(num));
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61452, "Enable selected retainers".Loc()))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var retainers = P.GetSelectedRetainers(x.CID);
                retainers.Add(x.RetainerName);
                num++;
            }
            Notify.Success("Affected ?? characters".Loc(num));
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61453, "Disable selected retainers".Loc()))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var retainers = P.GetSelectedRetainers(x.CID);
                retainers.Remove(x.RetainerName);
                num++;
            }
            Notify.Success("Affected ?? characters".Loc(num));
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61528, "Enable retainer multi mode for owners of selected retainers".Loc()))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata is { Enabled: false })
                {
                    odata.Enabled = true;
                    num++;
                }
            }
            Notify.Success("Affected ?? characters".Loc(num));
        }

        ImGui.Separator();

        if(ImGuiEx.IconButtonWithText((FontAwesomeIcon)61527, "Disable retainer multi mode for owners of selected retainers".Loc()))
        {
            var num = 0;
            foreach(var x in SelectedRetainers)
            {
                var odata = C.OfflineData.FirstOrDefault(z => z.CID == x.CID);
                if(odata is { Enabled: true })
                {
                    odata.Enabled = false;
                    num++;
                }
            }
            Notify.Success("Affected ?? characters".Loc(num));
        }
    }
}
