C#
public partial class PurchaseOrder
{
    public PurchaseOrder()
    {
    }

    public List<EDIBuilder> EDIBuilders
    {
        get
        {
            return Env.Context.Get<List<EDIBuilder>>("EDIBuilders") ?? new List<EDIBuilder>();
        }
    }

    public List<EDIBuilder> GetEDIBuilders()
    {
        return Env.Context.Get<List<EDIBuilder>>("EDIBuilders") ?? new List<EDIBuilder>();
    }
}
