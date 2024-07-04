csharp
public partial class WebsiteResCompany {

    public Dictionary<string, object> GetDefaultPricelistVals()
    {
        var values = Env.Call("Website.ResCompany", "_GetDefaultPricelistVals", this);
        values["WebsiteId"] = null;
        return values;
    }
}
