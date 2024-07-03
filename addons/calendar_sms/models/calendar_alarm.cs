csharp
public partial class CalendarAlarm
{
    public void ComputeSmsTemplateId()
    {
        if (AlarmType == AlarmType.Sms && SmsTemplateId == null)
        {
            SmsTemplateId = Env.Ref<Sms.SmsTemplate>("calendar_sms.sms_template_data_calendar_reminder");
        }
        else if (AlarmType != AlarmType.Sms || SmsTemplateId == null)
        {
            SmsTemplateId = null;
        }
    }

    public void OnChangeDurationInterval()
    {
        base.OnChangeDurationInterval();
        if (AlarmType != AlarmType.Sms)
        {
            SmsNotifyResponsible = false;
        }
        else if (SmsNotifyResponsible)
        {
            Name += " - " + Env.T("Notify Responsible");
        }
    }
}
