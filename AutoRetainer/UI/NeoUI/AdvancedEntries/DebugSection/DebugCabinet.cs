using ECommons.ExcelServices;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Text;
using Cabinet = Lumina.Excel.Sheets.Cabinet;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

public unsafe class DebugCabinet : DebugSectionBase
{
    public override void Draw()
    {
        var state = UIState.Instance()->Cabinet.State;
        ImGuiEx.Text($"State: {state}");
        foreach(var x in Cabinet.Values)
        {
            if(x.Item.RowId != 0)
            {
                ImGuiEx.Text(UIState.Instance()->Cabinet.IsItemInCabinet(x.RowId)?EColor.GreenBright:null, $"{ExcelItemHelper.GetName(x.Item.RowId, true)}");
            }
        }
    }
}
