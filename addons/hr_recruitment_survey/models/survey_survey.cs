csharp
public partial class Survey
{
    public List<SurveyType> AllowedSurveyTypes { get; set; }

    public void ComputeAllowedSurveyTypes()
    {
        // Base implementation logic here (if any)

        if (Env.User.HasGroup("hr_recruitment.group_hr_recruitment_interviewer") ||
            Env.User.HasGroup("survey.group_survey_user"))
        {
            AllowedSurveyTypes = (AllowedSurveyTypes ?? new List<SurveyType>()).Concat(new[] { SurveyType.Recruitment }).ToList();
        }
    }

    public int GetFormviewId(int? accessUid = null)
    {
        if (SurveyType == SurveyType.Recruitment)
        {
            var accessUser = accessUid.HasValue ? Env.Users.Browse(accessUid.Value) : Env.User;
            if (!accessUser.HasGroup("survey.group_survey_user"))
            {
                var view = Env.Ref("hr_recruitment_survey.survey_survey_view_form", false);
                if (view != null)
                {
                    return view.Id;
                }
            }
        }
        return base.GetFormviewId(accessUid);
    }
}
