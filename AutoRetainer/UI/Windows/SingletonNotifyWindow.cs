namespace AutoRetainer.UI.Windows;
public class SingletonNotifyWindow : NotifyWindow
{
    private bool IAmIdiot = false;
    private WindowSystem ws;
    public SingletonNotifyWindow() : base("AutoRetainer - warning!".Loc())
    {
        IsOpen = true;
        ws = new();
        Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
        ws.AddWindow(this);
    }

    public override void OnClose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= ws.Draw;
    }

    public override void DrawContent()
    {
        ImGuiEx.Text("AutoRetainer has detected that another instance of the plugin is running with the same data path configuration.".Loc());
        ImGuiEx.Text("Plugin load has been halted in order to prevent data loss.".Loc());
        if(ImGui.Button("Close this window without loading AutoRetainer".Loc()))
        {
            IsOpen = false;
        }
        if(ImGui.Button("Learn how to properly run 2 or more game instances".Loc()))
        {
            ShellStart("https://github.com/PunishXIV/AutoRetainer/issues/62");
        }
        ImGui.Separator();
        ImGui.Checkbox("I agree that I may lose all AutoRetainer data".Loc(), ref IAmIdiot);
        if(!IAmIdiot) ImGui.BeginDisabled();
        if(ImGui.Button("Load AutoRetainer".Loc()))
        {
            IsOpen = false;
            new TickScheduler(P.Load);
        }
        if(!IAmIdiot) ImGui.EndDisabled();
    }
}
