csharp
public partial class PosSale.ProductProduct
{
    public List<PosSale.ProductProduct> OptionalProductIds { get; set; }

    public virtual List<PosSale.ProductProduct> _load_pos_data_fields(Pos.PosConfig configId)
    {
        var paramsList = Env.Call<List<PosSale.ProductProduct>>("PosSale.ProductProduct", "_load_pos_data_fields", configId);
        paramsList.Add(this);
        paramsList.Add(OptionalProductIds);
        paramsList.Add(Type);

        return paramsList;
    }

    public virtual Dictionary<string, object> GetProductInfoPos(decimal price, decimal quantity, Pos.PosConfig posConfigId)
    {
        var res = Env.Call<Dictionary<string, object>>("PosSale.ProductProduct", "GetProductInfoPos", price, quantity, posConfigId);

        res["OptionalProducts"] = OptionalProductIds.Where(product => product.AvailableInPos && product.SaleOk).Select(product => new
        {
            Name = product.Name,
            Price = product.ListPrice
        }).ToList();

        return res;
    }

    public virtual bool HasOptionalProductInPos()
    {
        return OptionalProductIds.Any(product => product.AvailableInPos && product.SaleOk);
    }
}
