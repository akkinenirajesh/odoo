C#
public partial class StockDeliveryCarrier {
    // all the model methods are written here.
    public List<Dictionary<string, object>> SendShipping(List<StockPicking> pickings) {
        if (this.DeliveryType != null && this.DeliveryType != "") {
            string methodName = $"{this.DeliveryType}SendShipping";
            if (Env.HasMethod(this, methodName)) {
                return (List<Dictionary<string, object>>)Env.Call(this, methodName, pickings);
            }
        }
        return new List<Dictionary<string, object>>();
    }

    public object GetReturnLabel(List<StockPicking> pickings, string trackingNumber = null, DateTime? originDate = null) {
        if (this.CanGenerateReturn && this.DeliveryType != null && this.DeliveryType != "") {
            string methodName = $"{this.DeliveryType}GetReturnLabel";
            if (Env.HasMethod(this, methodName)) {
                return Env.Call(this, methodName, pickings, trackingNumber, originDate);
            }
        }
        return null;
    }

    public string GetReturnLabelPrefix() {
        return $"LabelReturn-{this.DeliveryType}";
    }

    public string GetDeliveryLabelPrefix() {
        return $"LabelShipping-{this.DeliveryType}";
    }

    public string GetDeliveryDocPrefix() {
        return $"ShippingDoc-{this.DeliveryType}";
    }

    public string GetTrackingLink(StockPicking picking) {
        if (this.DeliveryType != null && this.DeliveryType != "") {
            string methodName = $"{this.DeliveryType}GetTrackingLink";
            if (Env.HasMethod(this, methodName)) {
                return (string)Env.Call(this, methodName, picking);
            }
        }
        return null;
    }

    public object CancelShipment(List<StockPicking> pickings) {
        if (this.DeliveryType != null && this.DeliveryType != "") {
            string methodName = $"{this.DeliveryType}CancelShipment";
            if (Env.HasMethod(this, methodName)) {
                return Env.Call(this, methodName, pickings);
            }
        }
        return null;
    }

    public string GetDefaultCustomPackageCode() {
        if (this.DeliveryType != null && this.DeliveryType != "") {
            string methodName = $"_${this.DeliveryType}GetDefaultCustomPackageCode";
            if (Env.HasMethod(this, methodName)) {
                return (string)Env.Call(this, methodName);
            }
        }
        return null;
    }

    public List<DeliveryPackage> GetPackagesFromOrder(SaleOrder order, PackageType defaultPackageType) {
        List<DeliveryPackage> packages = new List<DeliveryPackage>();
        decimal totalCost = 0;
        foreach (SaleOrderLine line in order.OrderLines.Where(l => !l.IsDelivery && !l.DisplayType)) {
            totalCost += Env.Call(this, "_ProductPriceToCompanyCurrency", line.ProductQty, line.ProductId, order.CompanyId);
        }

        decimal totalWeight = order.GetEstimatedWeight() + defaultPackageType.BaseWeight;
        if (totalWeight == 0.0) {
            throw new Exception("The package cannot be created because the total weight of the products in the picking is 0.0");
        }
        decimal maxWeight = defaultPackageType.MaxWeight == 0 ? totalWeight + 1 : defaultPackageType.MaxWeight;
        int totalFullPackages = (int)(totalWeight / maxWeight);
        decimal lastPackageWeight = totalWeight % maxWeight;

        List<decimal> packageWeights = Enumerable.Repeat(maxWeight, totalFullPackages).ToList();
        if (lastPackageWeight != 0) {
            packageWeights.Add(lastPackageWeight);
        }
        decimal partialCost = totalCost / packageWeights.Count;
        List<DeliveryCommodity> orderCommodities = GetCommoditiesFromOrder(order);

        foreach (DeliveryCommodity commodity in orderCommodities) {
            commodity.MonetaryValue /= packageWeights.Count;
            commodity.Qty = Math.Max(1, commodity.Qty / packageWeights.Count);
        }

        foreach (decimal weight in packageWeights) {
            packages.Add(new DeliveryPackage(
                orderCommodities,
                weight,
                defaultPackageType,
                totalCost: partialCost,
                currency: order.CompanyId.CurrencyId,
                order: order
            ));
        }
        return packages;
    }

    public List<DeliveryPackage> GetPackagesFromPicking(StockPicking picking, PackageType defaultPackageType) {
        List<DeliveryPackage> packages = new List<DeliveryPackage>();

        if (picking.IsReturnPicking) {
            List<DeliveryCommodity> commodities = GetCommoditiesFromStockMoveLines(picking.MoveLineIds);
            decimal weight = picking.GetEstimatedWeight() + defaultPackageType.BaseWeight;
            packages.Add(new DeliveryPackage(
                commodities,
                weight,
                defaultPackageType,
                currency: picking.CompanyId.CurrencyId,
                picking: picking
            ));
            return packages;
        }

        foreach (StockPackage package in picking.PackageIds) {
            List<StockMoveLine> moveLines = picking.MoveLineIds.Where(ml => ml.ResultPackageId == package).ToList();
            List<DeliveryCommodity> commodities = GetCommoditiesFromStockMoveLines(moveLines);
            decimal packageTotalCost = 0.0;
            foreach (StockQuant quant in package.QuantIds) {
                packageTotalCost += Env.Call(this, "_ProductPriceToCompanyCurrency", quant.Quantity, quant.ProductId, picking.CompanyId);
            }
            packages.Add(new DeliveryPackage(
                commodities,
                package.ShippingWeight != null ? package.ShippingWeight : package.Weight,
                package.PackageTypeId,
                name: package.Name,
                totalCost: packageTotalCost,
                currency: picking.CompanyId.CurrencyId,
                picking: picking
            ));
        }

        if (picking.WeightBulk != null) {
            List<DeliveryCommodity> commodities = GetCommoditiesFromStockMoveLines(picking.MoveLineIds);
            decimal packageTotalCost = 0.0;
            foreach (StockMoveLine moveLine in picking.MoveLineIds) {
                packageTotalCost += Env.Call(this, "_ProductPriceToCompanyCurrency", moveLine.Quantity, moveLine.ProductId, picking.CompanyId);
            }
            packages.Add(new DeliveryPackage(
                commodities,
                picking.WeightBulk,
                defaultPackageType,
                name: "Bulk Content",
                totalCost: packageTotalCost,
                currency: picking.CompanyId.CurrencyId,
                picking: picking
            ));
        } else if (packages.Count == 0) {
            throw new Exception("The package cannot be created because the total weight of the products in the picking is 0.0");
        }
        return packages;
    }

    public List<DeliveryCommodity> GetCommoditiesFromOrder(SaleOrder order) {
        List<DeliveryCommodity> commodities = new List<DeliveryCommodity>();

        foreach (SaleOrderLine line in order.OrderLines.Where(l => !l.IsDelivery && !l.DisplayType && l.ProductId.Type == "consu")) {
            decimal unitQuantity = line.ProductUom.ComputeQuantity(line.ProductUomQty, line.ProductId.UomId);
            decimal roundedQty = Math.Max(1, Math.Round(unitQuantity, 0));
            string countryOfOrigin = line.ProductId.CountryOfOrigin.Code != null ? line.ProductId.CountryOfOrigin.Code : order.WarehouseId.PartnerId.CountryId.Code;
            commodities.Add(new DeliveryCommodity(
                line.ProductId,
                amount: roundedQty,
                monetaryValue: line.PriceReduceTaxInc,
                countryOfOrigin: countryOfOrigin
            ));
        }

        return commodities;
    }

    public List<DeliveryCommodity> GetCommoditiesFromStockMoveLines(List<StockMoveLine> moveLines) {
        List<DeliveryCommodity> commodities = new List<DeliveryCommodity>();

        List<StockMoveLine> productLines = moveLines.Where(line => line.ProductId.Type == "consu").ToList();
        foreach (Product product in productLines.GroupBy(x => x.ProductId).Select(g => g.Key).ToList()) {
            decimal unitQuantity = productLines.Where(l => l.ProductId == product).Sum(l => l.ProductUomId.ComputeQuantity(l.Quantity, product.UomId));
            decimal roundedQty = Math.Max(1, Math.Round(unitQuantity, 0));
            string countryOfOrigin = product.CountryOfOrigin.Code != null ? product.CountryOfOrigin.Code : moveLines[0].PickingId.PickingTypeId.WarehouseId.PartnerId.CountryId.Code;
            decimal unitPrice = productLines.Where(l => l.ProductId == product).Sum(l => l.SalePrice) / roundedQty;
            commodities.Add(new DeliveryCommodity(product, amount: roundedQty, monetaryValue: unitPrice, countryOfOrigin: countryOfOrigin));
        }

        return commodities;
    }

    public decimal ProductPriceToCompanyCurrency(decimal quantity, Product product, Company company) {
        return company.CurrencyId.Convert(quantity * product.StandardPrice, product.CurrencyId, company, DateTime.Now);
    }

    public List<Dictionary<string, object>> FixedSendShipping(List<StockPicking> pickings) {
        List<Dictionary<string, object>> res = new List<Dictionary<string, object>>();
        foreach (StockPicking p in pickings) {
            res.Add(new Dictionary<string, object>() {
                { "exact_price", p.CarrierId.FixedPrice },
                { "tracking_number", null }
            });
        }
        return res;
    }

    public string FixedGetTrackingLink(StockPicking picking) {
        if (this.TrackingUrl != null && picking.CarrierTrackingRef != null) {
            return this.TrackingUrl.ToLower().Replace("<shipmenttrackingnumber>", picking.CarrierTrackingRef);
        }
        return null;
    }

    public object FixedCancelShipment(List<StockPicking> pickings) {
        throw new NotImplementedException();
    }

    public List<Dictionary<string, object>> BaseOnRuleSendShipping(List<StockPicking> pickings) {
        List<Dictionary<string, object>> res = new List<Dictionary<string, object>>();
        foreach (StockPicking p in pickings) {
            StockDeliveryCarrier carrier = _MatchAddress(p.PartnerId);
            if (carrier == null) {
                throw new Exception("There is no matching delivery rule.");
            }
            res.Add(new Dictionary<string, object>() {
                { "exact_price", p.SaleId != null ? p.CarrierId.GetPriceAvailable(p.SaleId) : 0.0 },
                { "tracking_number", null }
            });
        }
        return res;
    }

    public string BaseOnRuleGetTrackingLink(StockPicking picking) {
        if (this.TrackingUrl != null && picking.CarrierTrackingRef != null) {
            return this.TrackingUrl.ToLower().Replace("<shipmenttrackingnumber>", picking.CarrierTrackingRef);
        }
        return null;
    }

    public object BaseOnRuleCancelShipment(List<StockPicking> pickings) {
        throw new NotImplementedException();
    }

    private StockDeliveryCarrier _MatchAddress(Partner partner) {
        // Implement logic to match address with delivery rules
        return null;
    }
}
