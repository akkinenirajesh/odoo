csharp
public partial class ResCompany
{
    public override string ToString()
    {
        // Assuming there's a Name field in ResCompany
        return Name;
    }

    // Method to get LDAP parameters with system group check
    public ResCompanyLdap[] GetLdapParameters()
    {
        if (Env.User.HasGroup("base.group_system"))
        {
            return Ldaps;
        }
        return new ResCompanyLdap[0];
    }
}
