csharp
public partial class ResPartner
{
    public override IEnumerable<string> CommercialFields()
    {
        var fields = base.CommercialFields().ToList();
        fields.Add("L10nEgBuildingNo");
        return fields;
    }

    public override IEnumerable<string> AddressFields()
    {
        var fields = base.AddressFields().ToList();
        fields.Add("L10nEgBuildingNo");
        return fields;
    }
}
