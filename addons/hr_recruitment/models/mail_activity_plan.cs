csharp
public partial class MailActivityPlan
{
    public void ComputeDepartmentAssignable()
    {
        base.ComputeDepartmentAssignable();
        if (!this.DepartmentAssignable)
        {
            this.DepartmentAssignable = this.ResModel == "HrRecruitment.Applicant";
        }
    }
}
