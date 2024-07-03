csharp
public partial class AccountMove
{
    public void Post(bool soft = true)
    {
        var vendorBillService = Env.Ref("account_fleet.data_fleet_service_type_vendor_bill", raiseIfNotFound: false);
        if (vendorBillService == null)
        {
            return base.Post(soft);
        }

        var valList = new List<Dictionary<string, object>>();
        var logList = new List<string>();
        var posted = base.Post(soft);

        foreach (var line in posted.LineIds)
        {
            if (line.VehicleId == null || line.VehicleLogServiceIds.Any() ||
                line.Move.MoveType != "in_invoice" || line.DisplayType != "product")
            {
                continue;
            }
            var val = line.PrepareFleetLogService();
            var log = $"Service Vendor Bill: {line.Move.GetHtmlLink()}";
            valList.Add(val);
            logList.Add(log);
        }

        var logServiceIds = Env.Get<FleetVehicleLogServices>().Create(valList);
        for (int i = 0; i < logServiceIds.Count; i++)
        {
            logServiceIds[i].MessagePost(body: logList[i]);
        }

        return posted;
    }
}

public partial class AccountMoveLine
{
    public void ComputeNeedVehicle()
    {
        NeedVehicle = false;
    }

    public Dictionary<string, object> PrepareFleetLogService()
    {
        var vendorBillService = Env.Ref("account_fleet.data_fleet_service_type_vendor_bill", raiseIfNotFound: false);
        return new Dictionary<string, object>
        {
            ["ServiceTypeId"] = vendorBillService.Id,
            ["VehicleId"] = VehicleId.Id,
            ["VendorId"] = PartnerId.Id,
            ["Description"] = Name,
            ["AccountMoveLineId"] = Id
        };
    }

    public override void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("VehicleId") && vals["VehicleId"] == null)
        {
            VehicleLogServiceIds.WithContext(new { ignore_linked_bill_constraint = true }).Unlink();
        }
        base.Write(vals);
    }

    public override void Unlink()
    {
        VehicleLogServiceIds.WithContext(new { ignore_linked_bill_constraint = true }).Unlink();
        base.Unlink();
    }
}
