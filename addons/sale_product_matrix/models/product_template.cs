csharp
public partial class Sale_ProductTemplate {

    public Sale_ProductTemplate(Env env)
    {
        Env = env;
    }

    public Env Env { get; }

    public Sale.ProductAddMode ProductAddMode { get; set; }

    public bool HasConfigurableAttributes { get; set; }

    public Sale.ProductTemplate GetSingleProductVariant()
    {
        var res = Env.Call("super", "GetSingleProductVariant");
        if (HasConfigurableAttributes)
        {
            res.Set("mode", ProductAddMode);
        }
        else
        {
            res.Set("mode", Sale.ProductAddMode.Configurator);
        }
        return res;
    }
}
