C#
public partial class WebsiteServerAction
{
    public virtual string XmlId { get; set; }
    public virtual string WebsitePath { get; set; }
    public virtual string WebsiteUrl { get; set; }
    public virtual bool WebsitePublished { get; set; }
    public virtual string State { get; set; }
    public virtual string Name { get; set; }
    public virtual string Type { get; set; }
    public virtual string Target { get; set; }
    public virtual int? Sequence { get; set; }
    public virtual IrModel Model { get; set; }
    public virtual int? ModelId { get; set; }
    public virtual int? BindingModelId { get; set; }
    public virtual int? FlowId { get; set; }
    public virtual IrModel BindingModel { get; set; }
    public virtual string BindingType { get; set; }
    public virtual string StateField { get; set; }
    public virtual string Condition { get; set; }
    public virtual string ConditionType { get; set; }
    public virtual string Code { get; set; }
    public virtual string Lang { get; set; }
    public virtual string ResModel { get; set; }
    public virtual int? ResId { get; set; }
    public virtual string Action { get; set; }
    public virtual string Result { get; set; }
    public virtual string Fields { get; set; }
    public virtual string TargetField { get; set; }
    public virtual string TargetModel { get; set; }
    public virtual int? TargetId { get; set; }
    public virtual bool UseContext { get; set; }
    public virtual bool Create { get; set; }
    public virtual bool Update { get; set; }
    public virtual bool Delete { get; set; }
    public virtual bool OnCreate { get; set; }
    public virtual bool OnWrite { get; set; }
    public virtual bool OnDelete { get; set; }
    public virtual ResUsers User { get; set; }
    public virtual int? GroupId { get; set; }
    public virtual int? TargetGroupId { get; set; }
    public virtual string Trigger { get; set; }
    public virtual string Cron { get; set; }
    public virtual string CronLog { get; set; }
    public virtual DateTime? CronNextCall { get; set; }
    public virtual ICollection<ResUsers> UserIds { get; set; }
    public virtual ICollection<IrAttachment> AttachmentIds { get; set; }

    public virtual void ComputeXmlId()
    {
        XmlId = Env.GetExternalId(this.Id);
    }

    public virtual void ComputeWebsiteUrl()
    {
        if (State == "code" && WebsitePublished)
        {
            WebsiteUrl = ComputeWebsiteUrl(WebsitePath, XmlId);
        }
        else
        {
            WebsiteUrl = null;
        }
    }

    public virtual string ComputeWebsiteUrl(string websitePath, string xmlId)
    {
        string baseUrl = Env.GetBaseUrl();
        string link = websitePath ?? xmlId ?? (this.Id != null ? this.Id.ToString() : string.Empty);
        if (baseUrl != null && link != string.Empty)
        {
            string path = string.Format("{0}/{1}", "/website/action", link);
            return urls.url_join(baseUrl, path);
        }
        return string.Empty;
    }

    public virtual Dictionary<string, object> GetEvalContext(WebsiteServerAction action)
    {
        Dictionary<string, object> evalContext = base.GetEvalContext(action);
        if (action.State == "code")
        {
            evalContext["request"] = Env.Request;
            evalContext["json"] = json_scriptsafe;
        }
        return evalContext;
    }

    public virtual object RunActionCodeMulti(Dictionary<string, object> evalContext = null)
    {
        object res = base.RunActionCodeMulti(evalContext);
        return evalContext.ContainsKey("response") ? evalContext["response"] : res;
    }
}
