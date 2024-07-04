csharp
using System;
using System.Text.RegularExpressions;

public partial class SaleOrder
{
    public string ComputePaymentReferenceFinnish(string number)
    {
        string soNumber = Number2Numeric(number);
        string checkDigit = GetFinnishCheckDigit(soNumber);
        return soNumber + checkDigit;
    }

    private string Number2Numeric(string number)
    {
        string soNumber = Regex.Replace(number, @"\D", "");
        if (string.IsNullOrEmpty(soNumber))
        {
            throw new Exception("Reference must contain numeric characters");
        }

        if (soNumber.Length < 3)
        {
            soNumber = ("11" + soNumber).Substring(soNumber.Length - 3);
        }
        else if (soNumber.Length > 19)
        {
            soNumber = soNumber.Substring(0, 19);
        }

        return soNumber;
    }

    private string GetFinnishCheckDigit(string baseNumber)
    {
        int total = 0;
        int[] multipliers = { 7, 3, 1 };
        for (int i = baseNumber.Length - 1, j = 0; i >= 0; i--, j++)
        {
            total += multipliers[j % 3] * int.Parse(baseNumber[i].ToString());
        }
        return ((10 - (total % 10)) % 10).ToString();
    }

    public override bool Write(Dictionary<string, object> values)
    {
        if (values.TryGetValue("Reference", out object referenceObj) && referenceObj is string reference)
        {
            values["Reference"] = ComputePaymentReferenceFinnish(reference);
        }
        return base.Write(values);
    }
}
