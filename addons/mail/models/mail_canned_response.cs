csharp
public partial class MailCannedResponse {
    public void ComputeIsShared() {
        this.IsShared = this.GroupIds.Count > 0;
    }

    public void ComputeIsEditable() {
        this.IsEditable = Env.IsUser(this.CreateUserId) || Env.HasAccess("Mail.MailCannedResponse", "write");
    }

    public MailCannedResponse Create() {
        var result = Env.Create<MailCannedResponse>(this);
        Broadcast("insert");
        return result;
    }

    public void Write() {
        var result = Env.Write(this);
        Broadcast();
        return result;
    }

    public void Unlink() {
        Broadcast("delete");
        Env.Unlink(this);
    }

    private void Broadcast(string method = "insert") {
        var notifType = method == "insert" ? "mail.record/insert" : "mail.record/delete";
        var fieldNames = method == "insert" ? new[] { "Id", "Source", "Substitution" } : new[] { "Id" };
        var targets = new List<int>() { Env.UserId };
        if (Env.UserId != this.CreateUserId) {
            targets.Add(this.CreateUserId);
        }
        targets.AddRange(this.GroupIds.Select(x => x.Id));
        var payload = new Dictionary<string, object>() { { "CannedResponse", this.Read(fieldNames) } };
        var notifications = targets.Select(target => (target, notifType, payload)).ToList();
        Env.Bus.SendMany(notifications);
    }
}
