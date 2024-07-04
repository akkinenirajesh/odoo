csharp
public partial class BaseIrCron 
{
    public bool MethodDirectTrigger()
    {
        this.EnsureOne();
        this.CheckAccessRights("write");
        this.TryLock();
        Env.Logger.Info($"Job {this.Name} ({this.Id}) started manually");
        this = this.WithUser(this.UserId).WithContext(new Dictionary<string, object> { { "Lastcall", this.Lastcall } }).AddProgress();
        Env.GetModel("Base.IrActionsServer").Browse(this.IrActionsServerId).Run();
        this.Lastcall = Env.Now;
        Env.FlushAll();
        Env.Logger.Info($"Job {this.Name} ({this.Id}) done");
        return true;
    }

    private void TryLock()
    {
        if (this.Ids.Count == 0)
        {
            return;
        }

        try
        {
            Env.Cr.Execute(
                $"SELECT id FROM \"{this.TableName}\" WHERE id IN {this.Ids} FOR NO KEY UPDATE NOWAIT",
                new object[] { });
        }
        catch (Exception)
        {
            Env.Cr.Rollback();
            throw new UserError("Record cannot be modified right now: This cron task is currently being executed and may not be modified. Please try again in a few minutes.");
        }
    }

    private BaseIrCron AddProgress(int timedOutCounter = 0)
    {
        var progress = Env.GetModel("Base.IrCronProgress").Create(new Dictionary<string, object> {
            { "CronId", this.Id },
            { "Remaining", 0 },
            { "Done", 0 },
            { "TimedOutCounter", timedOutCounter },
        });
        return this.WithContext(new Dictionary<string, object> { { "IrCronProgressId", progress.Id } });
    }

    private void NotifyProgress(int done, int remaining)
    {
        if (!this.Context.ContainsKey("IrCronProgressId"))
        {
            return;
        }

        Env.GetModel("Base.IrCronProgress").Browse(this.Context["IrCronProgressId"]).Write(new Dictionary<string, object> {
            { "Remaining", remaining },
            { "Done", done },
        });
    }

    private void Callback(string cronName, int serverActionId)
    {
        this.EnsureOne();
        try
        {
            // Reload self if registry has changed
            this = Env.GetModel(this.ModelName);
            Env.Logger.Info($"Job {cronName} ({this.Id}) starting");
            var startTime = DateTime.Now;
            Env.GetModel("Base.IrActionsServer").Browse(serverActionId).Run();
            Env.FlushAll();
            var endTime = DateTime.Now;
            Env.Logger.Info($"Job {cronName} ({this.Id}) done in {endTime - startTime}");
            Env.Pool.SignalChanges();
        }
        catch (Exception ex)
        {
            Env.Pool.ResetChanges();
            Env.Logger.Error($"Job {cronName} ({this.Id}) server action #{serverActionId} failed", ex);
            Env.Cr.Rollback();
            throw;
        }
    }

    private void NotifyAdmin(string message)
    {
        Env.Logger.Warning(message);
    }
}
