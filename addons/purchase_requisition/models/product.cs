csharp
public partial class ProductProduct {
    public object _prepare_sellers(object params) {
        object sellers = Env.Call("super", this, "_prepare_sellers", params);
        if (params != null && params.ContainsKey("order_id") && Env.GetField("purchase.order", "order_id", "requisition_id") != null) {
            return Env.Call("filtered", sellers, (object seller) => {
                return !(bool)Env.GetField(seller, "purchase_requisition_id") || (bool)Env.GetField(seller, "purchase_requisition_id") == (bool)Env.GetField(params["order_id"], "requisition_id");
            });
        } else {
            return sellers;
        }
    }
}
