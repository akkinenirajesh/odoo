csharp
public partial class WebsiteSaleMondialRelay.ResPartner 
{
    public bool CanBeEditedByCurrentCustomer(WebsiteSale.SaleOrder saleOrder, string mode)
    {
        return base.CanBeEditedByCurrentCustomer(saleOrder, mode) && !this.IsMondialRelay;
    }
}
