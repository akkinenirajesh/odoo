C#
public partial class ResUsers
{
    public virtual List<Core.Group> GetActivityGroups()
    {
        List<Core.Group> activities = base.GetActivityGroups();
        foreach (Core.Group activity in activities)
        {
            if (activity.Model == "mailing.mailing")
            {
                activity.Name = Env.Translate("Email Marketing");
                break;
            }
        }
        return activities;
    }
}
