csharp
public partial class Employee
{
    [DependsContext("Lang")]
    [Depends("SubscribedCourses", "UserPartnerId.SlideChannelCompletedIds")]
    public void ComputeCoursesCompletionText()
    {
        if (UserPartnerId == null)
        {
            CoursesCompletionText = null;
            HasSubscribedCourses = false;
            return;
        }

        int totalCompletedCourses = UserPartnerId.SlideChannelCompletedIds.Count;
        int total = SubscribedCourses.Count;
        CoursesCompletionText = string.Format(Env.Lang.GetString("{0} / {1}"), totalCompletedCourses, total);
        HasSubscribedCourses = total > 0;
    }

    public Dictionary<string, object> ActionOpenCourses()
    {
        return new Dictionary<string, object>
        {
            ["type"] = "ir.actions.act_url",
            ["target"] = "self",
            ["url"] = $"/profile/user/{UserId.Id}"
        };
    }
}
