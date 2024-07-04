csharp
public partial class ResCompany
{
    public bool L10nInEdiTokenIsValid()
    {
        if (!string.IsNullOrEmpty(this.L10nInEdiToken) && this.L10nInEdiTokenValidity > DateTime.Now)
        {
            return true;
        }
        return false;
    }
}
