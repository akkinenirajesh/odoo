csharp
public partial class ResPartnerBank
{
    public string GetBban()
    {
        if (AccType != "iban")
        {
            throw new UserException("Cannot compute the BBAN because the account number is not an IBAN.");
        }
        return GetBbanFromIban(AccNumber);
    }

    public bool CheckIban(string iban = "")
    {
        try
        {
            ValidateIban(iban);
            return true;
        }
        catch (ValidationException)
        {
            return false;
        }
    }

    public override void OnCreate()
    {
        base.OnCreate();
        if (!string.IsNullOrEmpty(AccNumber))
        {
            try
            {
                ValidateIban(AccNumber);
                AccNumber = PrettyIban(NormalizeIban(AccNumber));
            }
            catch (ValidationException)
            {
                // Ignore validation error
            }
        }
    }

    public override void OnWrite()
    {
        base.OnWrite();
        if (!string.IsNullOrEmpty(AccNumber))
        {
            try
            {
                ValidateIban(AccNumber);
                AccNumber = PrettyIban(NormalizeIban(AccNumber));
            }
            catch (ValidationException)
            {
                // Ignore validation error
            }
        }
    }

    public override void OnValidate()
    {
        base.OnValidate();
        if (AccType == "iban")
        {
            ValidateIban(AccNumber);
        }
    }

    private string NormalizeIban(string iban)
    {
        return System.Text.RegularExpressions.Regex.Replace(iban ?? "", @"[\W_]", "");
    }

    private string PrettyIban(string iban)
    {
        try
        {
            ValidateIban(iban);
            return string.Join(" ", Enumerable.Range(0, iban.Length / 4)
                .Select(i => iban.Substring(i * 4, 4)));
        }
        catch (ValidationException)
        {
            return iban;
        }
    }

    private string GetBbanFromIban(string iban)
    {
        return NormalizeIban(iban).Substring(4);
    }

    private void ValidateIban(string iban)
    {
        iban = NormalizeIban(iban);
        if (string.IsNullOrEmpty(iban))
        {
            throw new ValidationException("There is no IBAN code.");
        }

        string countryCode = iban.Substring(0, 2).ToLower();
        if (!_mapIbanTemplate.ContainsKey(countryCode))
        {
            throw new ValidationException("The IBAN is invalid, it should begin with the country code");
        }

        string ibanTemplate = _mapIbanTemplate[countryCode];
        if (iban.Length != ibanTemplate.Replace(" ", "").Length || !System.Text.RegularExpressions.Regex.IsMatch(iban, "^[a-zA-Z0-9]+$"))
        {
            throw new ValidationException($"The IBAN does not seem to be correct. You should have entered something like this {ibanTemplate}\n" +
                "Where B = National bank code, S = Branch code, C = Account No, k = Check digit");
        }

        string checkChars = iban.Substring(4) + iban.Substring(0, 4);
        BigInteger digits = BigInteger.Parse(string.Join("", checkChars.Select(c => ((int)c >= 65 ? ((int)c - 55).ToString() : c.ToString())));
        if (digits % 97 != 1)
        {
            throw new ValidationException("This IBAN does not pass the validation check, please verify it.");
        }
    }

    private static readonly Dictionary<string, string> _mapIbanTemplate = new Dictionary<string, string>
    {
        // Add all country codes and IBAN templates here
        {"ad", "ADkk BBBB SSSS CCCC CCCC CCCC"},
        {"ae", "AEkk BBBC CCCC CCCC CCCC CCC"},
        // ... (add all other country codes and templates)
    };
}
