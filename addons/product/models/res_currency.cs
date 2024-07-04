csharp
public partial class ResCurrency {

    public void ActivateGroupMultiCurrency()
    {
        // for Sale/ POS - Multi currency flows require pricelists
        base.ActivateGroupMultiCurrency();
        if (!Env.User.HasGroup("product.group_product_pricelist"))
        {
            var groupUser = Env.Ref("base.group_user").sudo();
            groupUser.ApplyGroup(Env.Ref("product.group_product_pricelist"));
            Env["res.company"]._activate_or_create_pricelists();
        }
    }

    public void Write(Dictionary<string, object> vals)
    {
        // Archive pricelist when the linked currency is archived. 
        base.Write(vals);

        if (this != null && vals.ContainsKey("Active") && !(bool)vals["Active"])
        {
            Env["product.pricelist"].Search(new Dictionary<string, object> { { "currency_id", this.Id } }).ActionArchive();
        }
    }
}
