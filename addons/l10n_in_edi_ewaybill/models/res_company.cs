csharp
public partial class ResCompany
{
    public bool L10nInEdiEwaybillTokenIsValid()
    {
        if (L10nInEdiEwaybillAuthValidity != null && L10nInEdiEwaybillAuthValidity > DateTime.Now)
        {
            return true;
        }
        return false;
    }
}
