C#
public partial class UtmCampaign {
    public void ComputeQuotationCount() {
        var quotationData = Env.GetModel("Sale.Order").ReadGroup(new []{new[] { "Campaign", this.Id }}, new []{"Campaign"}, new []{"Count"});
        var dataMap = quotationData.ToDictionary(d => (int)d["Campaign"], d => (int)d["Count"]);
        QuotationCount = dataMap.ContainsKey(Id) ? dataMap[Id] : 0;
    }

    public void ComputeSaleInvoicedAmount() {
        Env.GetModel("Account.MoveLine").FlushModel(new []{"Balance", "Move", "Account", "DisplayType"});
        Env.GetModel("Account.Move").FlushModel(new []{"State", "Campaign", "MoveType"});
        var query = @"SELECT move.Campaign, -SUM(line.Balance) as PriceSubtotal
                    FROM Account.MoveLine line
                    INNER JOIN Account.Move move ON line.Move = move.Id
                    WHERE move.State not in ('Draft', 'Cancel')
                        AND move.Campaign IN @Ids
                        AND move.MoveType IN ('OutInvoice', 'OutRefund', 'InInvoice', 'InRefund', 'OutReceipt', 'InReceipt')
                        AND line.Account IS NOT NULL
                        AND line.DisplayType = 'Product'
                    GROUP BY move.Campaign";

        var queryRes = Env.GetModel("Account.MoveLine").ExecuteQuery(query, new []{new []{Id}});

        var campaigns = this;
        foreach (var datum in queryRes) {
            var campaign = Env.GetModel("Sale.UtmCampaign").Browse(datum["Campaign"]);
            campaign.InvoicedAmount = (decimal)datum["PriceSubtotal"];
            campaigns |= campaign;
        }
        foreach (var campaign in (this - campaigns)) {
            campaign.InvoicedAmount = 0;
        }
    }

    public void ActionRedirectToQuotations() {
        var action = Env.GetModel("Ir.Actions.Actions")._ForXmlId("Sale.ActionQuotationsWithOnboarding");
        action["Domain"] = new []{new []{ "Campaign", Id }};
        action["Context"] = new { DefaultCampaign = Id };
    }

    public void ActionRedirectToInvoiced() {
        var action = Env.GetModel("Ir.Actions.Actions")._ForXmlId("Account.ActionMoveJournalLine");
        var invoices = Env.GetModel("Account.Move").Search(new []{new []{ "Campaign", Id }});
        action["Context"] = new { Create = false, Edit = false, ViewNoMaturity = true };
        action["Domain"] = new []{
            new []{ "Id", invoices.Ids },
            new []{ "MoveType", new []{ "OutInvoice", "OutRefund", "InInvoice", "InRefund", "OutReceipt", "InReceipt" } },
            new []{ "State", new []{ "Draft", "Cancel" }, "not in" }
        };
    }
}
