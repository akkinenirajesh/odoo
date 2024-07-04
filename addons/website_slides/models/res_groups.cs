csharp
public partial class UserGroup {
    public void Write(Dictionary<string, object> vals) {
        var writeRes = Env.Call("super", "write", new object[] { vals });
        if (vals.ContainsKey("Users")) {
            Env.Call("slide.channel", "Search", new object[] { new List<object>() { new object[] { "enroll_group_ids", "in", this.Id } } }).Call("_AddGroupsMembers");
        }
        return writeRes;
    }
}
