csharp
public partial class ResUsers
{
    public void CreateRecruitmentInterviewers()
    {
        if (this == null)
            return;

        var interviewerGroup = Env.Ref<Core.Group>("hr_recruitment.group_hr_recruitment_interviewer");
        var recruitmentGroup = Env.Ref<Core.Group>("hr_recruitment.group_hr_recruitment_user");

        var allUsers = Env.Set<ResUsers>();
        var interviewers = allUsers.Except(recruitmentGroup.Users);

        foreach (var interviewer in interviewers)
        {
            interviewer.GroupIds = interviewer.GroupIds.Append(interviewerGroup);
        }
    }

    public void RemoveRecruitmentInterviewers()
    {
        if (this == null)
            return;

        var interviewerGroup = Env.Ref<Core.Group>("hr_recruitment.group_hr_recruitment_interviewer");
        var recruitmentGroup = Env.Ref<Core.Group>("hr_recruitment.group_hr_recruitment_user");

        var jobInterviewers = Env.Set<Hr.Job>()
            .Where(j => j.InterviewerIds.Contains(this))
            .SelectMany(j => j.InterviewerIds)
            .Distinct();

        var applicationInterviewers = Env.Set<Hr.Applicant>()
            .Where(a => a.InterviewerIds.Contains(this))
            .SelectMany(a => a.InterviewerIds)
            .Distinct();

        var userIds = jobInterviewers.Union(applicationInterviewers).Select(u => u.Id).ToHashSet();

        var usersToRemove = Env.Set<ResUsers>()
            .Where(u => !userIds.Contains(u.Id) && !recruitmentGroup.Users.Contains(u));

        foreach (var user in usersToRemove)
        {
            user.GroupIds = user.GroupIds.Where(g => g != interviewerGroup).ToList();
        }
    }
}
