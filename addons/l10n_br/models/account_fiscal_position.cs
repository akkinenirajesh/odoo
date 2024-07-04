csharp
public partial class AccountFiscalPosition
{
    private static readonly HashSet<string> SOUTH_SOUTHEAST = new HashSet<string> { "PR", "RS", "SC", "SP", "ES", "MG", "RJ" };
    private static readonly HashSet<string> NORTH_NORTHEAST_MIDWEST = new HashSet<string>
    {
        "AC", "AP", "AM", "PA", "RO", "RR", "TO", "AL", "BA", "CE",
        "MA", "PB", "PE", "PI", "RN", "SE", "DF", "GO", "MT", "MS"
    };

    public AccountFiscalPosition GetFiscalPosition(Core.Partner partner, Core.Partner delivery = null)
    {
        delivery = delivery ?? partner;

        if (Env.Company.Country.Code != "BR" || delivery.Country.Code != "BR")
        {
            // Call the base implementation
            return base.GetFiscalPosition(partner, delivery);
        }

        // Manually set fiscal position on partner has a higher priority
        var manualFiscalPosition = delivery.PropertyAccountPosition ?? partner.PropertyAccountPosition;
        if (manualFiscalPosition != null)
        {
            return manualFiscalPosition;
        }

        // Taxation in Brazil depends on both the state of the partner and the state of the company
        if (Env.Company.State == delivery.State)
        {
            return Env.Search<AccountFiscalPosition>()
                .Where(fp => fp.L10nBrFpType == AccountFiscalPositionType.Internal && fp.Company == Env.Company)
                .FirstOrDefault();
        }

        if (SOUTH_SOUTHEAST.Contains(Env.Company.State.Code) && NORTH_NORTHEAST_MIDWEST.Contains(delivery.State.Code))
        {
            return Env.Search<AccountFiscalPosition>()
                .Where(fp => fp.L10nBrFpType == AccountFiscalPositionType.SsNnm && fp.Company == Env.Company)
                .FirstOrDefault();
        }

        return Env.Search<AccountFiscalPosition>()
            .Where(fp => fp.L10nBrFpType == AccountFiscalPositionType.Interstate && fp.Company == Env.Company)
            .FirstOrDefault();
    }
}
