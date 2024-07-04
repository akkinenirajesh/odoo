csharp
public partial class ResPartner
{
    public override string[] CommercialFields()
    {
        var baseFields = base.CommercialFields();
        return baseFields.Concat(new[] { "L10nHuGroupVat" }).ToArray();
    }

    public string RunViesTest(string vatNumber, Core.Country defaultCountry)
    {
        if (defaultCountry != null && defaultCountry.Code == "HU" && !vatNumber.StartsWith("HU"))
        {
            vatNumber = $"HU{vatNumber.Substring(0, 8)}";
        }
        return base.RunViesTest(vatNumber, defaultCountry);
    }
}
