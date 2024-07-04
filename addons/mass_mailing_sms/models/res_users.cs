C#
public partial class ResUsers {

    public List<ActivityGroup> GetActivityGroups() {
        var activities = Env.Call("res.users", "_get_activity_groups", this);
        return activities;
    }
}
