namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;
public abstract class DebugSectionBase : NeoUIEntry
{
    public override string Path => "Advanced".Loc() + "/" + "Debug".Loc() + "/" + GetType().Name.Replace("Debug", "").Loc();
    public override bool ShouldDisplay()
    {
        return C.Verbose;
    }
}
