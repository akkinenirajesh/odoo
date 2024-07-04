csharp
public partial class AccountFiscalPosition
{
    public IEnumerable<AfipResponsibilityType> GetFposRankingFunctions(Core.Partner partner)
    {
        if (Env.Company.Country?.Code != "AR")
        {
            return base.GetFposRankingFunctions(partner);
        }

        var additionalFunctions = new List<Func<AccountFiscalPosition, bool>>
        {
            fpos => partner.AfipResponsibilityType != null && 
                    fpos.AfipResponsibilityTypes.Contains(partner.AfipResponsibilityType)
        };

        return additionalFunctions.Concat(base.GetFposRankingFunctions(partner));
    }
}
