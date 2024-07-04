csharp
public partial class ResPartner
{
    public override string ToString()
    {
        // Assuming there's a Name field in the base ResPartner model
        return Name;
    }

    public List<string> CommercialFields()
    {
        var baseFields = base.CommercialFields();
        baseFields.Add("L10nKeExemptionNumber");
        return baseFields;
    }
}
