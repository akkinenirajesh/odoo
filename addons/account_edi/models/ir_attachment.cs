csharp
public partial class IrAttachment
{
    [OnDelete(AtUninstall = false)]
    public void UnlinkExceptGovernmentDocument()
    {
        var accountEdiDocumentModel = Env.GetModel("Account.EdiDocument");
        var linkedEdiDocuments = accountEdiDocumentModel.Search(new[] { ("AttachmentId", "in", new[] { this.Id }) });
        
        var linkedEdiFormatsWs = linkedEdiDocuments
            .Select(doc => doc.EdiFormatId)
            .Where(format => format.NeedsWebServices())
            .ToList();

        if (linkedEdiFormatsWs.Any())
        {
            throw new UserException("You can't unlink an attachment being an EDI document sent to the government.");
        }
    }
}
