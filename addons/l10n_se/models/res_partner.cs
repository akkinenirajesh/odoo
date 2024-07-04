csharp
public partial class ResPartner
{
    public void OnChangeL10nSeDefaultVendorPaymentRef()
    {
        if (!string.IsNullOrEmpty(this.L10nSeDefaultVendorPaymentRef) && this.L10nSeCheckVendorOcr)
        {
            string reference = this.L10nSeDefaultVendorPaymentRef;
            try
            {
                // Use a C# library for Luhn validation
                // Ensure the library you use provides the same functionality as the 'stdnum' library.
                // Example using a hypothetical library:
                bool isValid = LuhnValidator.Validate(reference);
                if (!isValid)
                {
                    Env.NotifyWarning("Warning", "Default vendor OCR number isn't a valid OCR number.");
                }
            }
            catch
            {
                Env.NotifyWarning("Warning", "Default vendor OCR number isn't a valid OCR number.");
            }
        }
    }
}
