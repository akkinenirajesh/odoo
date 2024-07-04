C#
public partial class MassMailing.UtmSource {
    public void UnlinkExceptLinkedMailings() {
        var linkedMailings = Env.GetModel<MassMailing.Mailing>().Search(x => x.SourceId.IsIn(this.Ids));
        if (linkedMailings != null && linkedMailings.Count > 0) {
            throw new UserError($"You cannot delete these UTM Sources as they are linked to the following mailings in Mass Mailing:\n{string.Join(", ", linkedMailings.Select(x => $"\"{x.Subject}\""))}.");
        }
    }
}
