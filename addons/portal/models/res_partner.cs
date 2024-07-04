csharp
public partial class PortalResPartner
{
    public bool CanEditName()
    {
        return true;
    }

    public bool CanEditVat()
    {
        return this.ParentID == null;
    }
}
