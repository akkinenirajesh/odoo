csharp
public partial class MailTemplate {
    public virtual void ComputeTemplateCategory() {
        // ComputeTemplateCategory logic here
    }

    public virtual void ComputeCanWrite() {
        // ComputeCanWrite logic here
    }

    public virtual void ComputeIsTemplateEditor() {
        // ComputeIsTemplateEditor logic here
    }

    public virtual void SearchTemplateCategory(string operator, string value) {
        // SearchTemplateCategory logic here
    }

    public virtual void FixAttachmentOwnership() {
        // FixAttachmentOwnership logic here
    }

    public virtual void CheckAbstractModels(List<Dictionary<string, object>> valsList) {
        // CheckAbstractModels logic here
    }

    public virtual int OpenDeleteConfirmationModal() {
        // OpenDeleteConfirmationModal logic here
    }

    public virtual void UnlinkAction() {
        // UnlinkAction logic here
    }

    public virtual void CreateAction() {
        // CreateAction logic here
    }

    public virtual Dictionary<string, object> GenerateTemplateAttachments(List<int> resIds, List<string> renderFields, Dictionary<int, Dictionary<string, object>> renderResults) {
        // GenerateTemplateAttachments logic here
    }

    public virtual Dictionary<string, object> GenerateTemplateRecipients(List<int> resIds, List<string> renderFields, bool findOrCreatePartners, Dictionary<int, Dictionary<string, object>> renderResults) {
        // GenerateTemplateRecipients logic here
    }

    public virtual Dictionary<string, object> GenerateTemplateScheduledDate(List<int> resIds, Dictionary<int, Dictionary<string, object>> renderResults) {
        // GenerateTemplateScheduledDate logic here
    }

    public virtual Dictionary<string, object> GenerateTemplateStaticValues(List<int> resIds, List<string> renderFields, Dictionary<int, Dictionary<string, object>> renderResults) {
        // GenerateTemplateStaticValues logic here
    }

    public virtual Dictionary<int, Dictionary<string, object>> GenerateTemplate(List<int> resIds, List<string> renderFields, bool findOrCreatePartners) {
        // GenerateTemplate logic here
    }

    public virtual List<int> ParsePartnerTo(string partnerTo) {
        // ParsePartnerTo logic here
    }

    public virtual void SendCheckAccess(List<int> resIds) {
        // SendCheckAccess logic here
    }

    public virtual int SendMail(int resId, bool forceSend, bool raiseException, Dictionary<string, object> emailValues, string emailLayoutXmlid) {
        // SendMail logic here
    }
}
