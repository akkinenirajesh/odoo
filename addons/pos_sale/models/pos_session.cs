csharp
public partial class PosSession 
{
    public virtual Crm.Team CrmTeamId { get; set; }

    public virtual List<Sale.Order> LoadPosDataModels(Pos.Config ConfigId)
    {
        var data = base.LoadPosDataModels(ConfigId);
        data.AddRange(new List<string>() { "Sale.Order", "Sale.OrderLine" });
        return data;
    }
}
