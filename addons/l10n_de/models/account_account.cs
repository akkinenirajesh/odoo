csharp
public partial class AccountAccount
{
    public override bool Write(Dictionary<string, object> vals)
    {
        if (this.Company.AccountFiscalCountry.Code.Contains("DE") && (vals.ContainsKey("Code") || vals.ContainsKey("Name")))
        {
            var hashedAmlDomain = new List<(string, string, object)>
            {
                ("AccountId", "in", new[] { this.Id }),
                ("MoveId.InalterableHash", "!=", null)
            };

            if (Env.Get<AccountMoveLine>().SearchCount(hashedAmlDomain, limit: 1) > 0)
            {
                throw new UserException("You cannot change the code/name of an account if it contains hashed entries.");
            }
        }

        return base.Write(vals);
    }
}
