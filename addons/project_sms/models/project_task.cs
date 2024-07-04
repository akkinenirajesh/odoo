csharp
public partial class ProjectTask {
  public void SendSms() {
    if (this.PartnerId != null && this.StageId != null && this.StageId.SmsTemplateId != null) {
      this.MessageSmsWithTemplate(
        template: this.StageId.SmsTemplateId,
        partnerIds: new List<long> { this.PartnerId.Id });
    }
  }

  public void Create(List<Dictionary<string, object>> valsList) {
    var tasks = Env.Model<ProjectTask>().Create(valsList);
    foreach (var task in tasks) {
      task.SendSms();
    }
  }

  public void Write(Dictionary<string, object> vals) {
    if (vals.ContainsKey("StageId")) {
      this.SendSms();
    }
  }

  private void MessageSmsWithTemplate(MailTemplate template, List<long> partnerIds) {
    // TODO: Implement this method using C# libraries for sending SMS. 
    // You may need to access the template's content and use it to send the message. 
  }
}
