csharp
public partial class AccountMove
{
    public void _ComputeL10nEsIsSimplified()
    {
        var simplifiedPartner = Env.Ref("l10n_es.partner_simplified", raiseIfNotFound: false);
        if (simplifiedPartner != null)
        {
            L10nEsIsSimplified = (PartnerId == simplifiedPartner);
        }
    }
}
