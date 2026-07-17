namespace AutoRetainer.UI.NeoUI;
public class Keybinds : NeoUIEntry
{
    public override string Path => "Keybinds".Loc();

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Access summoning bell/workshop panel keybinds".Loc())
        .Widget("Temporarily prevents AutoRetainer from being automatically enabled when using a Summoning Bell/Workshop Panel".Loc(), (x) =>
        {
            UIUtils.DrawKeybind(x, ref C.Suppress);
        })
        .Widget("Temporarily set the Collect Operation mode, preventing ventures from being assigned for the current cycle/Temporarily set Deployables mode to Finalize only".Loc(), (x) =>
        {
            UIUtils.DrawKeybind(x, ref C.TempCollectB);
        })

        .Section("Quick Retainer Action".Loc())
        .Widget("Sell Item".Loc(), (x) => UIUtils.QRA(x, ref C.SellKey))
        .Widget("Entrust Item".Loc(), (x) => UIUtils.QRA(x, ref C.EntrustKey))
        .Widget("Retrieve Item".Loc(), (x) => UIUtils.QRA(x, ref C.RetrieveKey))
        .Widget("Put up For Sale".Loc(), (x) => UIUtils.QRA(x, ref C.SellMarketKey));
}
