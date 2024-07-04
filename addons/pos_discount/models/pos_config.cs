csharp
public partial class PosConfig {
    public virtual void OpenUi() {
        if (!this.CurrentSessionId && this.ModulePosDiscount && !this.DiscountProductId) {
            throw new UserError("A discount product is needed to use the Global Discount feature. Go to Point of Sale > Configuration > Settings to set it.");
        }
        // Call super method using reflection
        var baseMethod = this.GetType().BaseType.GetMethod("OpenUi", BindingFlags.Instance | BindingFlags.NonPublic);
        baseMethod.Invoke(this, new object[] { });
    }

    public virtual IEnumerable<Product.Product> GetSpecialProducts() {
        // Call super method using reflection
        var baseMethod = this.GetType().BaseType.GetMethod("GetSpecialProducts", BindingFlags.Instance | BindingFlags.NonPublic);
        var superResult = (IEnumerable<Product.Product>)baseMethod.Invoke(this, new object[] { });
        var defaultDiscountProduct = Env.Ref("pos_discount.product_product_consumable") ?? Env.Model<Product.Product>().Search([]).First();
        return superResult.Union(Env.Model<PosConfig>().Search([]).Select(x => x.DiscountProductId)).Union(new List<Product.Product> { defaultDiscountProduct });
    }

    public virtual Domain GetAvailableProductDomain() {
        var baseMethod = this.GetType().BaseType.GetMethod("GetAvailableProductDomain", BindingFlags.Instance | BindingFlags.NonPublic);
        var superResult = (Domain)baseMethod.Invoke(this, new object[] { });
        return superResult | new Domain("[('Id', '=', DiscountProductId.Id)]");
    }

    public virtual void DefaultDiscountValueOnModuleInstall() {
        var configs = Env.Model<PosConfig>().Search([]);
        var openConfigs = Env.Model<PosSession>().Search([new Or(new Domain("State", "!=", "closed"), new Domain("Rescue", "=", true))]).Select(x => x.ConfigId);
        var product = Env.Ref("pos_discount.product_product_consumable");
        foreach (var conf in configs.Except(openConfigs)) {
            conf.DiscountProductId = product ? (product.CompanyId == conf.CompanyId || product.CompanyId == null ? product : null) : null;
        }
    }
}
