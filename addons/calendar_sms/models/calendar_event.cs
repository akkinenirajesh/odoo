csharp
public partial class CalendarEvent
{
    public void DoSmsReminder(Calendar.CalendarAlarm[] alarms)
    {
        var declinedPartners = AttendeeIds
            .Where(a => a.State == "declined")
            .Select(a => a.Partner)
            .ToList();

        foreach (var alarm in alarms)
        {
            var partners = GetMailPartners()
                .Where(p => !string.IsNullOrEmpty(p.PhoneSanitized) && !declinedPartners.Contains(p))
                .ToList();

            if (UserId != null && !alarm.SmsNotifyResponsible)
            {
                partners.Remove(UserId.Partner);
            }

            MessageSmsWithTemplate(
                template: alarm.SmsTemplateId,
                templateFallback: $"Event reminder: {Name}, {DisplayTime}.",
                partnerIds: partners.Select(p => p.Id).ToArray(),
                putInQueue: false
            );
        }
    }

    public Dictionary<string, object> ActionSendSms()
    {
        if (!PartnerIds.Any())
        {
            throw new UserErrorException("There are no attendees on these events");
        }

        return new Dictionary<string, object>
        {
            ["type"] = "ir.actions.act_window",
            ["name"] = "Send SMS Text Message",
            ["res_model"] = "sms.composer",
            ["view_mode"] = "form",
            ["target"] = "new",
            ["context"] = new Dictionary<string, object>
            {
                ["default_composition_mode"] = "mass",
                ["default_res_model"] = "res.partner",
                ["default_res_ids"] = PartnerIds.Select(p => p.Id).ToArray(),
                ["default_mass_keep_log"] = true
            }
        };
    }

    public string[] GetTriggerAlarmTypes()
    {
        var baseTypes = base.GetTriggerAlarmTypes();
        return baseTypes.Concat(new[] { "sms" }).ToArray();
    }

    // Helper methods (these would typically be implemented elsewhere or use Env)
    private List<Core.Partner> GetMailPartners()
    {
        // Implementation would use Env to fetch partners
        throw new NotImplementedException();
    }

    private void MessageSmsWithTemplate(object template, string templateFallback, int[] partnerIds, bool putInQueue)
    {
        // Implementation would use Env to send SMS
        throw new NotImplementedException();
    }
}
