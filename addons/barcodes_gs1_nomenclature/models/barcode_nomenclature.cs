csharp
public partial class BarcodeNomenclature
{
    public void CheckPattern()
    {
        if (this.IsGs1Nomenclature && !string.IsNullOrEmpty(this.Gs1SeparatorFnc1))
        {
            try
            {
                System.Text.RegularExpressions.Regex.Match("", $"(?:{this.Gs1SeparatorFnc1})?");
            }
            catch (System.ArgumentException error)
            {
                throw new ValidationException($"The FNC1 Separator Alternative is not a valid Regex: {error.Message}");
            }
        }
    }

    public DateTime Gs1DateToDate(string gs1Date)
    {
        var now = DateTime.Today;
        var currentCentury = now.Year / 100;
        var substractYear = int.Parse(gs1Date.Substring(0, 2)) - (now.Year % 100);
        var century = (51 <= substractYear && substractYear <= 99) ? currentCentury - 1 :
                      (-99 <= substractYear && substractYear <= -50) ? currentCentury + 1 :
                      currentCentury;
        var year = century * 100 + int.Parse(gs1Date.Substring(0, 2));

        if (gs1Date.Substring(gs1Date.Length - 2) == "00")
        {
            var date = new DateTime(year, int.Parse(gs1Date.Substring(2, 2)), 1);
            return date.AddMonths(1).AddDays(-1);
        }
        else
        {
            return new DateTime(year, int.Parse(gs1Date.Substring(2, 2)), int.Parse(gs1Date.Substring(4, 2)));
        }
    }

    public Dictionary<string, object> ParseGs1RulePattern(System.Text.RegularExpressions.Match match, BarcodeRule rule)
    {
        var result = new Dictionary<string, object>
        {
            { "rule", rule },
            { "ai", match.Groups[1].Value },
            { "string_value", match.Groups[2].Value }
        };

        if (rule.Gs1ContentType == "measure")
        {
            try
            {
                int decimalPosition = 0;
                if (rule.Gs1DecimalUsage)
                {
                    decimalPosition = int.Parse(match.Groups[1].Value.Substring(match.Groups[1].Value.Length - 1));
                }
                if (decimalPosition > 0)
                {
                    result["value"] = float.Parse(match.Groups[2].Value.Substring(0, match.Groups[2].Value.Length - decimalPosition) + "." + match.Groups[2].Value.Substring(match.Groups[2].Value.Length - decimalPosition));
                }
                else
                {
                    result["value"] = int.Parse(match.Groups[2].Value);
                }
            }
            catch
            {
                throw new ValidationException($"There is something wrong with the barcode rule \"{rule.Name}\" pattern.\n" +
                    "If this rule uses decimal, check it can't get sometime else than a digit as last char for the Application Identifier.\n" +
                    "Check also the possible matched values can only be digits, otherwise the value can't be casted as a measure.");
            }
        }
        else if (rule.Gs1ContentType == "identifier")
        {
            // Check digit and remove it of the value
            if (match.Groups[2].Value[match.Groups[2].Value.Length - 1] != GetBarcodeCheckDigit(new string('0', 18 - match.Groups[2].Value.Length) + match.Groups[2].Value).ToString()[0])
            {
                return null;
            }
            result["value"] = match.Groups[2].Value;
        }
        else if (rule.Gs1ContentType == "date")
        {
            if (match.Groups[2].Value.Length != 6)
            {
                return null;
            }
            result["value"] = Gs1DateToDate(match.Groups[2].Value);
        }
        else
        {
            result["value"] = match.Groups[2].Value;
        }
        return result;
    }

    // Other methods like Gs1DecomposeExtanded, ParseBarcode, etc. would be implemented similarly
}
