csharp
public partial class AccountTax
{
    [OnDelete(AtUninstall = false)]
    public void NeverUnlinkDeclarationOfIntentTax()
    {
        if (this == Env.Company.L10nItEdiDoiTaxId)
        {
            throw new UserException("You cannot delete the special tax for Declarations of Intent.");
        }
    }
}
