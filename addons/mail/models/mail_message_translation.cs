csharp
public partial class MailMessageTranslation {
    public void Init() {
        Env.Cr.Execute($"CREATE UNIQUE INDEX IF NOT EXISTS mail_message_translation_unique ON {this.TableName} (MessageId, TargetLang)");
    }

    public void GcTranslations() {
        var treshold = Env.Fields.Datetime.Now - new DateUtil.RelativeDelta(weeks: 2);
        this.Search(x => x.CreateDate < treshold).Unlink();
    }
}
