csharp
public partial class MassMailingSms.MailingTrace
{
    public void ComputeSmsId()
    {
        this.SmsId = null;
        if (this.TraceType == "sms" && this.SmsIdInt != 0)
        {
            var smsTraces = Env.Model("MassMailingSms.SmsSms").Search(new Dictionary<string, object>() { { "Id", this.SmsIdInt }, { "ToDelete", false } });
            if (smsTraces.Count > 0)
            {
                this.SmsId = smsTraces[0];
            }
        }
    }

    public void Create(Dictionary<string, object> values)
    {
        if (values.ContainsKey("TraceType") && values["TraceType"] == "sms" && !values.ContainsKey("SmsCode"))
        {
            values.Add("SmsCode", GetRandomCode());
        }
        Env.Create(this, values);
    }

    public string GetRandomCode()
    {
        return Guid.NewGuid().ToString().Substring(0, 3);
    }
}
