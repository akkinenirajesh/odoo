csharp
public partial class Mrp.StockMoveLine {
    public void OnChangeSerialNumber()
    {
        var currentLocationId = this.LocationId;
        var res = Env.Call("stock.stock_move_line", "_onchange_serial_number");
        if (res != null && string.IsNullOrEmpty(this.LotName) && currentLocationId.IsSubcontractingLocation)
        {
            this.LocationId = currentLocationId;
            var message = (string)res["warning"]["message"];
            message = message.Split("\n\n", 2)[0] + "\n\n" + Env.Translate("Make sure you validate or adapt the related resupply picking to your subcontractor in order to avoid inconsistencies in your stock.");
            res["warning"]["message"] = message;
        }
    }

    public void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("LotId") && this.MoveId.IsSubcontract && this.LocationId.IsSubcontractingLocation)
        {
            var subcontractedProduction = this.MoveId.GetSubcontractProduction().Where(p => p.State != "done" && p.State != "cancel" && p.LotProducingId == this.LotId).ToList();
            if (subcontractedProduction.Any())
            {
                subcontractedProduction.First().LotProducingId = (int)vals["LotId"];
            }
        }
        Env.Call("stock.stock_move_line", "write", this, vals);
    }
}
