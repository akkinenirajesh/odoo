csharp
public partial class Payment
{
    public string ComputeL10nChReferenceWarningMsg()
    {
        if (PaymentType == "outbound" &&
            Partner.Country?.Code is "CH" or "LI" &&
            PartnerBank.L10nChQrIban &&
            !L10nChReferenceIsValid(Ref))
        {
            return Env.Translate("Please fill in a correct QRR reference in the payment reference. The banks will refuse your payment file otherwise.");
        }
        return null;
    }

    public bool L10nChReferenceIsValid(string paymentReference)
    {
        if (string.IsNullOrEmpty(paymentReference))
        {
            return false;
        }

        string ref = paymentReference.Replace(" ", "");
        if (System.Text.RegularExpressions.Regex.IsMatch(ref, @"^(\d{2,27})$"))
        {
            return ref == Mod10r(ref.Substring(0, ref.Length - 1));
        }
        return false;
    }

    private string Mod10r(string number)
    {
        // Implement the mod10r algorithm here
        // This is a placeholder for the actual implementation
        return number;
    }
}
