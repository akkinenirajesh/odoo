csharp
public partial class Lead
{
    public object FormViewAutoFill()
    {
        // Note: This method is marked as deprecated in the original Python code
        return new
        {
            Type = "ir.actions.act_window",
            ViewMode = "form",
            ResModel = "Crm.Lead",
            Context = new
            {
                DefaultPartnerId = Env.Context.GetValueOrDefault("params", new Dictionary<string, object>())
                    .GetValueOrDefault("partner_id")
            }
        };
    }
}
