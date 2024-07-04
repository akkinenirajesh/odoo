csharp
public partial class WebsiteEventTag
{
    public WebsiteEventTag()
    {
    }

    public virtual WebsiteWebsite Website { get; set; }

    public virtual void DefaultGet(ref Dictionary<string, object> fieldsList)
    {
        if (Env.Context.ContainsKey("default_website_id"))
        {
            fieldsList["Website"] = Env.Context["default_website_id"];
        }
    }
}
