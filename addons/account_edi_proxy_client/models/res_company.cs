csharp
public partial class ResCompany
{
    public override string ToString()
    {
        // Assuming there's a Name field in the ResCompany model
        return Name;
    }

    public IEnumerable<AccountEdiProxyClient.User> GetAccountEdiProxyClientUsers()
    {
        return Env.Set<AccountEdiProxyClient.User>().Where(u => u.Company == this);
    }
}
