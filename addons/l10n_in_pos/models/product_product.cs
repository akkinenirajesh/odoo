csharp
public partial class ProductProduct
{
    public List<string> LoadPosDataFields(int configId)
    {
        var fields = base.LoadPosDataFields(configId);
        if (Env.Company.Country?.Code == "IN")
        {
            fields.Add("L10nInHsnCode");
        }
        return fields;
    }
}
