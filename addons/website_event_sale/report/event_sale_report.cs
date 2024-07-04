C#
public partial class WebsiteEventSaleReport
{
    public bool IsPublished { get; set; }

    public virtual object _SelectClause(params object[] select)
    {
        return Env.Call("super", "_SelectClause", "event_event.is_published as is_published", select);
    }
}
