csharp
public partial class RepairProduct
{
    public virtual List<RepairProduct> _countReturnedSnProductsDomain(string snLot, List<List<object>> orDomains)
    {
        orDomains.Add(new List<object>() {
            new Dictionary<string, object>() { { "MoveId.RepairLineType", "in" }, new List<string>() { "remove", "recycle" } },
            new Dictionary<string, object>() { { "LocationDestUsage", "=" }, "internal" }
        });
        return Env.Call("Repair.Product", "_countReturnedSnProductsDomain", snLot, orDomains);
    }
}
