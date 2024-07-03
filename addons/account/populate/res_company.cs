csharp
public partial class ResCompany
{
    public override string ToString()
    {
        return Name;
    }

    public void Populate(int size)
    {
        Env.Logger.Info("Loading Chart Template");
        
        // Assuming a method to create records exists in the base class or a helper
        var records = base.Populate(size);

        // Load the chart of accounts for the first 3 companies
        for (int i = 0; i < Math.Min(3, records.Count); i++)
        {
            var company = records[i];
            Env.Get<AccountChartTemplate>().TryLoading(company: company, templateCode: null);
        }

        return records;
    }
}
