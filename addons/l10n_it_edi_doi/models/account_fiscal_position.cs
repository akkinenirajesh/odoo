csharp
public partial class AccountFiscalPosition
{
    public override void OnDelete()
    {
        if (this == this.Company.L10nItEdiDoiFiscalPosition)
        {
            throw new UserException("You cannot delete the special fiscal position for Declarations of Intent.");
        }
        base.OnDelete();
    }
}
