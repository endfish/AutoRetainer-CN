using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.GCDeliveryEntries;
public sealed unsafe class GeneralSettings : InventoryManagementBase
{
    public override string Name { get; } = "Grand Company Delivery/General Settings";

    public override NuiBuilder Builder => new NuiBuilder()
        .Section("General Settings".Loc())
        .Checkbox("Enable Expert Delivery continuation".Loc(), () => ref C.AutoGCContinuation)
        .TextWrapped("When Expert Delivery Continuation is enabled:\n- The plugin will automatically spend available Grand Company Seals to purchase items from the configured Exchange List.\n- If the Exchange List is empty, only Ventures will be purchased.\n- Make sure that \"Delivery Mode\" is not set to \"Disabled\" in \"Character Configuration\" section\n\nAfter seals have been spent:\n- Expert Delivery will resume automatically.\n- The process will repeat until there are no eligible items left to deliver or no seals remaining.".Loc())

        .Section("Multi Mode Expert Delivery".Loc())
        .TextWrapped("When enabled:\n- Characters with teleportation enabled will automatically deliver items for expert delivery and buy items according to exchange plan, if their rank is sufficient, during multi mode.".Loc())
        .Checkbox("Enable Multi Mode Expert Delivery".Loc(), () => ref C.FullAutoGCDelivery)
        .Checkbox("Only when workstation is not locked".Loc(), () => ref C.FullAutoGCDeliveryOnlyWsUnlocked)
        .InputInt(150f, "Inventory slots remaining to trigger delivery, less or equal".Loc(), () => ref C.FullAutoGCDeliveryInventory, "Only primary inventory is accounted for, not armory".Loc())
        .Checkbox("Trigger on venture exhaustion".Loc(), () => ref C.FullAutoGCDeliveryDeliverOnVentureExhaust, "This may cause situation where you will just go to GC exchange every login. Make sure you have a purchase plan to buy enough ventures set. ".Loc())
        .Indent()
        .InputInt(150f, "Ventures remaining to trigger delivery, less or equal".Loc(), () => ref C.FullAutoGCDeliveryDeliverOnVentureLessThan)
        .Unindent()
        .Checkbox("Use Priority seal allowance, if possible".Loc(), () => ref C.FullAutoGCDeliveryUseBuffItem)
        .Checkbox("Use Free Company seal buff, if possible".Loc(), () => ref C.FullAutoGCDeliveryUseBuffFCAction)
        .Checkbox("Teleport back to house/inn after delivery".Loc(), () => ref C.TeleportAfterGCExchange)
        .Indent()
        .Checkbox("Only when Multi Mode is active".Loc(), () => ref C.TeleportAfterGCExchangeMulti)
        .Unindent()
        ;
}
