csharp
public partial class ResPartner
{
    public IEnumerable<DeclarationOfIntent> L10nItEdiDoiIds
    {
        get
        {
            return Env.Set<DeclarationOfIntent>()
                .Where(d => d.Partner == this && d.Company == Env.Company);
        }
    }

    public ActionResult L10nItEdiDoiActionOpenDeclarations()
    {
        return new ActionResult
        {
            Name = $"Declaration of Intent of {this.DisplayName}",
            Type = ActionType.Window,
            Model = typeof(DeclarationOfIntent),
            Domain = new Domain(d => d.Partner == this.CommercialPartner),
            Views = new[]
            {
                new ActionView { ViewType = ViewType.Tree, ViewId = "Partner.ViewL10nItEdiDoiTree" },
                new ActionView { ViewType = ViewType.Form, ViewId = "Partner.ViewL10nItEdiDoiForm" }
            },
            Context = new Dictionary<string, object>
            {
                { "default_partner_id", this.Id }
            }
        };
    }
}
