csharp
public partial class SaleOrder {
    public decimal Margin { get; set; }
    public float MarginPercent { get; set; }

    public void ComputeMargin() {
        if (Env.Context.ContainsKey("active_id"))
        {
            // Single record case
            this.Margin = Env.Ref<SaleOrderLine>(Env.Context["active_id"]).Sum(x => x.Margin);
            this.MarginPercent = this.AmountUntaxed > 0 ? this.Margin / this.AmountUntaxed : 0;
        }
        else
        {
            // Batch records case
            var groupedOrderLinesData = Env.Ref<SaleOrderLine>().ReadGroup(
                new List<object> {
                    new { order_id = Env.Current.Ids }
                }, new List<string> { "order_id" }, new List<string> { "margin:sum" }
            );
            var mappedData = groupedOrderLinesData.ToDictionary(x => x.order_id, x => x.margin);
            foreach (var order in Env.Current)
            {
                order.Margin = mappedData.TryGetValue(order.Id, out var margin) ? margin : 0;
                order.MarginPercent = order.AmountUntaxed > 0 ? order.Margin / order.AmountUntaxed : 0;
            }
        }
    }
}
