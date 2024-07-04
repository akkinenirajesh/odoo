csharp
public partial class PosSession
{
    public List<string> LoadPosDataModels(int configId)
    {
        var data = base.LoadPosDataModels(configId);
        if (Env.Company.Country.Code == "AR")
        {
            data.AddRange(new[] { "L10nAr.AfipResponsibilityType", "L10nLatam.IdentificationType" });
        }
        return data;
    }

    public Dictionary<string, object> LoadPosData(Dictionary<string, object> data)
    {
        data = base.LoadPosData(data);
        if (Env.Company.Country.Code == "AR")
        {
            var dataList = (List<Dictionary<string, object>>)data["data"];
            if (dataList.Count > 0)
            {
                dataList[0]["ConsumidorFinalAnonimoId"] = Env.Ref("L10nAr.ParCfa").Id;
            }
        }
        return data;
    }
}
