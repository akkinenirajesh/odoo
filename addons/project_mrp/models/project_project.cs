csharp
public partial class Project
{
    public virtual ActionViewMrpProduction ActionViewMrpProduction()
    {
        var action = Env.Ref<IrActionsActions>("mrp.mrp_production_action");
        action.Domain = new[] { new[] { "Id", "in", Env.Ref<AccountAnalyticAccount>("analytic_account_id").ProductionIds.Select(x => x.Id).ToArray() } };
        action.Context = new Dictionary<string, object>() { { "default_analytic_account_id", Env.Ref<AccountAnalyticAccount>("analytic_account_id").Id } };
        if (ProductionCount == 1)
        {
            action.ViewMode = "form";
            action.ResId = Env.Ref<AccountAnalyticAccount>("analytic_account_id").ProductionIds.First().Id;
            action.Views = action.Views.Where(x => x.Item2 == "form").ToList() ?? new List<Tuple<long, string>>() { new Tuple<long, string>(0, "form") };
        }
        return action;
    }

    public virtual ActionViewMrpBom ActionViewMrpBom()
    {
        var action = Env.Ref<AccountAnalyticAccount>("analytic_account_id").ActionViewMrpBom();
        if (BomCount > 1)
        {
            action.ViewMode = "tree,form,kanban";
        }
        return action;
    }

    public virtual ActionViewWorkorder ActionViewWorkorder()
    {
        var action = Env.Ref<AccountAnalyticAccount>("analytic_account_id").ActionViewWorkorder();
        if (WorkorderCount > 1)
        {
            action.ViewMode = "tree,form,kanban,calendar,pivot,graph";
        }
        return action;
    }

    public virtual Dictionary<string, string> GetProfitabilityLabels()
    {
        var labels = base.GetProfitabilityLabels();
        labels["manufacturing_order"] = "Manufacturing Orders";
        return labels;
    }

    public virtual Dictionary<string, int> GetProfitabilitySequencePerInvoiceType()
    {
        var sequencePerInvoiceType = base.GetProfitabilitySequencePerInvoiceType();
        sequencePerInvoiceType["manufacturing_order"] = 12;
        return sequencePerInvoiceType;
    }

    public virtual List<List<object>> GetProfitabilityAalDomain()
    {
        return new List<List<object>>()
        {
            base.GetProfitabilityAalDomain(),
            new List<object>() { "category", "!=", "manufacturing_order" }
        };
    }

    public virtual Dictionary<string, object> GetProfitabilityItems(bool withAction = true)
    {
        var profitabilityItems = base.GetProfitabilityItems(withAction);
        var mrpCategory = "manufacturing_order";
        var mrpAalReadGroup = Env.Ref<AccountAnalyticLine>()._ReadGroup(
            new List<List<object>>()
            {
                new List<object>() { "auto_account_id", "in", Env.Ref<AccountAnalyticAccount>("analytic_account_id").Ids },
                new List<object>() { "category", "=", mrpCategory }
            },
            new[] { "currency_id" },
            new[] { "amount:sum" }
        );
        if (mrpAalReadGroup.Any())
        {
            var canSeeManufacturingOrder = withAction && Env.User.HasGroup("mrp.group_mrp_user");
            var totalAmount = 0.0;
            foreach (var currencyAmountSummed in mrpAalReadGroup)
            {
                totalAmount += currencyAmountSummed.CurrencyId.Convert(currencyAmountSummed["amount:sum"], CurrencyId, CompanyId);
            }
            var mrpCosts = new Dictionary<string, object>()
            {
                { "id", mrpCategory },
                { "sequence", GetProfitabilitySequencePerInvoiceType()[mrpCategory] },
                { "billed", totalAmount },
                { "to_bill", 0.0 }
            };
            if (canSeeManufacturingOrder)
            {
                mrpCosts["action"] = new Dictionary<string, object>() { { "name", "action_view_mrp_production" }, { "type", "object" } };
            }
            var costs = profitabilityItems["costs"] as Dictionary<string, object>;
            (costs["data"] as List<object>).Add(mrpCosts);
            (costs["total"] as Dictionary<string, object>)["billed"] += mrpCosts["billed"];
        }
        return profitabilityItems;
    }

    public virtual List<Dictionary<string, object>> GetStatButtons()
    {
        var buttons = base.GetStatButtons();
        if (Env.User.HasGroup("mrp.group_mrp_user"))
        {
            buttons.AddRange(new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    { "icon", "flask" },
                    { "text", "Bills of Materials" },
                    { "number", BomCount },
                    { "action_type", "object" },
                    { "action", "action_view_mrp_bom" },
                    { "show", BomCount > 0 },
                    { "sequence", 35 }
                },
                new Dictionary<string, object>()
                {
                    { "icon", "wrench" },
                    { "text", "Manufacturing Orders" },
                    { "number", ProductionCount },
                    { "action_type", "object" },
                    { "action", "action_view_mrp_production" },
                    { "show", ProductionCount > 0 },
                    { "sequence", 46 }
                }
            });
        }
        return buttons;
    }
}
