csharp
public partial class ProjectProject
{
    public override string ToString()
    {
        return Name;
    }

    public Dictionary<string, object> MapTasksDefaultValues()
    {
        var defaults = base.MapTasksDefaultValues();
        defaults["SaleLine"] = null;
        return defaults;
    }

    public void ComputePartner()
    {
        if (!AllowBillable || (Company != null && Partner?.Company != null && Company != Partner.Company))
        {
            Partner = null;
        }
    }

    public void ComputeSaleLine()
    {
        if (SaleLine != null && (Partner == null || SaleLine.OrderPartner.CommercialPartner != Partner.CommercialPartner))
        {
            SaleLine = null;
        }
    }

    public List<ProjectProject> GetProjectsForInvoiceStatus(string invoiceStatus)
    {
        // Implementation depends on how you want to handle database queries in your C# environment
        // This is a placeholder for the logic
        return new List<ProjectProject>();
    }

    public void ComputeHasAnySoToInvoice()
    {
        if (Id == 0)
        {
            HasAnySoToInvoice = false;
            return;
        }

        var projectToInvoice = GetProjectsForInvoiceStatus("to invoice");
        HasAnySoToInvoice = projectToInvoice.Contains(this);
    }

    public void ComputeSaleOrderCount()
    {
        var saleOrderItemsPerProjectId = FetchSaleOrderItemsPerProjectId(new Dictionary<string, List<object>> { { "project.task", new List<object> { new List<object> { "IsClosed", "=", false } } } });
        var saleOrderLines = saleOrderItemsPerProjectId.GetValueOrDefault(Id, new List<Sale.SaleOrderLine>());
        SaleOrderLineCount = saleOrderLines.Count;
        SaleOrderCount = saleOrderLines.Select(sol => sol.Order).Distinct().Count();
    }

    public void ComputeInvoiceCount()
    {
        // Implementation depends on how you want to handle database queries in your C# environment
        // This is a placeholder for the logic
        InvoiceCount = 0;
    }

    public void ComputeDisplaySalesStatButtons()
    {
        DisplaySalesStatButtons = AllowBillable && Partner != null;
    }

    public Dictionary<string, object> ActionCustomerPreview()
    {
        return new Dictionary<string, object>
        {
            { "type", "ir.actions.act_url" },
            { "target", "self" },
            { "url", GetPortalUrl() }
        };
    }

    // Add other action methods here...
}
