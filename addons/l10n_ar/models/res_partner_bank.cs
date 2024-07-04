csharp
using System;
using System.Linq;
using System.Text.RegularExpressions;

public partial class ResPartnerBank
{
    public string RetrieveAccType(string accNumber)
    {
        try
        {
            ValidateCbu(accNumber);
            return "cbu";
        }
        catch (Exception)
        {
            // Call the base implementation
            return base.RetrieveAccType(accNumber);
        }
    }

    private void ValidateCbu(string number)
    {
        number = Regex.Replace(number, @"[\s-]", "").Trim();

        if (number.Length != 22)
        {
            throw new ValidationException("Invalid Length");
        }

        if (!number.All(char.IsDigit))
        {
            throw new ValidationException("Invalid Format");
        }

        if (CalculateCheckDigit(number.Substring(0, 7)) != number[7] - '0')
        {
            throw new ValidationException("Invalid Checksum");
        }

        if (CalculateCheckDigit(number.Substring(8, 13)) != number[21] - '0')
        {
            throw new ValidationException("Invalid Checksum");
        }
    }

    private int CalculateCheckDigit(string number)
    {
        int[] weights = { 3, 1, 7, 9 };
        int sum = number.Select((c, i) => (c - '0') * weights[i % 4]).Sum();
        return (10 - sum % 10) % 10;
    }

    public override string ToString()
    {
        return AccountNumber;
    }

    public IEnumerable<(string Value, string Name)> GetSupportedAccountTypes()
    {
        var baseTypes = base.GetSupportedAccountTypes();
        return baseTypes.Append(("cbu", Env.T("CBU")));
    }
}
