csharp
public partial class Sale_AccountInvoiceReport 
{
    public Sql QuerySelect()
    {
        return new Sql($"%s, move.team_id as team_id", Env.Ref("Account.AccountInvoiceReport").QuerySelect());
    }
}
