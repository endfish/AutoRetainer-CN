using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.MainWindow.MultiModeTab;
public class CharaConfig
{
    public static void Draw(OfflineCharacterData data, bool isRetainer)
    {
        ImGui.PushID(data.CID.ToString());
        SharedUI.DrawMultiModeHeader(data);
        var b = new NuiBuilder()

        .Section("General Character Specific Settings".Loc())
        .Widget(() =>
        {
            SharedUI.DrawServiceAccSelector(data);
            SharedUI.DrawPreferredCharacterUI(data);
        });
        if(isRetainer)
        {
            b = b.Section("Retainers".Loc()).Widget(() =>
            {
                ImGuiEx.Text("Automatic Grand Company Expert Delivery:".Loc());
                if(!AutoGCHandin.Operation)
                {
                    ImGuiEx.SetNextItemWidthScaled(200f);
                    ImGuiEx.EnumCombo("##gcHandin", ref data.GCDeliveryType);
                }
                else
                {
                    ImGuiEx.Text("Can't change this now".Loc());
                }
            });
        }
        else
        {
            b = b.Section("Deployables".Loc()).Widget(() =>
            {
                ImGui.Checkbox("Wait For Voyage Completion".Loc(), ref data.MultiWaitForAllDeployables);
                ImGuiComponents.HelpMarker("This setting works like the global option but applies to individual characters. When enabled, AutoRetainer will wait for all deployables to return before logging into the character. If you're already logged in for another reason, it will still resend completed submarines—unless the global setting \"Wait even when already logged in\" is also turned on.".Loc());
            });
        }
        b = b.Section("Teleport overrides".Loc(), data.GetAreTeleportSettingsOverriden() ? ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg] with { X = 1f } : null, true)
        .Widget(() =>
        {
            ImGuiEx.Text("You can override teleport settings per character.".Loc());
            bool? demo = null;
            ImGuiEx.Checkbox("Options marked with this marker will use values from global configuration".Loc(), ref demo);
            ImGuiEx.Checkbox("Enabled".Loc(), ref data.TeleportOptionsOverride.Enabled);
            ImGui.Indent();
            ImGuiEx.Checkbox("Teleport for retainers...".Loc(), ref data.TeleportOptionsOverride.Retainers);
            ImGui.Indent();
            ImGuiEx.Checkbox("...to private house".Loc(), ref data.TeleportOptionsOverride.RetainersPrivate);
            ImGuiEx.Checkbox("...to shared house".Loc(), ref data.TeleportOptionsOverride.RetainersShared);
            ImGuiEx.Checkbox("...to free company house".Loc(), ref data.TeleportOptionsOverride.RetainersFC);
            ImGuiEx.Checkbox("...to apartment".Loc(), ref data.TeleportOptionsOverride.RetainersApartment);
            ImGui.Text("If all above are disabled or fail, will be teleported to inn.".Loc());
            ImGui.Unindent();
            ImGuiEx.Checkbox("Teleport to free company house for deployables".Loc(), ref data.TeleportOptionsOverride.Deployables);
            ImGui.Unindent(); 
        }).Draw();
        SharedUI.DrawExcludeReset(data);
        ImGui.PopID();
    }
}
