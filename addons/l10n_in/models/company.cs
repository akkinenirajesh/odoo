csharp
public partial class ResCompany
{
    public override ResCompany Create(Dictionary<string, object> vals)
    {
        var res = base.Create(vals);
        // Update Fiscal Positions for new branch
        res.UpdateL10nInFiscalPosition();
        return res;
    }

    public override bool Write(Dictionary<string, object> vals)
    {
        var res = base.Write(vals);
        if ((vals.ContainsKey("StateId") || vals.ContainsKey("CountryId")) && !Env.Context.ContainsKey("delay_account_group_sync"))
        {
            // Update Fiscal Positions for companies setting up state for the first time
            UpdateL10nInFiscalPosition();
        }
        return res;
    }

    private void UpdateL10nInFiscalPosition()
    {
        if (ParentIds.FirstOrDefault()?.ChartTemplate == "in")
        {
            var chartTemplate = Env.Get<AccountChartTemplate>().WithCompany(this);
            var fiscalPositionData = chartTemplate.GetInAccountFiscalPosition();
            chartTemplate.LoadData(new Dictionary<string, object> { { "AccountFiscalPosition", fiscalPositionData } });
        }
    }
}
