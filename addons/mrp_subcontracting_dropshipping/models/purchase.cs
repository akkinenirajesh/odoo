C#
public partial class Mrp.PurchaseOrder
{
    public void ComputeDestAddressId()
    {
        if (this.DefaultLocationDestIdIsSubcontractingLoc)
        {
            var subcontractorIds = Env.Ref<Mrp.Subcontractor>("picking_type_id.default_location_dest_id.subcontractor_ids");
            this.DestAddressId = subcontractorIds.Count == 1 ? subcontractorIds : null;
        }
        else
        {
            // call super method
        }
    }

    public void OnChangePickingTypeId()
    {
        if (this.DefaultLocationDestIdIsSubcontractingLoc)
        {
            Env.NotifyWarning("Warning", "Please note this purchase order is for subcontracting purposes.");
        }
    }

    public int GetDestinationLocation()
    {
        if (this.DefaultLocationDestIdIsSubcontractingLoc)
        {
            return this.DestAddressId.PropertyStockSubcontractor.Id;
        }
        else
        {
            // call super method
        }
    }
}
