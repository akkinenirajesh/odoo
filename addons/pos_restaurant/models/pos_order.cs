csharp
public partial class PosOrder
{
    public virtual void RemoveFromUi(List<int> serverIds)
    {
        var tables = Env.Get<PosOrder>().Search(x => serverIds.Contains(x.Id)).Table;
        var orderIds = base.RemoveFromUi(serverIds);
        SendTableCountNotification(tables);
        return orderIds;
    }

    public virtual List<int> SyncFromUi(List<object> orders)
    {
        var result = base.SyncFromUi(orders);

        if (Env.Context.ContainsKey("table_ids"))
        {
            var orderIds = result["pos.order"].Select(x => (int)x["id"]).ToList();
            var tableOrders = this.Search(x => Env.Context["table_ids"].Contains(x.Table.Id) && x.State == "draft" && !orderIds.Contains(x.Id));

            if (tableOrders.Any())
            {
                var configId = tableOrders.First().ConfigId.Id;
                result["pos.order"].AddRange(tableOrders.Read(tableOrders.LoadPosDataFields(configId), false));
                result["pos.payment"].AddRange(tableOrders.PaymentIds.Read(tableOrders.PaymentIds.LoadPosDataFields(configId), false));
                result["pos.order.line"].AddRange(tableOrders.Lines.Read(tableOrders.Lines.LoadPosDataFields(configId), false));
                result["pos.pack.operation.lot"].AddRange(tableOrders.Lines.PackLotIds.Read(tableOrders.Lines.PackLotIds.LoadPosDataFields(configId), false));
                result["product.attribute.custom.value"].AddRange(tableOrders.Lines.CustomAttributeValueIds.Read(tableOrders.Lines.CustomAttributeValueIds.LoadPosDataFields(configId), false));
            }
        }

        return result["pos.order"].Select(x => (int)x["id"]).ToList();
    }

    public virtual int _ProcessSavedOrder(object draft)
    {
        var orderId = base._ProcessSavedOrder(draft);
        SendTableCountNotification(this.Table);
        return orderId;
    }

    public virtual void SendTableCountNotification(Restaurant.Table tableIds)
    {
        var messages = new List<object>();
        var aConfig = Env.Get<PosConfig>().Search(x => x.FloorIds.Contains(tableIds.FloorId)).FirstOrDefault();
        if (aConfig != null && aConfig.CurrentSessionId != null)
        {
            var orderCount = aConfig.GetTablesOrderCountAndPrintingChanges();
            messages.Add(new { Type = "TABLE_ORDER_COUNT", Value = orderCount });
        }
        if (messages.Any())
        {
            aConfig._Notify(messages, false);
        }
    }
}
