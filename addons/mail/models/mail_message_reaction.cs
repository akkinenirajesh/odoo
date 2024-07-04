csharp
public partial class MailMessageReaction {
    public void Init() {
        Env.Cr.Execute("CREATE UNIQUE INDEX IF NOT EXISTS mail_message_reaction_partner_unique ON %s (message_id, content, partner_id) WHERE partner_id IS NOT NULL" % this.TableName);
        Env.Cr.Execute("CREATE UNIQUE INDEX IF NOT EXISTS mail_message_reaction_guest_unique ON %s (message_id, content, guest_id) WHERE guest_id IS NOT NULL" % this.TableName);
    }
}
