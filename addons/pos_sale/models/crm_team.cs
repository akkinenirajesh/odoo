csharp
public partial class PosSale.CrmTeam
{
    public int PosSessionsOpenCount { get; set; }
    public double PosOrderAmountTotal { get; set; }

    public void ComputePosSessionsOpenCount()
    {
        this.PosSessionsOpenCount = Env.SearchCount<Pos.PosSession>(x => x.ConfigId.CrmTeamId == this.Id && x.State == "opened");
    }

    public void ComputePosOrderAmountTotal()
    {
        var data = Env.ReadGroup<Pos.PosOrder>(
            new[] {
                new SearchDomain("Session.State", "=", "opened"),
                new SearchDomain("Config.CrmTeamId", "in", new[] { this.Id })
            },
            new[] { "ConfigId" },
            new[] { "PriceTotal:sum" }
        );

        var rgResults = data.ToDictionary(x => x.ConfigId.Id, x => x.PriceTotalSum);

        this.PosOrderAmountTotal = this.PosConfigIds.Sum(config => rgResults.GetValueOrDefault(config.Id, 0.0));
    }
}
