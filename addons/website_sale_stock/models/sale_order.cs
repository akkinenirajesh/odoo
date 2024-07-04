csharp
public partial class WebsiteSaleStock.SaleOrder
{
    public Stock.Warehouse GetWarehouseAvailable()
    {
        var warehouse = Env.Get<Website.Website>().Browse(this.WebsiteId)._GetWarehouseAvailable();
        if (warehouse == null && this.UserId != null && this.CompanyId != null)
        {
            warehouse = Env.Get<Res.Users>().Browse(this.UserId).WithCompany(this.CompanyId.Id)._GetDefaultWarehouseId();
        }
        if (warehouse == null)
        {
            warehouse = Env.Get<Res.Users>().Browse(Env.User.Id)._GetDefaultWarehouseId();
        }
        return warehouse;
    }

    public void ComputeWarehouseId()
    {
        if (this.WebsiteId != null)
        {
            this.WarehouseId = GetWarehouseAvailable();
        }
    }

    public (decimal AllowedLineQty, string ReturnedWarning) VerifyUpdatedQuantity(SaleOrderLine orderLine, Product.Product product, decimal newQty)
    {
        if (product.IsStorable && !product.AllowOutOfStockOrder)
        {
            var (productQtyInCart, availableQty) = GetCartAndFreeQty(product, orderLine);

            decimal oldQty = orderLine != null ? orderLine.ProductUomQty : 0;
            decimal addedQty = newQty - oldQty;
            decimal totalCartQty = productQtyInCart + addedQty;
            if (availableQty < totalCartQty)
            {
                decimal allowedLineQty = availableQty - (productQtyInCart - oldQty);
                if (allowedLineQty > 0)
                {
                    if (orderLine != null)
                    {
                        orderLine.SetShopWarningStock(totalCartQty, availableQty);
                    }
                    else
                    {
                        this.SetShopWarningStock(totalCartQty, availableQty);
                    }
                    string returnedWarning = orderLine != null ? orderLine.ShopWarning : this.ShopWarning;
                    return (allowedLineQty, returnedWarning);
                }
                else
                {
                    if (orderLine != null)
                    {
                        this.ShopWarning = "Some products became unavailable and your cart has been updated. We're sorry for the inconvenience.";
                        returnedWarning = this.ShopWarning;
                    }
                    else
                    {
                        returnedWarning = "The item has not been added to your cart since it is not available.";
                    }
                    return (0, returnedWarning);
                }
            }
        }
        return Env.Get<SaleOrder>().Browse(this.Id).VerifyUpdatedQuantity(orderLine, product, newQty);
    }

    public (decimal CartQty, decimal FreeQty) GetCartAndFreeQty(Product.Product product, SaleOrderLine line)
    {
        if (line == null && product == null)
        {
            return (0, 0);
        }
        decimal cartQty = this.OrderLine.Where(l => l.ProductId == (product != null ? product.Id : line.ProductId)).Sum(l => l.ProductUomQty);
        decimal freeQty = (product ?? line.ProductId).WithContext(WarehouseId: this.WarehouseId.Id).FreeQty;
        return (cartQty, freeQty);
    }

    public IEnumerable<SaleOrderLine> GetCommonProductLines(SaleOrderLine line, Product.Product product)
    {
        if (line == null && product == null)
        {
            return Enumerable.Empty<SaleOrderLine>();
        }
        product = product ?? line.ProductId;
        return this.OrderLine.Where(l => l.ProductId == product.Id);
    }

    public void CheckCartIsReadyToBePaid()
    {
        List<string> values = new List<string>();
        foreach (var line in this.OrderLine)
        {
            if (line.ProductId.IsStorable && !line.ProductId.AllowOutOfStockOrder)
            {
                var (cartQty, avlQty) = GetCartAndFreeQty(line.ProductId, line);
                if (cartQty > avlQty)
                {
                    line.SetShopWarningStock(cartQty, Math.Max(avlQty, 0));
                    values.Add(line.ShopWarning);
                }
            }
        }
        if (values.Count > 0)
        {
            throw new Exception(" " + string.Join(" ", values));
        }
        Env.Get<SaleOrder>().Browse(this.Id).CheckCartIsReadyToBePaid();
    }

    public string SetShopWarningStock(decimal desiredQty, decimal newQty)
    {
        this.ShopWarning = $"You ask for {desiredQty} products but only {newQty} is available";
        return this.ShopWarning;
    }

    public IEnumerable<SaleOrder> FilterCanSendAbandonedCartMail()
    {
        return Env.Get<SaleOrder>().Browse(this.Id).FilterCanSendAbandonedCartMail().Where(so => so.AllProductAvailable());
    }

    public bool AllProductAvailable()
    {
        foreach (var line in this.WithContext(WebsiteSaleStockGetQuantity: true).OrderLine)
        {
            var product = line.ProductId;
            if (!product.IsStorable || product.AllowOutOfStockOrder)
            {
                continue;
            }
            decimal freeQty = this.WebsiteId._GetProductAvailableQty(product);
            if (freeQty == 0)
            {
                return false;
            }
        }
        return true;
    }
}
