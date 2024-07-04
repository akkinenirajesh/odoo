csharp
public partial class LoyaltyResPartner {
    public int LoyaltyCardCount { get; set; }
    
    public void ComputeCountActiveCards() {
        var loyaltyGroups = Env.Get("Loyalty.Card").ReadGroup(
            domain: new List<object>() {
                new List<object>() { "or", new List<object>() { "company_id", "=", null }, new List<object>() { "company_id", "in", Env.Companies.Ids } },
                new List<object>() { "partner_id", "in", Env.Get("Loyalty.ResPartner").Search(new List<object>() { "id", "child_of", this.Id }).Ids },
                new List<object>() { "points", ">", 0 },
                new List<object>() { "program_id.active", "=", true },
                new List<object>() { "or", new List<object>() { "expiration_date", ">=", Env.Date.Today }, new List<object>() { "expiration_date", "=", null } }
            },
            groupby: new List<object>() { "partner_id" },
            aggregates: new List<object>() { "__count" }
        );

        this.LoyaltyCardCount = 0;

        foreach (var group in loyaltyGroups) {
            var partner = group[0] as LoyaltyResPartner;
            var count = (int)group[1];

            while (partner != null) {
                if (partner.Id == this.Id) {
                    partner.LoyaltyCardCount += count;
                }

                partner = partner.Parent;
            }
        }
    }

    public void ActionViewLoyaltyCards() {
        var action = Env.Get("ir.actions.act_window").ForXmlId("loyalty.loyalty_card_action");
        var allChild = Env.Get("Loyalty.ResPartner").Search(new List<object>() { "id", "child_of", this.Id });
        action.Domain = new List<object>() { "partner_id", "in", allChild.Ids };
        action.Context = new Dictionary<string, object>() { { "search_default_active", true }, { "create", false } };

        Env.ExecuteAction(action);
    }
}
