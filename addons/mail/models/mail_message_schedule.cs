csharp
public partial class MailMessageSchedule 
{
    public MailMessageSchedule()
    {
    }

    public void ForceSend()
    {
        _SendNotifications();
    }

    public void _SendNotifications(Dictionary<string, object> defaultNotifyKwargs = null)
    {
        var grouped = _GroupByModel();
        foreach (var model in grouped.Keys)
        {
            List<MailMessageSchedule> schedules = grouped[model];

            if (model != null)
            {
                List<object> records = Env.GetRecords(model, schedules.Select(s => s.MailMessageId.ResId).ToList());
                foreach (var record in records)
                {
                    foreach (var schedule in schedules)
                    {
                        var notifyKwargs = new Dictionary<string, object>(defaultNotifyKwargs ?? new Dictionary<string, object>());
                        notifyKwargs.Add("SkipExisting", true);
                        try
                        {
                            var scheduleNotifyKwargs = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(schedule.NotificationParameters);
                            scheduleNotifyKwargs.Remove("ScheduledDate");
                            notifyKwargs = notifyKwargs.Union(scheduleNotifyKwargs).ToDictionary(x => x.Key, x => x.Value);
                        }
                        catch (Exception)
                        {
                        }

                        Env.NotifyThread(record, schedule.MailMessageId, false, notifyKwargs);
                    }
                }
            }
            else
            {
                foreach (var schedule in schedules)
                {
                    var notifyKwargs = new Dictionary<string, object>(defaultNotifyKwargs ?? new Dictionary<string, object>());
                    notifyKwargs.Add("SkipExisting", true);
                    try
                    {
                        var scheduleNotifyKwargs = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(schedule.NotificationParameters);
                        scheduleNotifyKwargs.Remove("ScheduledDate");
                        notifyKwargs = notifyKwargs.Union(scheduleNotifyKwargs).ToDictionary(x => x.Key, x => x.Value);
                    }
                    catch (Exception)
                    {
                    }

                    Env.NotifyThread(Env.GetRecord("Mail.Thread"), schedule.MailMessageId, false, notifyKwargs);
                }
            }
        }
        this.Delete();
    }

    public void _SendNotificationsCron()
    {
        var now = DateTime.UtcNow;
        var messagesScheduled = Env.Search<MailMessageSchedule>(s => s.ScheduledDateTime <= now);
        if (messagesScheduled.Count > 0)
        {
            _logger.Info($"Send {messagesScheduled.Count} scheduled messages");
            messagesScheduled._SendNotifications();
        }
    }

    private Dictionary<string, List<MailMessageSchedule>> _GroupByModel()
    {
        var grouped = new Dictionary<string, List<MailMessageSchedule>>();
        foreach (var schedule in this)
        {
            var model = schedule.MailMessageId.Model;
            if (model != null && schedule.MailMessageId.ResId != null)
            {
                if (!grouped.ContainsKey(model))
                {
                    grouped.Add(model, new List<MailMessageSchedule>() { schedule });
                }
                else
                {
                    grouped[model].Add(schedule);
                }
            }
        }
        return grouped;
    }

    public bool _SendMessageNotifications(List<MailMessage> messages, Dictionary<string, object> defaultNotifyKwargs = null)
    {
        var messagesScheduled = Env.Search<MailMessageSchedule>(s => messages.Select(m => m.Id).Contains(s.MailMessageId.Id));
        if (messagesScheduled.Count == 0)
        {
            return false;
        }
        messagesScheduled._SendNotifications(defaultNotifyKwargs);
        return true;
    }

    public bool _UpdateMessageScheduledDateTime(List<MailMessage> messages, DateTime newDateTime)
    {
        var messagesScheduled = Env.Search<MailMessageSchedule>(s => messages.Select(m => m.Id).Contains(s.MailMessageId.Id));
        if (messagesScheduled.Count == 0)
        {
            return false;
        }
        messagesScheduled.ScheduledDateTime = newDateTime;
        Env.GetRecord("Ir.Cron", "mail.ir_cron_send_scheduled_message")._Trigger(newDateTime);
        return true;
    }
}
