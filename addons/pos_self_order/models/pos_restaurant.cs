csharp
public partial class PosSelfOrder.RestaurantTable
{
    public string GetIdentifier()
    {
        return System.Guid.NewGuid().ToString().Substring(0, 8);
    }

    public void UpdateIdentifier()
    {
        var tables = Env.GetModel("PosSelfOrder.RestaurantTable").Search<PosSelfOrder.RestaurantTable>();
        foreach (var table in tables)
        {
            table.Identifier = GetIdentifier();
        }
    }

    public List<string> LoadPosSelfDataFields(int configId)
    {
        return new List<string>() { "Name", "Identifier", "FloorId" };
    }

    public List<object> LoadPosSelfDataDomain(object data)
    {
        var floors = (List<object>)data["restaurant.floor"]["data"];
        var floorIds = floors.Select(f => (int)f["id"]).ToList();
        return new List<object>() { new object[] { "FloorId", "in", floorIds } };
    }
}

public partial class PosSelfOrder.RestaurantFloor
{
    public List<string> LoadPosSelfDataFields(int configId)
    {
        return new List<string>() { "Name", "TableIds" };
    }

    public List<object> LoadPosSelfDataDomain(object data)
    {
        var configData = (List<object>)data["pos.config"]["data"];
        var floorIds = (List<int>)configData[0]["floor_ids"];
        return new List<object>() { new object[] { "Id", "in", floorIds } };
    }
}
