csharp
public partial class EventTypeMail
{
    public List<(string, string)> SelectionTemplateModel()
    {
        var baseSelection = base.SelectionTemplateModel();
        baseSelection.Add(("Sms.Template", "SMS"));
        return baseSelection;
    }

    public void ComputeTemplateModelId()
    {
        var smsModel = Env.GetModel("Sms.Template");
        var smsMails = this.NotificationType == NotificationType.SMS ? new[] { this } : Array.Empty<EventTypeMail>();
        foreach (var mail in smsMails)
        {
            mail.TemplateModelId = smsModel.Id;
        }
        base.ComputeTemplateModelId();
    }
}

public partial class EventMailScheduler
{
    public List<(string, string)> SelectionTemplateModel()
    {
        var baseSelection = base.SelectionTemplateModel();
        baseSelection.Add(("Sms.Template", "SMS"));
        return baseSelection;
    }

    public Dictionary<string, string> SelectionTemplateModelGetMapping()
    {
        var baseMapping = base.SelectionTemplateModelGetMapping();
        baseMapping["sms"] = "Sms.Template";
        return baseMapping;
    }

    public void ComputeTemplateModelId()
    {
        var smsModel = Env.GetModel("Sms.Template");
        var smsMails = this.NotificationType == NotificationType.SMS ? new[] { this } : Array.Empty<EventMailScheduler>();
        foreach (var mail in smsMails)
        {
            mail.TemplateModelId = smsModel.Id;
        }
        base.ComputeTemplateModelId();
    }

    public void Execute()
    {
        var now = DateTime.Now;
        if (this.IntervalType != "after_sub" && this.NotificationType == NotificationType.SMS)
        {
            if (this.MailDone) return;
            if (string.IsNullOrEmpty(this.TemplateRef)) return;

            if (this.ScheduledDate <= now && (this.IntervalType != "before_event" || this.Event.DateEnd > now))
            {
                var registrations = this.Event.Registrations.Where(r => r.State != "cancel").ToList();
                foreach (var registration in registrations)
                {
                    registration.MessageSmsScheduleMass(this.TemplateRef, true);
                }
                this.MailDone = true;
                this.MailCountDone = registrations.Count;
            }
        }
        base.Execute();
    }

    public void SetTemplateRefModel()
    {
        base.SetTemplateRefModel();
        var mailModel = Env.GetModel("Sms.Template");
        if (this.NotificationType == NotificationType.SMS)
        {
            var record = mailModel.Search(new[] { ("Model", "=", "Event.Registration") }, limit: 1).FirstOrDefault();
            this.TemplateRef = record != null ? $"Sms.Template,{record.Id}" : null;
        }
    }
}

public partial class EventMailRegistration
{
    public void Execute()
    {
        var now = DateTime.Now;
        var todo = this.MailSent == false &&
                   this.Registration.State == "open" || this.Registration.State == "done" &&
                   this.ScheduledDate <= now &&
                   this.Scheduler.NotificationType == NotificationType.SMS;

        if (todo)
        {
            this.Registration.MessageSmsScheduleMass(this.Scheduler.TemplateRef, true);
            this.MailSent = true;
        }

        base.Execute();
    }
}
