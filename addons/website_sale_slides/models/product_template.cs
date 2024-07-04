C#
public partial class ProductTemplate {

    public string _prepare_service_tracking_tooltip() {
        if (this.ServiceTracking == "course") {
            return Env.Translate("Grant access to the eLearning course linked to this product.");
        }
        return Env.Call("Product.ProductTemplate", "_prepare_service_tracking_tooltip");
    }

    public object _get_product_types_allow_zero_price() {
        return Env.Call("Product.ProductTemplate", "_get_product_types_allow_zero_price");
    }
}
