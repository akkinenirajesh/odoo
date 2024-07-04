csharp
public partial class HrJob
{
    public IActionResult ActionSearchMatchingApplicant()
    {
        string helpMessage1 = "No Matching Applicants";
        string helpMessage2 = "We do not have any applicants who meet the skill requirements for this job position in the database at the moment.";

        var action = Env.Actions.ForXmlId("HrRecruitment", "CrmCaseCateg0ActJob");
        var context = new Dictionary<string, object>(action.Context)
        {
            ["ActiveId"] = this.Id
        };

        action.Name = "Matching Applicants";
        action.Views = new List<View>
        {
            new View(Env.Ref("HrRecruitmentSkills.CrmCaseTreeViewInheritHrRecruitmentSkills"), "tree"),
            new View(null, "form")
        };
        action.Context = context;
        action.Domain = new List<object>
        {
            new List<object> { "JobId", "!=", this.Id },
            new List<object> { "SkillIds", "in", this.SkillIds.Select(s => s.Id).ToList() }
        };
        action.Help = $"<p class='o_view_nocontent_empty_folder'>{helpMessage1}</p><p>{helpMessage2}</p>";

        return action;
    }
}
