csharp
public partial class IrActionsReport
{
    public virtual object PreRenderQwebPdf(object reportRef, object resIds, object data)
    {
        // Check for reports only available for invoices.
        if (Env.Ref(reportRef.ToString()).GetAttributeValue<string>("ReportName") == "l10n_th.report_commercial_invoice")
        {
            var invoices = Env.Ref("Account.Move").Browse(resIds);
            if (invoices.Any(x => !x.GetAttributeValue<bool>("IsInvoice", true)))
            {
                throw new UserError("Only invoices could be printed.");
            }
        }

        return Env.Ref("base.ir_actions_report").Call("PreRenderQwebPdf", reportRef, resIds, data);
    }
}
