c#
public partial class PurchaseOrder 
{
    public virtual void ActionRfqSend()
    {
        // TODO: Implement ActionRfqSend
    }

    public virtual void PrintQuotation()
    {
        // TODO: Implement PrintQuotation
    }

    public virtual void ButtonApprove(bool force = false)
    {
        // TODO: Implement ButtonApprove
    }

    public virtual void ButtonDraft()
    {
        // TODO: Implement ButtonDraft
    }

    public virtual void ButtonConfirm()
    {
        // TODO: Implement ButtonConfirm
    }

    public virtual void ButtonCancel()
    {
        // TODO: Implement ButtonCancel
    }

    public virtual void ButtonUnlock()
    {
        // TODO: Implement ButtonUnlock
    }

    public virtual void ButtonDone()
    {
        // TODO: Implement ButtonDone
    }

    public virtual void OnChangePartner()
    {
        // TODO: Implement OnChangePartner
    }

    public virtual void _ComputeTaxId()
    {
        // TODO: Implement _ComputeTaxId
    }

    public virtual void OnChangePartnerIdWarning()
    {
        // TODO: Implement OnChangePartnerIdWarning
    }

    public virtual void MessagePost(Dictionary<string, object> kwargs)
    {
        // TODO: Implement MessagePost
    }

    public virtual void _NotifyGetRecipientsGroups(object message, string modelDescription, Dictionary<string, object> msgVals = null)
    {
        // TODO: Implement _NotifyGetRecipientsGroups
    }

    public virtual void _NotifyByEmailPrepareRenderingContext(object message, Dictionary<string, object> msgVals = null, string modelDescription = null, bool forceEmailCompany = false, bool forceEmailLang = false)
    {
        // TODO: Implement _NotifyByEmailPrepareRenderingContext
    }

    public virtual void _TrackSubtype(Dictionary<string, object> initValues)
    {
        // TODO: Implement _TrackSubtype
    }

    public virtual void _ComputeDatePlanned()
    {
        // TODO: Implement _ComputeDatePlanned
    }

    public virtual void OnChangeDatePlanned()
    {
        // TODO: Implement OnChangeDatePlanned
    }

    public virtual void Write(Dictionary<string, object> vals)
    {
        // TODO: Implement Write
    }

    public virtual void Create(Dictionary<string, object> vals)
    {
        // TODO: Implement Create
    }

    public virtual void _UnlinkIfCancelled()
    {
        // TODO: Implement _UnlinkIfCancelled
    }

    public virtual void Copy(Dictionary<string, object> defaultValues = null)
    {
        // TODO: Implement Copy
    }

    public virtual void _MustDeleteDatePlanned(string fieldName)
    {
        // TODO: Implement _MustDeleteDatePlanned
    }

    public virtual string _GetReportBaseFilename()
    {
        // TODO: Implement _GetReportBaseFilename
    }

    public virtual void _ComputeDateCalendarStart()
    {
        // TODO: Implement _ComputeDateCalendarStart
    }

    public virtual void _ComputeCurrencyRate()
    {
        // TODO: Implement _ComputeCurrencyRate
    }

    public virtual void _ComputeDisplayName()
    {
        // TODO: Implement _ComputeDisplayName
    }

    public virtual void _ComputeReceiptReminderEmail()
    {
        // TODO: Implement _ComputeReceiptReminderEmail
    }

    public virtual void _ComputeTaxCountry()
    {
        // TODO: Implement _ComputeTaxCountry
    }

    public virtual void _ComputeTaxTotals()
    {
        // TODO: Implement _ComputeTaxTotals
    }

    public virtual void _AmountAll()
    {
        // TODO: Implement _AmountAll
    }

    public virtual void _GetInvoiced()
    {
        // TODO: Implement _GetInvoiced
    }

    public virtual void _ComputeInvoice()
    {
        // TODO: Implement _ComputeInvoice
    }

    public virtual void _CheckOrderLineCompanyId()
    {
        // TODO: Implement _CheckOrderLineCompanyId
    }

    public virtual void _ComputeAccessUrl()
    {
        // TODO: Implement _ComputeAccessUrl
    }

    public virtual void ActionCreateInvoice()
    {
        // TODO: Implement ActionCreateInvoice
    }

    public virtual Dictionary<string, object> _PrepareInvoice()
    {
        // TODO: Implement _PrepareInvoice
    }

    public virtual void ActionViewInvoice(object invoices = null)
    {
        // TODO: Implement ActionViewInvoice
    }

    public virtual Dictionary<string, object> RetrieveDashboard()
    {
        // TODO: Implement RetrieveDashboard
    }

    public virtual void _SendReminderMail(bool sendSingle = false)
    {
        // TODO: Implement _SendReminderMail
    }

    public virtual void SendReminderPreview()
    {
        // TODO: Implement SendReminderPreview
    }

    public virtual void _SendReminderOpenComposer(int templateId)
    {
        // TODO: Implement _SendReminderOpenComposer
    }

    public virtual object _GetOrdersToRemind()
    {
        // TODO: Implement _GetOrdersToRemind
    }

    public virtual Dictionary<string, object> _DefaultOrderLineValues(string childField = null)
    {
        // TODO: Implement _DefaultOrderLineValues
    }

    public virtual void ActionAddFromCatalog()
    {
        // TODO: Implement ActionAddFromCatalog
    }

    public virtual Dictionary<string, object> _GetActionAddFromCatalogExtraContext()
    {
        // TODO: Implement _GetActionAddFromCatalogExtraContext
    }

    public virtual object _GetProductCatalogDomain()
    {
        // TODO: Implement _GetProductCatalogDomain
    }

    public virtual Dictionary<string, object> _GetProductCatalogOrderData(object products, Dictionary<string, object> kwargs = null)
    {
        // TODO: Implement _GetProductCatalogOrderData
    }

    public virtual object _GetProductCatalogRecordLines(object productIds, string childField = null)
    {
        // TODO: Implement _GetProductCatalogRecordLines
    }

    public virtual Dictionary<string, object> _GetProductPriceAndData(object product)
    {
        // TODO: Implement _GetProductPriceAndData
    }

    public virtual string GetConfirmUrl(string confirmType = null)
    {
        // TODO: Implement GetConfirmUrl
    }

    public virtual string GetUpdateUrl()
    {
        // TODO: Implement GetUpdateUrl
    }

    public virtual void ConfirmReminderMail(object confirmedDate = null)
    {
        // TODO: Implement ConfirmReminderMail
    }

    public virtual bool _ApprovalAllowed()
    {
        // TODO: Implement _ApprovalAllowed
    }

    public virtual void _ConfirmReceptionMail()
    {
        // TODO: Implement _ConfirmReceptionMail
    }

    public virtual object GetLocalizedDatePlanned(object datePlanned = null)
    {
        // TODO: Implement GetLocalizedDatePlanned
    }

    public virtual object GetOrderTimezone()
    {
        // TODO: Implement GetOrderTimezone
    }

    public virtual void _UpdateDatePlannedForLines(List<object> updatedDates)
    {
        // TODO: Implement _UpdateDatePlannedForLines
    }

    public virtual object _UpdateOrderLineInfo(int productId, object quantity, Dictionary<string, object> kwargs = null)
    {
        // TODO: Implement _UpdateOrderLineInfo
    }

    public virtual object _CreateUpdateDateActivity(List<object> updatedDates)
    {
        // TODO: Implement _CreateUpdateDateActivity
    }

    public virtual void _UpdateUpdateDateActivity(List<object> updatedDates, object activity)
    {
        // TODO: Implement _UpdateUpdateDateActivity
    }

    public virtual Tuple<Dictionary<string, object>, Dictionary<string, object>> _WritePartnerValues(Dictionary<string, object> vals)
    {
        // TODO: Implement _WritePartnerValues
    }

    public virtual bool _IsReadonly()
    {
        // TODO: Implement _IsReadonly
    }

    public virtual List<object> _GetEdiBuilders()
    {
        // TODO: Implement _GetEdiBuilders
    }
}
