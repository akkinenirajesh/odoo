csharp
public partial class AccountMove
{
    public string Number2Numeric(string number)
    {
        var invoiceNumber = System.Text.RegularExpressions.Regex.Replace(number, @"\D", "");

        if (string.IsNullOrEmpty(invoiceNumber))
        {
            throw new UserException("Invoice number must contain numeric characters");
        }

        if (invoiceNumber.Length < 3)
        {
            invoiceNumber = ("11" + invoiceNumber).Substring(invoiceNumber.Length - 3);
        }
        else if (invoiceNumber.Length > 19)
        {
            invoiceNumber = invoiceNumber.Substring(0, 19);
        }

        return invoiceNumber;
    }

    public string GetFinnishCheckDigit(string baseNumber)
    {
        int total = 0;
        int[] multipliers = { 7, 3, 1 };
        for (int i = baseNumber.Length - 1; i >= 0; i--)
        {
            total += multipliers[i % 3] * int.Parse(baseNumber[i].ToString());
        }

        return ((10 - (total % 10)) % 10).ToString();
    }

    public string GetRfCheckDigits(string baseNumber)
    {
        string checkBase = baseNumber + "RF00";
        string convertedBase = string.Join("", checkBase.Select(c => char.IsDigit(c) ? c.ToString() : (char.ToUpper(c) - 55).ToString()));
        int remainder = int.Parse(convertedBase) % 97;
        return (98 - remainder).ToString().PadLeft(2, '0');
    }

    public string ComputePaymentReferenceFinnish(string number)
    {
        string invoiceNumber = Number2Numeric(number);
        string checkDigit = GetFinnishCheckDigit(invoiceNumber);
        return invoiceNumber + checkDigit;
    }

    public string ComputePaymentReferenceFinnishRf(string number)
    {
        string invoiceNumber = Number2Numeric(number);
        invoiceNumber += GetFinnishCheckDigit(invoiceNumber);
        string rfCheckDigits = GetRfCheckDigits(invoiceNumber);
        return "RF" + rfCheckDigits + invoiceNumber;
    }

    public string GetInvoiceReferenceFiRfInvoice()
    {
        return ComputePaymentReferenceFinnishRf(this.Name);
    }

    public string GetInvoiceReferenceFiRfPartner()
    {
        return ComputePaymentReferenceFinnishRf(this.Partner.Id.ToString());
    }

    public string GetInvoiceReferenceFiInvoice()
    {
        return ComputePaymentReferenceFinnish(this.Name);
    }

    public string GetInvoiceReferenceFiPartner()
    {
        return ComputePaymentReferenceFinnish(this.Partner.Id.ToString());
    }
}
