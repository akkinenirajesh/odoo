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
        if (Encoding == BarcodeEncoding.Gs1128)
        {
            try
            {
                Regex.Match("", Pattern);
            }
            catch (ArgumentException ex)
            {
                throw new ValidationException($"The rule pattern \"{Name}\" is not a valid Regex: {ex.Message}");
            }

            var groups = Regex.Matches(Pattern, @"\([^)]*\)");
            if (groups.Count != 2)
            {
                throw new ValidationException(
                    $"The rule pattern \"{Name}\" is not valid, it needs two groups:\n" +
                    "\t- A first one for the Application Identifier (usually 2 to 4 digits);\n" +
                    "\t- A second one to catch the value."
                );
            }
        }
        else
        {
            // Call the base implementation for non-GS1 rules
            base.CheckPattern();
        }
    }

    public BarcodeEncoding DefaultEncoding()
    {
        return Env.Context.GetValueOrDefault("is_gs1", false) ? BarcodeEncoding.Gs1128 : BarcodeEncoding.Any;
    }
}
