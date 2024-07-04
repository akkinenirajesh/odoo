csharp
public partial class MailResGroups 
{
    public virtual int Write(Dictionary<string, object> vals)
    {
        int res = Env.Model("Mail.ResGroups").Call("Write", this, vals);
        if (vals.ContainsKey("Users"))
        {
            Env.Model("Discuss.Channel").Search(new Dictionary<string, object>() { { "GroupIds", this.Id } }).Call("_SubscribeUsersAutomatically");
        }
        return res;
    }
}
