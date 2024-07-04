csharp
public partial class PosOrderLine {
    public PosOrderLine(BuviContext env, int id)
    {
        Env = env;
        Id = id;
    }

    public BuviContext Env { get; }
    public int Id { get; }

    public void Create(List<Dictionary<string, object>> valsList)
    {
        foreach (var vals in valsList)
        {
            if (vals.ContainsKey("ComboParentUuid"))
            {
                var comboParentUuid = vals["ComboParentUuid"].ToString();
                var comboParentId = Env.Search<PosOrderLine>(new Dictionary<string, object> { { "Uuid", comboParentUuid } }).FirstOrDefault()?.Id;
                vals["ComboParentId"] = comboParentId;
                vals.Remove("ComboParentUuid");
            }
        }

        Env.Create<PosOrderLine>(valsList);
    }

    public void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("ComboParentUuid"))
        {
            var comboParentUuid = vals["ComboParentUuid"].ToString();
            var comboParentId = Env.Search<PosOrderLine>(new Dictionary<string, object> { { "Uuid", comboParentUuid } }).FirstOrDefault()?.Id;
            vals["ComboParentId"] = comboParentId;
            vals.Remove("ComboParentUuid");
        }
        Env.Write(Id, vals);
    }
}

public partial class PosOrder {
    public PosOrder(BuviContext env, int id)
    {
        Env = env;
        Id = id;
    }

    public BuviContext Env { get; }
    public int Id { get; }

    public List<Dictionary<string, object>> LoadPosSelfDataDomain(Dictionary<string, object> data)
    {
        return new List<Dictionary<string, object>> { new Dictionary<string, object> { { "Id", false } } };
    }

    public void SyncFromUi(List<Dictionary<string, object>> orders)
    {
        foreach (var order in orders)
        {
            if (order.ContainsKey("Id"))
            {
                var orderId = Convert.ToInt32(order["Id"]);
                var oldOrder = Env.Browse<PosOrder>(orderId);
                if (oldOrder.Takeaway)
                {
                    order["Takeaway"] = oldOrder.Takeaway;
                }
            }
        }

        Env.SyncFromUi<PosOrder>(orders);
    }

    public void ProcessSavedOrder(Dictionary<string, object> draft)
    {
        Env.CallMethod<PosOrder>(Id, "_ProcessSavedOrder", draft);
        if (!Env.Context.ContainsKey("FromSelf") || !Convert.ToBoolean(Env.Context["FromSelf"]))
        {
            SendNotification(Id);
        }
    }

    public void RemoveFromUi(List<int> serverIds)
    {
        var orderIds = Env.Browse<PosOrder>(serverIds);
        foreach (var orderId in orderIds)
        {
            Env.CallMethod<PosOrder>(orderId, "State", "cancel");
        }
        SendNotification(orderIds);
        Env.RemoveFromUi<PosOrder>(serverIds);
    }

    public void SendNotification(List<int> orderIds)
    {
        foreach (var orderId in orderIds)
        {
            var order = Env.Browse<PosOrder>(orderId);
            var orderData = order.Read(order.LoadPosSelfDataFields(order.ConfigId.Id), load: false);
            var orderLinesData = order.Lines.Read(order.LoadPosSelfDataFields(order.ConfigId.Id), load: false);
            var paymentData = order.PaymentIds.Read(order.PaymentIds.LoadPosDataFields(order.ConfigId.Id), load: false);
            var paymentMethodData = order.PaymentIds.Select(x => x.PaymentMethodId).Read(Env.Browse<PosPaymentMethod>().LoadPosDataFields(order.ConfigId.Id), load: false);
            var customAttributeValueData = order.Lines.CustomAttributeValueIds.Read(order.Lines.CustomAttributeValueIds.LoadPosDataFields(order.ConfigId.Id), load: false);

            Env.CallMethod<PosOrder>(orderId, "_Notify", "ORDER_STATE_CHANGED", new Dictionary<string, object> { 
                { "pos.order", orderData },
                { "pos.order.line", orderLinesData },
                { "pos.payment", paymentData },
                { "pos.payment.method", paymentMethodData },
                { "product.attribute.custom.value", customAttributeValueData },
            });
        }
    }

    public Dictionary<string, object> GetStandaloneSelfOrder()
    {
        var orders = Env.SearchRead<PosOrder>(new List<Dictionary<string, object>> {
            Env.CheckCompanyDomain(),
            new Dictionary<string, object> { { "State", "draft" } },
            new Dictionary<string, object> { { "PosReference", "ilike", "Kiosk" } },
            new Dictionary<string, object> { { "PosReference", "ilike", "Self-Order" } },
            new Dictionary<string, object> { { "TableId", false } }
        }, new List<string>(), load: false);

        return new Dictionary<string, object> { { "pos.order", orders } };
    }
}
