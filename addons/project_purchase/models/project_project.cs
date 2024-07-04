csharp
public partial class Project {
    public int PurchaseOrdersCount { get; set; }

    public Account.AnalyticAccount AnalyticAccountId { get; set; }

    public void ComputePurchaseOrdersCount() {
        var data = Env.Model("Purchase.Order.Line").ReadGroup(
            new[] { new[] { "AnalyticDistribution", "in", AnalyticAccountId.Ids } },
            new[] { "AnalyticDistribution" },
            new[] { "__count" }
        );
        var dataDict = data.ToDictionary(x => int.Parse((string)x[0]), x => (int)x[1]);
        PurchaseOrdersCount = dataDict.GetValueOrDefault(AnalyticAccountId.Id, 0);
    }

    public object ActionOpenProjectPurchaseOrders() {
        var purchaseOrders = Env.Model("Purchase.Order.Line").Search(
            new[] { new[] { "AnalyticDistribution", "in", AnalyticAccountId.Ids } }
        ).OrderIds;
        return new {
            Name = "Purchase Orders",
            Type = "ir.actions.act_window",
            ResModel = "Purchase.Order",
            Views = new[] { new[] { false, "tree" }, new[] { false, "form" } },
            Domain = new[] { new[] { "Id", "in", purchaseOrders.Ids } },
            Context = new { ProjectId = this.Id }
        };
    }

    public object ActionProfitabilityItems(string sectionName, object domain = null, object resId = null) {
        if (sectionName == "PurchaseOrder") {
            return new {
                Name = "Purchase Order Items",
                Type = "ir.actions.act_window",
                ResModel = "Purchase.Order.Line",
                Views = new[] { new[] { false, "tree" }, new[] { false, "form" } },
                Domain = domain,
                Context = new { Create = false, Edit = false },
                ResId = resId,
                ViewMode = resId != null ? "form" : null
            };
        }
        return Env.Model("Project.Project").Call("ActionProfitabilityItems", sectionName, domain, resId);
    }

    public List<object> GetStatButtons() {
        var buttons = Env.Model("Project.Project").Call("GetStatButtons");
        if (Env.User.HasGroup("Purchase.GroupPurchaseUser")) {
            buttons.Add(new {
                Icon = "credit-card",
                Text = "Purchase Orders",
                Number = PurchaseOrdersCount,
                ActionType = "object",
                Action = "ActionOpenProjectPurchaseOrders",
                Show = PurchaseOrdersCount > 0,
                Sequence = 36
            });
        }
        return buttons;
    }

    public object GetProfitabilityAALDomain() {
        var baseDomain = Env.Model("Project.Project").Call("GetProfitabilityAALDomain");
        return new[] {
            new[] { "OR", new[] { new[] { "MoveLineId", "=", false }, new[] { "MoveLineId.PurchaseLineId", "=", false } } },
            baseDomain
        };
    }

    public object AddPurchaseItems(object profitabilityItems, object withAction = true) {
        return false;
    }

    public Dictionary<string, object> GetProfitabilityLabels() {
        var labels = Env.Model("Project.Project").Call("GetProfitabilityLabels");
        labels["PurchaseOrder"] = "Purchase Orders";
        return labels;
    }

    public Dictionary<string, int> GetProfitabilitySequencePerInvoiceType() {
        var sequencePerInvoiceType = Env.Model("Project.Project").Call("GetProfitabilitySequencePerInvoiceType");
        sequencePerInvoiceType["PurchaseOrder"] = 10;
        return sequencePerInvoiceType;
    }

    public object GetProfitabilityItems(object withAction = true) {
        var profitabilityItems = Env.Model("Project.Project").Call("GetProfitabilityItems", withAction);
        if (AnalyticAccountId != null) {
            var invoiceLines = Env.Model("Account.Move.Line").SearchFetch(
                new[] {
                    new[] { "ParentState", "in", new[] { "draft", "posted" } },
                    new[] { "AnalyticDistribution", "in", AnalyticAccountId.Ids },
                    new[] { "PurchaseLineId", "!=", false }
                },
                new[] { "ParentState", "CurrencyId", "PriceSubtotal", "AnalyticDistribution" }
            );
            var purchaseOrderLineInvoiceLineIds = GetAlreadyIncludedProfitabilityInvoiceLineIds();
            var hasPurchaseAccess = Env.User.HasGroup("Purchase.GroupPurchaseUser") || Env.User.HasGroup("Account.GroupAccountInvoice") || Env.User.HasGroup("Account.GroupAccountReadonly");
            withAction = withAction && hasPurchaseAccess;
            if (invoiceLines.Count > 0) {
                var amountInvoiced = 0.0;
                var amountToInvoice = 0.0;
                purchaseOrderLineInvoiceLineIds.AddRange(invoiceLines.Ids);
                foreach (var line in invoiceLines) {
                    var priceSubtotal = line.CurrencyId.Convert(line.PriceSubtotal, this.CurrencyId, this.CompanyId);
                    var analyticContribution = line.AnalyticDistribution.Sum(x => {
                        var ids = x.Key.Split(',');
                        return ids.Contains(AnalyticAccountId.Id.ToString()) ? x.Value / 100.0 : 0;
                    });
                    var cost = priceSubtotal * analyticContribution;
                    if (line.ParentState == "posted") {
                        amountInvoiced -= cost;
                    } else {
                        amountToInvoice -= cost;
                    }
                }
                var costs = profitabilityItems["costs"];
                var sectionId = "PurchaseOrder";
                var purchaseOrderCosts = new {
                    Id = sectionId,
                    Sequence = GetProfitabilitySequencePerInvoiceType()[sectionId],
                    Billed = amountInvoiced,
                    ToBill = amountToInvoice
                };
                if (withAction) {
                    var args = new[] { sectionId, new[] { new[] { "Id", "in", invoiceLines.PurchaseLineId.Ids } } };
                    if (invoiceLines.PurchaseLineId.Count == 1) {
                        args.Add(invoiceLines.PurchaseLineId[0].Id);
                    }
                    var action = new {
                        Name = "action_profitability_items",
                        Type = "object",
                        Args = Json.Serialize(args)
                    };
                    purchaseOrderCosts = new { purchaseOrderCosts, Action = action };
                }
                costs["data"].Add(purchaseOrderCosts);
                costs["total"]["billed"] += amountInvoiced;
                costs["total"]["to_bill"] += amountToInvoice;
            }
            var domain = new[] {
                new[] { "MoveId.MoveType", "in", new[] { "in_invoice", "in_refund" } },
                new[] { "ParentState", "in", new[] { "draft", "posted" } },
                new[] { "PriceSubtotal", ">", 0 },
                new[] { "Id", "not in", purchaseOrderLineInvoiceLineIds }
            };
            GetCostsItemsFromPurchase(domain, profitabilityItems, withAction);
        }
        return profitabilityItems;
    }

    // Placeholder for missing methods
    private object GetAlreadyIncludedProfitabilityInvoiceLineIds() {
        return new List<object>();
    }

    private void GetCostsItemsFromPurchase(object domain, object profitabilityItems, object withAction = true) {
        // Placeholder for missing method implementation
    }
}
