csharp
public partial class Users
{
    public string GetTotpInviteUrl()
    {
        if (!IsInternal())
        {
            return "/my/security";
        }
        else
        {
            // Assuming there's a base implementation in a parent class
            return base.GetTotpInviteUrl();
        }
    }

    private bool IsInternal()
    {
        // Implementation of _is_internal() method
        // This would need to be implemented based on the specific logic in Odoo
        throw new NotImplementedException("IsInternal method needs to be implemented");
    }
}
