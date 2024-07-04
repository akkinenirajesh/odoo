csharp
public partial class EventMail
{
    public override string ToString()
    {
        return Event?.ToString() ?? string.Empty;
    }

    public void Execute()
    {
        var now = DateTime.Now;
        if (IntervalType == IntervalType.AfterSub)
        {
            var newRegistrations = Event.RegistrationIds
                .Where(r => r.State != "cancel" && r.State != "draft")
                .Except(MailRegistrationIds.Select(mr => mr.Registration))
                .ToList();

            CreateMissingMailRegistrations(newRegistrations);

            foreach (var mailRegistration in MailRegistrationIds)
            {
                mailRegistration.Execute();
            }

            var totalSent = MailRegistrationIds.Count(reg => reg.MailSent);
            MailDone = totalSent >= (Event.SeatsReserved + Event.SeatsUsed);
            MailCountDone = totalSent;
        }
        else
        {
            if (MailDone || NotificationType != NotificationType.Mail || TemplateRef == null)
            {
                return;
            }

            if (ScheduledDate <= now && (IntervalType != IntervalType.BeforeEvent || Event.DateEnd > now))
            {
                Event.MailAttendees(TemplateRef.Id);
                MailDone = true;
                MailCountDone = Event.RegistrationIds.Count(r => r.State != "cancel");
            }
        }
    }

    private void CreateMissingMailRegistrations(List<EventRegistration> registrations)
    {
        foreach (var registration in registrations)
        {
            Env.EventMailRegistration.Create(new EventMailRegistration
            {
                Registration = registration,
                Scheduler = this
            });
        }
    }

    public MailValues PrepareEventMailValues()
    {
        return new MailValues(
            NotificationType,
            IntervalNbr,
            IntervalUnit,
            IntervalType,
            $"{TemplateRef.GetType().Name},{TemplateRef.Id}"
        );
    }

    [Compute(nameof(ScheduledDate))]
    private void ComputeScheduledDate()
    {
        DateTime? date;
        int sign;

        switch (IntervalType)
        {
            case IntervalType.AfterSub:
                date = Event.CreateDate;
                sign = 1;
                break;
            case IntervalType.BeforeEvent:
                date = Event.DateBegin;
                sign = -1;
                break;
            default:
                date = Event.DateEnd;
                sign = 1;
                break;
        }

        if (date.HasValue)
        {
            ScheduledDate = date.Value.AddInterval(IntervalUnit, sign * IntervalNbr);
        }
        else
        {
            ScheduledDate = null;
        }
    }

    [Compute(nameof(MailState))]
    private void ComputeMailState()
    {
        if (IntervalType == IntervalType.AfterSub)
        {
            MailState = MailState.Running;
        }
        else if (MailDone)
        {
            MailState = MailState.Sent;
        }
        else if (ScheduledDate.HasValue)
        {
            MailState = MailState.Scheduled;
        }
        else
        {
            MailState = MailState.Running;
        }
    }

    [Compute(nameof(TemplateModelId))]
    private void ComputeTemplateModelId()
    {
        TemplateModelId = NotificationType == NotificationType.Mail
            ? Env.IrModel.GetByName("mail.template")
            : null;
    }
}
