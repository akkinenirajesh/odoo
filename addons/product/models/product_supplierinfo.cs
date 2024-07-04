csharp
public partial class Product.SupplierInfo
{
    public virtual decimal PriceDiscounted { get; set; }

    public void ComputePriceDiscounted()
    {
        PriceDiscounted = Price * (1 - Discount / 100);
    }

    public virtual decimal DefaultProductId
    {
        get
        {
            var product_id = Env.Get("default_product_id");
            if (product_id == null)
            {
                var model = Env.Context.Get("model");
                var activeId = Env.Context.Get("active_id");
                if (model == "Product.Product" && activeId != null)
                {
                    product_id = Env.Get<Product.Product>(activeId).Exists();
                }
            }
            return product_id;
        }
    }

    public void OnChangeProductTmplId()
    {
        if (ProductId != null && !ProductTmplId.ProductVariantIds.Contains(ProductId))
        {
            ProductId = null;
        }
    }

    public virtual List<ImportTemplate> GetImportTemplates()
    {
        return new List<ImportTemplate>
        {
            new ImportTemplate
            {
                Label = "Import Template for Vendor Pricelists",
                Template = "/product/static/xls/product_supplierinfo.xls"
            }
        };
    }

    public void SanitizeVals(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("ProductId") && !vals.ContainsKey("ProductTmplId"))
        {
            var product = Env.Get<Product.Product>(vals["ProductId"]);
            vals["ProductTmplId"] = product.ProductTmplId.Id;
        }
    }

    public virtual void Create(Dictionary<string, object> vals)
    {
        SanitizeVals(vals);
    }

    public virtual void Write(Dictionary<string, object> vals)
    {
        SanitizeVals(vals);
    }
}
