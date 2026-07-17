using AutoRetainerAPI.Configuration;
using ECommons.GameHelpers;

namespace AutoRetainer.UI.Statistics;
public sealed class FcDataManager
{
    private FcDataManager() { }

    public void Draw()
    {
        ImGui.Checkbox("Update every 30 hours".Loc(), ref C.UpdateStaleFCData);
        ImGui.SameLine();
        if(ImGuiEx.Button("Update".Loc(), Player.Interactable))
        {
            S.FCPointsUpdater.ScheduleUpdateIfNeeded(true);
        }
        ImGui.SameLine();
        ImGui.Checkbox("Show only wallet FC".Loc(), ref C.DisplayOnlyWalletFC);
        if(ImGui.BeginTable("FCData", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("Name".Loc(), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Characters".Loc());
            ImGui.TableSetupColumn("Gil".Loc());
            ImGui.TableSetupColumn("FC points".Loc());
            ImGui.TableSetupColumn($"##control");
            ImGui.TableHeadersRow();

            var totalGil = 0L;
            var totalPoint = 0L;

            var i = 0;
            foreach(var x in C.FCData)
            {
                if(x.Key == 0) continue;
                if(!x.Value.GilCountsTowardsChara && C.DisplayOnlyWalletFC) continue;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGuiEx.TextV(C.NoNames ? "Free company ??".Loc(++i) : x.Value.Name);

                ImGui.TableNextColumn();
                foreach(var c in C.OfflineData.Where(z => z.FCID == x.Key))
                {
                    ImGuiEx.Text(x.Value.HolderChara == c.CID && x.Value.GilCountsTowardsChara ? EColor.GreenBright : null, Censor.Character(c.Name, c.World));
                    if(ImGuiEx.HoveredAndClicked("Left click - Relog to this character".Loc()))
                    {
                        Svc.Commands.ProcessCommand($"/ays relog {c.Name}@{c.World}");
                    }
                    if(x.Value.GilCountsTowardsChara)
                    {
                        if(ImGuiEx.HoveredAndClicked("Right click - set as gil holder".Loc(), ImGuiMouseButton.Right))
                        {
                            x.Value.HolderChara = c.CID;
                        }
                    }
                }

                ImGui.TableNextColumn();
                if(x.Value.LastGilUpdate != -1 && x.Value.LastGilUpdate != 0)
                {
                    ImGuiEx.Text($"{x.Value.Gil:N0}");
                    totalGil += x.Value.Gil;
                    ImGuiEx.Tooltip("Last updated ??. Ctrl + click to reset".Loc(UpdatedWhen(x.Value.LastGilUpdate)));
                    if(ImGuiEx.HoveredAndClicked() && ImGuiEx.Ctrl)
                    {
                        x.Value.LastGilUpdate = -1;
                        x.Value.Gil = 0;
                    }
                }
                else
                {
                    ImGuiEx.Text("Unknown".Loc());
                }

                ImGui.TableNextColumn();
                if(x.Value.FCPointsLastUpdate != 0)
                {
                    ImGuiEx.Text($"{x.Value.FCPoints:N0}");
                    totalPoint += x.Value.FCPoints;
                    ImGuiEx.Tooltip("Last updated ??".Loc(UpdatedWhen(x.Value.FCPointsLastUpdate)));
                }
                else
                {
                    ImGuiEx.Text("Unknown".Loc());
                }

                ImGui.TableNextColumn();
                ImGui.PushFont(UiBuilder.IconFont);
                ImGuiEx.ButtonCheckbox($"\uf555##FC{x.Key}", ref x.Value.GilCountsTowardsChara, EColor.Green);
                ImGui.PopFont();
                ImGuiEx.Tooltip("Mark this free company as Wallet FC. Gil Display tab will include money of this FC.".Loc());
                ImGui.SameLine();
                if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, $"{x.Key}Dele", enabled: ImGuiEx.Ctrl))
                {
                    new TickScheduler(() => C.FCData.Remove(x));
                }

                ImGuiEx.Tooltip("Hold CTRL and click to delete this FC. Note that if you will relog to that FC, it will appear again.".Loc());
            }

            ImGui.TableNextRow();
            ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, EColor.GreenDark.ToUint());
            ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, EColor.GreenDark.ToUint());
            ImGui.TableNextColumn();
            ImGuiEx.Text("TOTAL".Loc());
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGuiEx.Text($"{totalGil:N0}");
            ImGui.TableNextColumn();
            ImGuiEx.Text($"{totalPoint:N0}");

            ImGui.EndTable();
        }


        string UpdatedWhen(long time)
        {
            var diff = DateTimeOffset.Now.ToUnixTimeMilliseconds() - time;
            if(diff < 1000L * 60) return "just now".Loc();
            if(diff < 1000L * 60 * 60) return "?? minute(s) ago".Loc((int)(diff / 1000 / 60));
            if(diff < 1000L * 60 * 60 * 60) return "?? hour(s) ago".Loc((int)(diff / 1000 / 60 / 60));
            return "?? day(s) ago".Loc((int)(diff / 1000 / 60 / 60 / 24));
        }
    }

    public OfflineCharacterData GetHolderChara(ulong fcid, FCData data)
    {
        if(C.OfflineData.TryGetFirst(x => x.FCID == fcid && x.CID == data.HolderChara, out var chara))
        {
            return chara;
        }
        else if(C.OfflineData.TryGetFirst(x => x.FCID == fcid, out var fchara))
        {
            data.HolderChara = fchara.CID;
            return fchara;
        }
        return null;
    }
}
