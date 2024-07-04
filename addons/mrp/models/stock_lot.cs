csharp
public partial class Mrp_StockLot
{
    public void _CheckCreate()
    {
        int activeMoId = Env.Context.Get("ActiveMoId");
        if (activeMoId != 0)
        {
            var activeMo = Env.Ref<Mrp_Production>(activeMoId);
            if (!activeMo.PickingTypeId.UseCreateComponentsLots)
            {
                throw new UserError("You are not allowed to create or edit a lot or serial number for the components with the operation type \"Manufacturing\". To change this, go on the operation type and tick the box \"Create New Lots/Serial Numbers for Components\".");
            }
        }
    }
}
