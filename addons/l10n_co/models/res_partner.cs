csharp
public partial class ResPartner
{
    public override void OnValidate()
    {
        base.OnValidate();
        CheckVat();
    }

    private void CheckVat()
    {
        // Check if base_vat module is installed
        var baseVatModule = Env.Ref("base.module_base_vat");
        if (baseVatModule != null && baseVatModule.State == "installed")
        {
            // Don't check Colombian partners unless they have RUT set as document type
            if (CountryId?.Code != "CO" || 
                L10nLatamIdentificationTypeId?.L10nCoDocumentCode == "rut")
            {
                // Perform VAT check
                // Note: Implement the actual VAT check logic here
                // This would typically involve validating the VAT number format
                // and potentially checking against a web service
            }
        }
    }
}
