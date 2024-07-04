csharp
public partial class MassMailing.UtmMedium
{
    public void UnlinkExceptLinkedMailings()
    {
        var linkedMailings = Env.Model("mailing.mailing").Search(new List<object[]> { new object[] { "medium_id", "in", this.Id } });

        if (linkedMailings.Count > 0)
        {
            throw new Exception(string.Format("You cannot delete these UTM Mediums as they are linked to the following mailings in Mass Mailing:\n{0}",
                string.Join(", ", linkedMailings.Select(x => x.GetField("subject").ToString()))));
        }
    }
}
