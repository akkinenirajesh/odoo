csharp
public partial class PickingType
{
    public string GetDefaultWeightUom()
    {
        return Env.Get<ProductTemplate>().GetWeightUomNameFromIrConfigParameter();
    }

    public void ComputeWeightUomName()
    {
        WeightUomName = Env.Get<ProductTemplate>().GetWeightUomNameFromIrConfigParameter();
    }

    public List<string> GetBatchGroupByKeys()
    {
        var keys = base.GetBatchGroupByKeys();
        keys.Add("BatchGroupByCarrier");
        return keys;
    }
}

public partial class Picking
{
    public List<object> GetPossiblePickingsDomain()
    {
        var domain = base.GetPossiblePickingsDomain();
        if (PickingType.BatchGroupByCarrier)
        {
            domain = Env.Expression.AND(domain, new List<object> { new List<object> { "CarrierId", "=", Carrier?.Id ?? false } });
        }
        return domain;
    }

    public List<object> GetPossibleBatchesDomain()
    {
        var domain = base.GetPossibleBatchesDomain();
        if (PickingType.BatchGroupByCarrier)
        {
            domain = Env.Expression.AND(domain, new List<object> { new List<object> { "PickingIds.CarrierId", "=", Carrier?.Id ?? false } });
        }
        return domain;
    }

    public bool IsAutoBatchable(Picking picking = null)
    {
        bool res = base.IsAutoBatchable(picking);
        picking = picking ?? Env.Get<Picking>();
        if (PickingType.BatchMaxWeight > 0)
        {
            res = res && (Weight + picking.Weight <= PickingType.BatchMaxWeight);
        }
        return res;
    }
}
