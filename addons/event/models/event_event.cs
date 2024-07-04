csharp
public partial class EventEvent
{
    public override string ToString()
    {
        return Name;
    }

    public void ActionSetDone()
    {
        var firstEndedStage = Env.EventStage.Search(new[] { ("PipeEnd", "=", true) }, limit: 1, order: "Sequence");
        if (firstEndedStage.Any())
        {
            this.Stage = firstEndedStage.First();
        }
    }

    public void MailAttendees(int templateId, bool forceSend = false, Func<EventRegistration, bool> filterFunc = null)
    {
        filterFunc ??= (registration => registration.State != "cancel");

        foreach (var attendee in this.Registrations.Where(filterFunc))
        {
            Env.MailTemplate.Browse(templateId).SendMail(attendee.Id, forceSend: forceSend);
        }
    }

    public string GetDateRangeStr(string langCode = null)
    {
        var today = DateTime.Now;
        var eventDate = this.DateBegin;
        var diff = (eventDate.Date - today.Date).Days;

        if (diff <= 0)
            return "today";
        if (diff == 1)
            return "tomorrow";
        if (diff < 7)
            return $"in {diff} days";
        if (diff < 14)
            return "next week";
        if (eventDate.Month == today.AddMonths(1).Month)
            return "next month";

        return $"on {Env.FormatDate(this.DateBegin, langCode: langCode, dateFormat: "medium")}";
    }

    public string GetExternalDescription()
    {
        var description = Env.HtmlToInnerContent(this.Description);
        return Env.TextwrapShorten(description, 1900);
    }

    public Dictionary<int, byte[]> GetIcsFile()
    {
        // Implementation depends on the vobject library equivalent in C#
        // This is a placeholder implementation
        return new Dictionary<int, byte[]>();
    }

    public string GetTicketsAccessHash(IEnumerable<int> registrationIds)
    {
        return Env.Tools.Hmac(Env.Su(), "event-registration-ticket-report-access", (this.Id, registrationIds.OrderBy(x => x)));
    }

    public static void GcMarkEventsDone()
    {
        var endedEvents = Env.EventEvent.Search(new[]
        {
            ("DateEnd", "<", DateTime.Now),
            ("Stage.PipeEnd", "=", false)
        });

        if (endedEvents.Any())
        {
            endedEvents.ActionSetDone();
        }
    }
}
