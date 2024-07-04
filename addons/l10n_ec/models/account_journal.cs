csharp
public partial class AccountJournal
{
    public void ComputeL10nEcRequireEmission()
    {
        L10nEcRequireEmission = Type == "sale" && CountryCode == "EC" && L10nLatamUseDocuments;
    }
}
