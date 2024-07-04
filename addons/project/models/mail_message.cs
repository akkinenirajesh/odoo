csharp
public partial class Project.MailMessage {
    public void Init() {
        Env.Database.CreateUniqueIndex("mail_message_date_res_id_id_for_burndown_chart", 
                                      "mail.message", 
                                      new string[] { "Date", "ResID", "Id" }, 
                                      "Model = 'project.task' AND MessageType = 'notification'");
    }
}
