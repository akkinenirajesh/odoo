csharp
public partial class SaleTimesheet.ProjectUpdate 
{
    public SaleTimesheet.ProjectUpdateService[] GetServicesValues(Sale.Project project)
    {
        if (!project.AllowBillable)
        {
            return new SaleTimesheet.ProjectUpdateService[0];
        }

        var services = new List<SaleTimesheet.ProjectUpdateService>();
        var sols = Env.Ref<Sale.OrderLine>("sale.order.line").Search(project.GetSaleItemsDomain(new[] { new Tuple<string, object>("IsDownpayment", false) }));
        var productUomUnit = Env.Ref<Uom.ProductUom>("uom.product_uom_unit");
        var productUomHour = Env.Ref<Uom.ProductUom>("uom.product_uom_hour");
        var companyUom = Env.Company.TimesheetEncodeUomId;
        foreach (var sol in sols)
        {
            //We only want to consider hours and days for this calculation
            var isUnit = sol.ProductUom == productUomUnit;
            if (sol.ProductUom.CategoryId == companyUom.CategoryId || isUnit)
            {
                var productUomQty = sol.ProductUom.ComputeQuantity(sol.ProductUomQty, companyUom, false);
                var qtyDelivered = sol.ProductUom.ComputeQuantity(sol.QtyDelivered, companyUom, false);
                var qtyInvoiced = sol.ProductUom.ComputeQuantity(sol.QtyInvoiced, companyUom, false);
                var unit = isUnit ? sol.ProductUom : companyUom;
                services.Add(new SaleTimesheet.ProjectUpdateService()
                {
                    Name = sol.DisplayName,
                    SoldValue = productUomQty,
                    EffectiveValue = qtyDelivered,
                    RemainingValue = productUomQty - qtyDelivered,
                    InvoicedValue = qtyInvoiced,
                    Unit = unit.Name,
                    IsUnit = isUnit,
                    IsHour = unit == productUomHour,
                    Sol = sol
                });
            }
        }

        return services.ToArray();
    }

    public SaleTimesheet.ProjectUpdateProfitability GetProfitabilityValues(Sale.Project project)
    {
        var costsRevenues = project.AnalyticAccountId != null && project.AllowBillable;
        if (!(Env.User.HasGroup("project.group_project_manager") && costsRevenues))
        {
            return null;
        }

        var profitabilityItems = project.GetProfitabilityItems(false);
        var costs = profitabilityItems.Costs.Total.Values.Sum();
        var revenues = profitabilityItems.Revenues.Total.Values.Sum();
        var margin = revenues + costs;
        return new SaleTimesheet.ProjectUpdateProfitability()
        {
            AnalyticAccountId = project.AnalyticAccountId,
            Costs = profitabilityItems.Costs,
            Revenues = profitabilityItems.Revenues,
            Total = new SaleTimesheet.ProjectUpdateProfitabilityTotal()
            {
                Costs = costs,
                Revenues = revenues,
                Margin = margin,
                MarginPercentage = (float)Math.Round((costs != 0 ? margin / -costs : 0) * 100, 0),
            },
            Labels = project.GetProfitabilityLabels(),
        };
    }

    public SaleTimesheet.ProjectUpdate GetTemplateValues(Sale.Project project)
    {
        var templateValues = Env.Ref<SaleTimesheet.ProjectUpdate>("sale_timesheet.project_update").GetTemplateValues(project);
        var services = GetServicesValues(project);
        var profitabilityValues = GetProfitabilityValues(project);
        var showProfitability = profitabilityValues != null && profitabilityValues.AnalyticAccountId != null && (profitabilityValues.Costs != null || profitabilityValues.Revenues != null);
        var showSold = templateValues.Project.AllowBillable && services.Length > 0;
        return new SaleTimesheet.ProjectUpdate()
        {
            AllowBillable = templateValues.Project.AllowBillable,
            AnalyticAccountId = templateValues.Project.AnalyticAccountId,
            ShowActivities = templateValues.ShowActivities || showProfitability || showSold,
            ShowProfitability = showProfitability,
            ShowSold = showSold,
            Services = services,
            Profitability = profitabilityValues,
            FormatValue = (value, isHour) => isHour ? FormatDuration(value) : ((float)Math.Round(value, 2)).ToString(),
        };
    }

    private string FormatDuration(double value)
    {
        // Implement format duration logic here
        return string.Empty;
    }
}
