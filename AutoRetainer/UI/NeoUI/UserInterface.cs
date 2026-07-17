using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommons.Configuration;

namespace AutoRetainer.UI.NeoUI;
public sealed unsafe class UserInterface : NeoUIEntry
{
    public override string Path => "User Interface".Loc();

    private static readonly Dictionary<string, string> LanguageDisplayNames = new()
    {
        ["English"] = "English",
        ["Chinese"] = "简体中文",
    };

    public override NuiBuilder Builder => new NuiBuilder()

        .Section("Language".Loc())
        .Widget(150f, "Plugin display language".Loc(), (x) =>
        {
            var current = C.PluginLanguage;
            if(ImGui.BeginCombo("##pluginlang", LanguageDisplayNames.GetValueOrDefault(current, current)))
            {
                foreach(var language in Localization.GetAvaliableLanguages())
                {
                    if(ImGui.Selectable(LanguageDisplayNames.GetValueOrDefault(language, language), language == current) && language != current)
                    {
                        C.PluginLanguage = language;
                        Localization.Init(language == "English" ? null : language);
                        EzConfig.Save();
                        S.NeoWindow.Reload();
                    }
                }
                ImGui.EndCombo();
            }
        }, "Some texts are cached and will only update after the plugin is reloaded.".Loc())

        .Section("User Interface".Loc())
        .Checkbox("Anonymise Retainers".Loc(), () => ref C.NoNames, "Retainer names will be redacted from general UI elements. They will not be hidden in debug menus and plugin logs however. While this option is on, character and retainer numbers are not guaranteed to be equal in different sections of a plugin (for example, retainer 1 in retainers view is not guaranteed to be the same retainer as in statistics view).".Loc())
        .Checkbox("Display Quick Menu in Retainer UI".Loc(), () => ref C.UIBar)
        .Checkbox("Display Extended Retainer Info".Loc(), () => ref C.ShowAdditionalInfo, "Displays retainer item level/gathering/perception and the name of their current venture in the main UI.".Loc())
        .Widget("Do not close AutoRetainer windows on ESC key press".Loc(), (x) =>
        {
            if(ImGui.Checkbox(x, ref C.IgnoreEsc)) Utils.ResetEscIgnoreByWindows();
        })
        .Checkbox("Display only most significant icon in status bar".Loc(), () => ref C.StatusBarMSI)
        .SliderInt(120f, "Status bar icon size".Loc(), () => ref C.StatusBarIconWidth, 32, 128)
        .Checkbox("Open AutoRetainer window on game start".Loc(), () => ref C.DisplayOnStart)
        //.Checkbox("Skip item sell/trade confirmation while plugin is active", () => ref C.SkipItemConfirmations)
        .Checkbox("Enable title screen button (requires plugin restart)".Loc(), () => ref C.UseTitleScreenButton)
        .Checkbox("Hide character search".Loc(), () => ref C.NoCharaSearch)
        .Checkbox("Don't flash background of characters that are complete".Loc(), () => ref C.NoGradient)
        .Checkbox("Do not warn about second game instance running from same directory".Loc(), () => ref C.No2ndInstanceNotify, "This will automatically skip AutoRetainer's loading on second instance of the game and you will have no way of loading it until you disable this option in primary instance".Loc())

        .Section("Character sorting in Retainer tab".Loc())
        .Checkbox("Enable".Loc(), () => ref C.EnableRetainerSort)
        .TextWrapped("This is purely visual order and does not affects character processing in any way.".Loc())
        .Widget(() => UIUtils.DrawSortableEnumList("rorder", C.RetainersVisualOrders))

        .Section("Character sorting in Deployables tab".Loc())
        .Checkbox("Enable".Loc(), () => ref C.EnableDeployablesSort)
        .TextWrapped("This is purely visual order and does not affects character processing in any way.".Loc())
        .Widget(() => UIUtils.DrawSortableEnumList("dorder", C.DeployablesVisualOrders));



}
