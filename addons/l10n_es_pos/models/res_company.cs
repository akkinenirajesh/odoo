csharp
public partial class ResCompany
{
    public List<string> LoadPosDataFields(int configId)
    {
        var parameters = base.LoadPosDataFields(configId);
        
        if (Env.Company.Country?.Code == "ES")
        {
            parameters.AddRange(new[] { "Street", "City", "Zip" });
        }
        
        return parameters;
    }
}
