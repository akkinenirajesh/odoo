csharp
public partial class AccountMoveLine
{
    public Dictionary<Product.Product, decimal> InvoicedQuantityPerProduct { get; set; }

    public Dictionary<Product.Product, decimal> GetInvoicedQuantityPerProduct()
    {
        Dictionary<Product.Product, decimal> qties = new Dictionary<Product.Product, decimal>();
        var res = base.GetInvoicedQuantityPerProduct();
        var invoicedProducts = Env.Ref<Product.Product>().Concat(*res.Keys);
        var bomKits = Env.Ref<Mrp.Bom>()._BomFind(invoicedProducts, company_id: this.Company.Id, bom_type: "phantom");
        foreach (var product in res.Keys)
        {
            var qty = res[product];
            var bomKit = bomKits[product];
            if (bomKit != null)
            {
                var invoicedQty = product.Uom._ComputeQuantity(qty, bomKit.ProductUom, round: false);
                var factor = invoicedQty / bomKit.ProductQty;
                var dummy, bomSubLines = bomKit.Explode(product, factor);
                foreach (var bomLine in bomSubLines)
                {
                    var bomLineData = bomSubLines[bomLine];
                    qties[bomLine.Product] += bomLine.ProductUom._ComputeQuantity(bomLineData["qty"], bomLine.Product.Uom);
                }
            }
            else
            {
                qties[product] += qty;
            }
        }
        return qties;
    }

}
