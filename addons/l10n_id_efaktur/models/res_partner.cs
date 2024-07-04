csharp
public partial class ResPartner
{
    public bool ComputeL10nIdPkp()
    {
        return !string.IsNullOrEmpty(Vat) && CountryCode == "ID";
    }

    public override string ToString()
    {
        // You can customize this based on your requirements
        return Name;
    }
}
