csharp
public partial class ResCompany
{
    public override string ToString()
    {
        // Assuming there's a Name field in the base Company model
        return this.Name;
    }

    // You can add any additional methods or computed properties here
    public bool IsEgyptianCompany()
    {
        // Assuming there's a Country field in the base Company model
        return this.Country?.Code == "EG";
    }

    public void UpdateEtaCredentials(string clientId, string clientSecret)
    {
        this.L10nEgClientIdentifier = clientId;
        this.L10nEgClientSecret = clientSecret;
        Env.SaveChanges();
    }
}
