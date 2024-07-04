C#
public partial class SaleOrderLine 
{
    public virtual SaleReward RewardId { get; set; }

    public List<SaleOrderField> GetSaleOrderFields()
    {
        List<SaleOrderField> fieldNames = Env.CallMethod<List<SaleOrderField>>(this, "Sale.SaleOrderLine", "_get_sale_order_fields");
        fieldNames.Add(new SaleOrderField { Name = "RewardId" });
        return fieldNames;
    }
}
