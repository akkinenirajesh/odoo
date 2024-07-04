csharp
public partial class SalePurchaseStock.PurchaseOrder
{
    public int SaleOrderCount { get; set; }

    public void ComputeSaleOrderCount()
    {
        SaleOrderCount = 0;

        // Implementation for calculating SaleOrderCount based on order_line.move_dest_ids.group_id.sale_id and order_line.move_ids.move_dest_ids.group_id.sale_id.
    }

    public List<Sale.SaleOrder> GetSaleOrders()
    {
        // Implementation for retrieving sale orders based on order_line.move_dest_ids.group_id.sale_id and order_line.move_ids.move_dest_ids.group_id.sale_id.
        // Consider using LINQ for efficient querying.
        return new List<Sale.SaleOrder>();
    }
}
