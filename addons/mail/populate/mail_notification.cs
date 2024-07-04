csharp
public partial class MailNotification
{
    public MailNotification()
    {
    }

    public void Populate(int size)
    {
        var res = Env.GetModel("Mail.MailNotification")._Populate(size);
    }

    public void PopulateThreads(int size, string modelName)
    {
        var admin = Env.Ref("base.user_admin").PartnerId;
        var random = new Random();
        var partners = Env.GetModel("res.partner").Browse(Env.Registry.PopulatedModels["res.partner"]);
        var threads = Env.GetModel(modelName).Browse(Env.Registry.PopulatedModels[modelName]);
        var threadsWithMessages = threads.Filter(thread => thread.MessageIds.Count > 0);
        var notifications = new List<Dictionary<string, object>>();
        var bigDone = 0;
        var maxPossible = admin.Count + partners.Count;
        var big = Math.Min(200, maxPossible);
        Env.Logger.Info($"Preparing to populate Mail.MailNotification for {threadsWithMessages.Count} threads with {maxPossible} possible different recipients");
        foreach (var thread in threadsWithMessages.Sample(random, size))
        {
            foreach (var message in thread.MessageIds)
            {
                var maxNotifications = size switch
                {
                    "small" => 10,
                    "medium" => 20,
                    "large" => 50,
                    _ => 0,
                };
                var numberNotifications = bigDone < 2 ? big : random.Next(maxNotifications);
                if (numberNotifications >= big)
                {
                    bigDone++;
                }
                var recipients = (admin + partners).Sample(random, Math.Min(numberNotifications, maxPossible));
                var hasError = false;
                foreach (var recipient in recipients)
                {
                    var notificationType = random.Choice(new[] { "inbox", "email" }, new[] { 1, 10 });
                    var forceError = !hasError && message.AuthorId == admin;
                    var notificationStatus = notificationType == "inbox"
                        ? "sent"
                        : random.Choice(
                            new[] { "ready", "process", "pending", "sent", "bounce", "exception", "canceled" },
                            new[] { 1, 1, 1, 10, forceError ? 10000 : 10, forceError ? 10000 : 10, 2 });
                    if (notificationStatus is "bounce" or "exception" && message.AuthorId == admin)
                    {
                        hasError = true;
                    }
                    var failureType = notificationStatus switch
                    {
                        "ready" or "process" or "pending" or "sent" => false,
                        "bounce" => "mail_bounce",
                        _ => random.Choice(new[] { "unknown", "mail_email_invalid", "mail_email_missing", "mail_from_invalid", "mail_from_missing", "mail_smtp" }),
                    };
                    notifications.Add(new Dictionary<string, object>
                    {
                        { "AuthorId", message.AuthorId.Id },
                        { "MailMessageId", message.Id },
                        { "ResPartnerId", recipient.Id },
                        { "NotificationType", notificationType },
                        { "NotificationStatus", notificationStatus },
                        { "FailureType", failureType },
                    });
                }
            }
        }
        var res = Env.GetModel("Mail.MailNotification");
        var batches = notifications.Chunk(1000).ToList();
        var count = 0;
        foreach (var batch in batches)
        {
            count += batch.Count;
            Env.Logger.Info($"Batch of Mail.MailNotification for {modelName}: {count}/{notifications.Count}");
            res += Env.GetModel("Mail.MailNotification").Create(batch);
        }
    }
}
