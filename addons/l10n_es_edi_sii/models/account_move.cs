csharp
public partial class AccountMove
{
    public void ComputeL10nEsEdiIsRequired()
    {
        bool hasTax = true;
        if (IsPurchaseDocument())
        {
            var taxes = InvoiceLineIds.SelectMany(line => line.TaxIds);
            hasTax = taxes.Any(t => t.L10nEsType != null && t.L10nEsType != "ignore");
        }

        L10nEsEdiIsRequired = IsInvoice() &&
                              CountryCode == "ES" &&
                              CompanyId.L10nEsEdiTaxAgency &&
                              hasTax;
    }

    public void ComputeEdiShowCancelButton()
    {
        // Call the base implementation
        base.ComputeEdiShowCancelButton();

        if (L10nEsEdiIsRequired)
        {
            EdiShowCancelButton = false;
        }
    }

    public bool L10nEsIsDua()
    {
        return InvoiceLineIds
            .SelectMany(line => line.TaxIds)
            .SelectMany(tax => tax.FlattenTaxesHierarchy())
            .Any(t => t.L10nEsType == "dua");
    }
}
