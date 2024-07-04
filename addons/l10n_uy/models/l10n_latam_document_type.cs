C#
public partial class L10nUy.L10nLatamDocumentType
{
    public string FormatDocumentNumber(string documentNumber)
    {
        if (Env.Context.Country.Code != "UY")
        {
            return base.FormatDocumentNumber(documentNumber);
        }

        if (string.IsNullOrEmpty(documentNumber))
        {
            return null;
        }

        documentNumber = documentNumber.Trim();
        var numberPart = System.Text.RegularExpressions.Regex.Matches(documentNumber, @"[\d]+");
        var seriePart = System.Text.RegularExpressions.Regex.Matches(documentNumber, @"^[A-Za-z]+");

        if (seriePart.Count == 0 || seriePart.Count > 1 || seriePart[0].Value.Length > 2 ||
            numberPart.Count == 0 || numberPart.Count > 1 || numberPart[0].Value.Length > 7)
        {
            throw new Exception("Please introduce a valid Document number: 2 letters and 7 digits (XX0000001)");
        }

        return seriePart[0].Value.ToUpper() + numberPart[0].Value.PadLeft(7, '0');
    }
}
