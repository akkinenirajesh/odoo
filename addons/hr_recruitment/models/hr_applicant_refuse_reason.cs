csharp
public partial class ApplicantRefuseReason
{
    public override string ToString()
    {
        return Name;
    }

    public IEnumerable<Mail.Template> GetAvailableTemplates()
    {
        return Env.Query<Mail.Template>().Where(t => t.Model == "HumanResources.Applicant").ToList();
    }
}
