csharp
public partial class Job
{
    public ActionResult ActionTestSurvey()
    {
        if (SurveyId == null)
        {
            throw new InvalidOperationException("Survey not set for this job.");
        }
        return SurveyId.ActionTestSurvey();
    }

    public ActionResult ActionNewSurvey()
    {
        var survey = Env.GetModel<Survey.Survey>().Create(new Dictionary<string, object>
        {
            { "Title", $"Interview Form: {Name}" }
        });

        SurveyId = survey;

        return new ActionResult
        {
            Name = "Survey",
            ViewMode = "form,tree",
            ResModel = "Survey.Survey",
            Type = "ir.actions.act_window",
            ResId = survey.Id
        };
    }

    public override string ToString()
    {
        return Name;
    }
}
