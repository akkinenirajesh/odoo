csharp
public partial class MrpSubcontracting.StockLocation
{
    public void CheckSubcontractingLocation()
    {
        if (this == Env.Company.SubcontractingLocationId)
        {
            throw new Exception("You cannot alter the company's subcontracting location");
        }
        if (this.IsSubcontractingLocation && this.Usage != "internal")
        {
            throw new Exception("In order to manage stock accurately, subcontracting locations must be type Internal, linked to the appropriate company.");
        }
    }

    public void ActivateSubcontractingLocationRules()
    {
        // TODO: Implement the logic to create or unarchive rules for the 'custom' subcontracting location(s).
        // The subcontracting location defined on the company is considered as the 'reference' one.
        // All rules defined on this 'reference' location will be replicated on 'custom' subcontracting locations.
    }

    public void ArchiveSubcontractingLocationRules()
    {
        // TODO: Implement the logic to archive subcontracting rules for locations that are no longer 'custom' subcontracting locations.
    }

    public MrpSubcontracting.StockLocation CheckAccessPutaway()
    {
        if (Env.User.PartnerId.IsSubcontractor)
        {
            return this.Sudo();
        }
        else
        {
            return this;
        }
    }
}
