csharp
public partial class Employee
{
    public override IEnumerable<Core.Partner> GetRelatedPartners()
    {
        var partners = base.GetRelatedPartners();
        return partners.Concat(Env.Sudo().Get<HumanResources.Applicant>().Where(a => a.EmpId == this.Id).Select(a => a.PartnerId));
    }

    public override void OnCreate()
    {
        base.OnCreate();
        if (this.ApplicantId != null)
        {
            this.ApplicantId.MessageLogWithView(
                "HumanResources.ApplicantHiredTemplate",
                new { applicant = this.ApplicantId }
            );
        }
    }
}
