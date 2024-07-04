csharp
public partial class SaleProject_ProductTemplate
{
    public virtual void ComputeServicePolicy()
    {
        this.ServicePolicy = GetGeneralToService(Env.Get<ProductTemplate>().InvoicePolicy, this.ServiceType);
        if (this.ServicePolicy == null && this.Type == "service")
        {
            this.ServicePolicy = "OrderedPrepaid";
        }
    }

    public virtual void InverseServicePolicy()
    {
        if (this.ServicePolicy != null)
        {
            var generalToService = GetServiceToGeneralMap();
            var general = generalToService.GetValueOrDefault(this.ServicePolicy);
            if (general != null)
            {
                Env.Get<ProductTemplate>().InvoicePolicy = general.Item1;
                this.ServiceType = general.Item2;
            }
        }
    }

    private SaleProject_ServiceInvoicingPolicy GetGeneralToService(string invoicePolicy, SaleProject_ServiceType serviceType)
    {
        var generalToService = GetGeneralToServiceMap();
        return generalToService.GetValueOrDefault((invoicePolicy, serviceType));
    }

    private Dictionary<(string, SaleProject_ServiceType), SaleProject_ServiceInvoicingPolicy> GetGeneralToServiceMap()
    {
        return new Dictionary<(string, SaleProject_ServiceType), SaleProject_ServiceInvoicingPolicy>()
        {
            { ("order", "manual"), SaleProject_ServiceInvoicingPolicy.OrderedPrepaid },
            { ("delivery", "milestones"), SaleProject_ServiceInvoicingPolicy.DeliveredMilestones },
            { ("delivery", "manual"), SaleProject_ServiceInvoicingPolicy.DeliveredManual },
        };
    }

    private Dictionary<SaleProject_ServiceInvoicingPolicy, (string, SaleProject_ServiceType)> GetServiceToGeneralMap()
    {
        return new Dictionary<SaleProject_ServiceInvoicingPolicy, (string, SaleProject_ServiceType)>()
        {
            { SaleProject_ServiceInvoicingPolicy.OrderedPrepaid, ("order", "manual") },
            { SaleProject_ServiceInvoicingPolicy.DeliveredMilestones, ("delivery", "milestones") },
            { SaleProject_ServiceInvoicingPolicy.DeliveredManual, ("delivery", "manual") },
        };
    }

    private (string, SaleProject_ServiceType) GetServiceToGeneral(SaleProject_ServiceInvoicingPolicy servicePolicy)
    {
        return GetServiceToGeneralMap().GetValueOrDefault(servicePolicy);
    }

    // Add other methods here...
}
