csharp
using System;
using System.Linq;
using System.Text.RegularExpressions;

public partial class ProductTemplate
{
    public string ComputeL10nInHsnWarning()
    {
        var digitSuffixes = new Dictionary<string, string>
        {
            {"4", "either 4, 6 or 8"},
            {"6", "either 6 or 8"},
            {"8", "8"}
        };

        var activeHsnCodeDigitLen = Env.Companies.Max(company => int.Parse(company.L10nInHsnCodeDigit));

        var checkHsn = SaleOk && !string.IsNullOrEmpty(L10nInHsnCode) && activeHsnCodeDigitLen > 0;

        if (checkHsn && (!Regex.IsMatch(L10nInHsnCode, @"^\d{4}$|^\d{6}$|^\d{8}$") || L10nInHsnCode.Length < activeHsnCodeDigitLen))
        {
            return $"HSN code field must consist solely of digits and be {digitSuffixes[activeHsnCodeDigitLen.ToString()]} in length.";
        }

        return null;
    }
}
