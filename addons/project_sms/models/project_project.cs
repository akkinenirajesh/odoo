csharp
public partial class ProjectProject
{
    public void SendSms()
    {
        if (this.Partner != null && this.Stage != null && this.Stage.SmsTemplate != null)
        {
            this.MessageSmsWithTemplate(this.Stage.SmsTemplate, this.Partner.Id);
        }
    }

    public ProjectProject Create(Dictionary<string, object> values)
    {
        var project = Env.Create<ProjectProject>(values);
        project.SendSms();
        return project;
    }

    public void Write(Dictionary<string, object> values)
    {
        Env.Write<ProjectProject>(this.Id, values);
        if (values.ContainsKey("Stage"))
        {
            this.SendSms();
        }
    }

    private void MessageSmsWithTemplate(MailTemplate template, int partnerId)
    {
        // Implementation of sending SMS using template and partnerId.
    }
}
