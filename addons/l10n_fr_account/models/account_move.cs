csharp
public partial class AccountMove
{
    public override ViewResult GetView(int? viewId = null, string viewType = "form", Dictionary<string, object> options = null)
    {
        var (arch, view) = base.GetView(viewId, viewType, options);
        var company = Env.Company;
        if (viewType == "form" && company.CountryCode.IsInFranceCountryCodes())
        {
            var shippingField = arch.XPath("//field[@name='PartnerShippingId']").FirstOrDefault();
            if (shippingField != null)
            {
                shippingField.SetAttribute("groups", "");
            }
        }
        return new ViewResult(arch, view);
    }

    private void _ComputeL10nFrIsCompanyFrench()
    {
        L10nFrIsCompanyFrench = CompanyId.CountryCode.IsInFranceCountryCodes();
    }

    private void _ComputeShowDeliveryDate()
    {
        base._ComputeShowDeliveryDate();
        if (CountryCode == "FR")
        {
            ShowDeliveryDate = IsSaleDocument();
        }
    }

    public override bool Post(bool soft = true)
    {
        var result = base.Post(soft);
        if (ShowDeliveryDate && !DeliveryDate.HasValue)
        {
            DeliveryDate = InvoiceDate;
        }
        return result;
    }

    private bool IsSaleDocument()
    {
        // Implement the logic to determine if it's a sale document
        return false; // Placeholder
    }
}
