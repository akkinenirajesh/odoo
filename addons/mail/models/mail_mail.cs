csharp
public partial class MailMail
{
    public void ComputeBodyContent()
    {
        this.BodyContent = this.BodyHtml;
    }

    public void ComputeMailMessageIdInt()
    {
        this.MailMessageIdInt = this.MailMessageId.Id;
    }

    public void ComputeRestrictedAttachments()
    {
        // Implement logic to compute RestrictedAttachmentCount and UnrestrictedAttachmentIds
        // Use Env to access other models and methods
    }

    public void InverseUnrestrictedAttachmentIds()
    {
        // Implement logic to update AttachmentIds based on UnrestrictedAttachmentIds
        // Use Env to access other models and methods
    }

    public void SearchBodyContent(string operator, string value)
    {
        // Implement logic to search based on BodyHtml
    }

    public void MarkOutgoing()
    {
        this.State = "Outgoing";
    }

    public void Cancel()
    {
        this.State = "Cancel";
    }

    public void ActionRetry()
    {
        if (this.State == "Exception")
        {
            this.MarkOutgoing();
        }
    }

    public void ActionOpenDocument()
    {
        // Implement logic to open the related record based on Model and ResId
    }

    // Other methods for mail processing, sending, etc.
}
