csharp
public partial class AccountPayment
{
    public void ActionOpenL10nPh2307Wizard()
    {
        if (this.PaymentType == "Outbound")
        {
            var wizardAction = Env.Get<IrActionsActWindow>()._ForXmlId("l10n_ph.view_l10n_ph_2307_wizard_act_window");
            wizardAction.Update(new { context = new { defaultMovesToExport = this.ReconciledBillIds.Ids } });
            return wizardAction;
        }
        else
        {
            throw new UserError("Only Outbound Payment is available.");
        }
    }
}
