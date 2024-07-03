csharp
using System;
using System.Text.RegularExpressions;

public partial class BarcodeRule
{
    public override string ToString()
    {
        return Name;
    }

    public void CheckPattern()
    {
        string p = Pattern.Replace("\\\\", "X").Replace("\\{", "X").Replace("\\}", "X");
        var findall = Regex.Matches(p, "[{]|[}]");

        if (findall.Count == 2)
        {
            if (!Regex.IsMatch(p, "[{][N]*[D]*[}]"))
            {
                throw new ValidationException($"There is a syntax error in the barcode pattern {Pattern}: braces can only contain N's followed by D's.");
            }
            else if (Regex.IsMatch(p, "[{][}]"))
            {
                throw new ValidationException($"There is a syntax error in the barcode pattern {Pattern}: empty braces.");
            }
        }
        else if (findall.Count != 0)
        {
            throw new ValidationException($"There is a syntax error in the barcode pattern {Pattern}: a rule can only contain one pair of braces.");
        }
        else if (p == "*")
        {
            throw new ValidationException("'*' is not a valid Regex Barcode Pattern. Did you mean '.*'?");
        }
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
