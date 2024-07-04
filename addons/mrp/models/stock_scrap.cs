csharp
public partial class Mrp.StockScrap
{
    public void ComputeLocationId()
    {
        if (this.ProductionId != null)
        {
            if (this.ProductionId.State != "done")
            {
                this.LocationId = this.ProductionId.LocationSrcId;
            }
            else
            {
                this.LocationId = this.ProductionId.LocationDestId;
            }
        }
        else if (this.WorkorderId != null)
        {
            this.LocationId = this.WorkorderId.ProductionId.LocationSrcId;
        }
        else
        {
            // Call super class method
        }
    }

    public Dictionary<string, object> PrepareMoveValues()
    {
        Dictionary<string, object> vals = new Dictionary<string, object>();

        if (this.ProductionId != null)
        {
            vals.Add("Origin", this.ProductionId.Name);
            if (this.ProductionId.MoveFinishedIds.Any(x => x.ProductId == this.ProductId))
            {
                vals.Add("ProductionId", this.ProductionId.Id);
            }
            else
            {
                vals.Add("RawMaterialProductionId", this.ProductionId.Id);
            }
        }
        // Call super class method to complete vals
        return vals;
    }

    public void OnchangeSerialNumber()
    {
        if (this.ProductId.Tracking == "serial" && this.LotId != null)
        {
            if (this.ProductionId != null)
            {
                string message;
                Stock.Location recommendedLocation;
                (message, recommendedLocation) = Env.Get("stock.quant")._CheckSerialNumber(this.ProductId, this.LotId, this.CompanyId, this.LocationId, this.ProductionId.LocationDestId);
                if (message != null)
                {
                    if (recommendedLocation != null)
                    {
                        this.LocationId = recommendedLocation;
                    }
                    // return {'warning': {'title': _('Warning'), 'message': message}} - you will need to handle this based on your UI library
                }
            }
            else
            {
                // Call super class method
            }
        }
    }

    public void ComputeScrapQty()
    {
        if (this.BomId == null)
        {
            // Call super class method
            return;
        }
        if (this.MoveIds != null)
        {
            this.ScrapQty = this.MoveIds._ComputeKitQuantities(this.ProductId, this.ScrapQty, this.BomId, new Dictionary<string, Func<Stock.Move, bool>> {
                { "incoming_moves", m => true },
                { "outgoing_moves", m => false }
            });
        }
    }

    public bool ShouldCheckAvailableQty()
    {
        return (base.ShouldCheckAvailableQty() || this.ProductIsKit);
    }

    public void DoReplenish(Dictionary<string, object> values = null)
    {
        if (values == null)
        {
            values = new Dictionary<string, object>();
        }
        if (this.ProductionId != null && this.ProductionId.ProcurementGroupId != null)
        {
            values.Add("GroupId", this.ProductionId.ProcurementGroupId.Id);
            values.Add("MoveDestIds", this.ProductionId.ProcurementGroupId.StockMoveIds.Where(m => m.LocationId == this.LocationId && m.ProductId == this.ProductId && m.State != "assigned" && m.State != "done" && m.State != "cancel"));
        }
        // Call super class method to complete replenishment
    }
}
