C#
public partial class SaleMrpBom 
{
    public virtual void ToggleActive()
    {
        if (this.Active)
        {
            EnsureBomIsFree();
            this.Active = false;
        }
        else
        {
            this.Active = true;
        }
    }

    public virtual void Unlink()
    {
        EnsureBomIsFree();
        Env.Delete(this);
    }

    public virtual void EnsureBomIsFree()
    {
        if (this.Type != "phantom")
        {
            return;
        }

        var productIds = new List<int>() { this.ProductVariantId.Id };
        productIds.AddRange(this.ProductTmplId.ProductVariantIds.Select(p => p.Id).ToList());
        
        if (productIds.Count == 0)
        {
            return;
        }

        var saleOrderLines = Env.Search<Sale.SaleOrderLine>(new List<Sale.SaleOrderLineField>() 
        {
            new Sale.SaleOrderLineField() { Field = Sale.SaleOrderLineField.State, Operator = "=", Value = "sale" },
            new Sale.SaleOrderLineField() { Field = Sale.SaleOrderLineField.InvoiceStatus, Operator = "in", Value = new List<string>() {"no", "to invoice"} },
            new Sale.SaleOrderLineField() { Field = Sale.SaleOrderLineField.ProductId, Operator = "in", Value = productIds },
            new Sale.SaleOrderLineField() { Field = Sale.SaleOrderLineField.MoveIds, Operator = "!=", Value = "cancel" }
        });

        if (saleOrderLines.Count > 0)
        {
            var productNames = string.Join(", ", saleOrderLines.Select(l => l.ProductName).ToList());
            throw new Exception($"As long as there are some sale order lines that must be delivered/invoiced and are related to these bills of materials, you can not remove them.\nThe error concerns these products: {productNames}");
        }
    }
}
