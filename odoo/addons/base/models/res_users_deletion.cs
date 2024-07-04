csharp
public partial class BaseResUsersDeletion {

    public void ComputeUserIdInt() {
        if (this.UserId != null) {
            this.UserIdInt = this.UserId.Id;
        }
    }

    public void GcPortalUsers(int batchSize = 50) {
        var deleteRequests = Env.Search<BaseResUsersDeletion>(x => x.State == "todo");
        var doneRequests = deleteRequests.Where(x => x.UserId == null).ToList();
        doneRequests.ForEach(x => x.State = "done");
        var todoRequests = deleteRequests.Except(doneRequests).ToList();
        var cronDone = doneRequests.Count;
        var cronRemaining = todoRequests.Count;
        Env.Get<IrCron>().NotifyProgress(cronDone, cronRemaining);
        var batchRequests = todoRequests.Take(batchSize).ToList();

        var autoCommit = !Env.Get<Threading.Thread>().Testing;

        foreach (var deleteRequest in batchRequests) {
            var user = deleteRequest.UserId;
            var userName = user.Name;
            var requesterName = deleteRequest.CreateUid.Name;
            try {
                Env.Cr.Execute("SAVEPOINT delete_user");
                var partner = user.PartnerId;
                user.Unlink();
                Env.Logger.Info($"User #{user.Id} {userName}, deleted. Original request from {requesterName}.");
                Env.Cr.Execute("RELEASE SAVEPOINT delete_user");
                deleteRequest.State = "done";
            } catch (Exception e) {
                Env.Logger.Error($"User #{user.Id} {userName} could not be deleted. Original request from {requesterName}. Related error: {e}");
                Env.Cr.Execute("ROLLBACK TO SAVEPOINT delete_user");
                deleteRequest.State = "fail";
            }
            cronDone++;
            cronRemaining--;
            if (autoCommit) {
                Env.Get<IrCron>().NotifyProgress(cronDone, cronRemaining);
                Env.Cr.Commit();
            }
            if (deleteRequest.State == "fail") {
                continue;
            }
            try {
                Env.Cr.Execute("SAVEPOINT delete_partner");
                partner.Unlink();
                Env.Logger.Info($"Partner #{partner.Id} {userName}, deleted. Original request from {requesterName}.");
                Env.Cr.Execute("RELEASE SAVEPOINT delete_partner");
            } catch (Exception e) {
                Env.Logger.Warning($"Partner #{partner.Id} {userName} could not be deleted. Original request from {requesterName}. Related error: {e}");
                Env.Cr.Execute("ROLLBACK TO SAVEPOINT delete_partner");
            }
            if (autoCommit) {
                Env.Cr.Commit();
            }
        }
        Env.Get<IrCron>().NotifyProgress(cronDone, cronRemaining);
    }
}
