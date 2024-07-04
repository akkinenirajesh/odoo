csharp
public partial class AccountMove 
{
    public AccountMove() 
    {
    }

    public Action OpenL10nPh2307Wizard()
    {
        var vendorBills = Env.Search<AccountMove>(x => x.MoveType == "in_invoice");
        if (vendorBills.Count > 0) 
        {
            var wizardAction = Env.GetAction("l10n_ph.view_l10n_ph_2307_wizard_act_window");
            wizardAction.Context = new Dictionary<string, object> { { "default_moves_to_export", vendorBills.Select(x => x.Id).ToList() } };
            return wizardAction;
        } 
        else 
        {
            throw new UserError("Only Vendor Bills are available.");
        }
    }
}
