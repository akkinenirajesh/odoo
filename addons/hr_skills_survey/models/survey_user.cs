csharp
public partial class SurveyUserInput 
{
    public void MarkDone()
    {
        // Call the base implementation if needed
        // base.MarkDone();

        var certificationUserInputs = this.Survey.Certification && this.ScoringSuccess
            ? new List<SurveyUserInput> { this }
            : new List<SurveyUserInput>();

        if (certificationUserInputs.Count == 0)
            return;

        var employee = Env.Get<Hr.Employee>().Search(new[] { ("User.Partner", "=", this.Partner.Id) }).FirstOrDefault();
        if (employee == null)
            return;

        var resumeLines = Env.Get<Hr.ResumeLine>().Search(new[] 
        { 
            ("Employee", "=", employee.Id),
            ("Survey", "=", this.Survey.Id)
        });

        var lineType = Env.Ref<Hr.ResumeLineType>("hr_skills_survey.resume_type_certification");

        var today = DateTime.Today;
        var validityMonths = this.Survey.CertificationValidityMonths;

        var resumeLineVals = new Dictionary<string, object>
        {
            ["Employee"] = employee.Id,
            ["Name"] = this.Survey.Title,
            ["DateStart"] = today,
            ["DateEnd"] = validityMonths.HasValue ? today.AddMonths(validityMonths.Value) : (object)null,
            ["Description"] = HtmlToPlainText(this.Survey.Description),
            ["LineType"] = lineType?.Id,
            ["DisplayType"] = "certification",
            ["Survey"] = this.Survey.Id
        };

        if (resumeLines.Any())
        {
            resumeLines.First().Write(resumeLineVals);
        }
        else
        {
            Env.Get<Hr.ResumeLine>().Create(resumeLineVals);
        }
    }

    private string HtmlToPlainText(string html)
    {
        // Implement HTML to plain text conversion logic here
        throw new NotImplementedException();
    }
}
