using ECommons.ExcelServices;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeLockout : NeoUIEntry
{
    public override string Path => "Multi Mode/Region Lock".Loc();

    private int Num = 12;

    public override void Draw()
    {
        ImGuiEx.TextV("For".Loc());
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150f);
        ImGui.InputInt("hours...".Loc(), ref Num.ValidateRange(1, 10000));
        foreach(var x in Enum.GetValues<ExcelWorldHelper.Region>())
        {
            if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Lock, "...do not log into ?? region".Loc(x)))
            {
                C.LockoutTime[x] = DateTimeOffset.Now.ToUnixTimeSeconds() + Num * 60 * 60;
            }
        }
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Unlock, "Remove all locks".Loc()))
        {
            C.LockoutTime.Clear();
        }
    }
}
