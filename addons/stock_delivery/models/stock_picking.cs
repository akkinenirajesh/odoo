csharp
public partial class StockPicking
{
    public virtual void ComputeCarrierTrackingUrl()
    {
        if (this.CarrierId != null && !string.IsNullOrEmpty(this.CarrierTrackingRef))
        {
            this.CarrierTrackingUrl = this.CarrierId.GetTrackingLink(this);
        }
    }

    public virtual void ComputeReturnPicking()
    {
        if (this.CarrierId != null && this.CarrierId.CanGenerateReturn)
        {
            this.IsReturnPicking = this.MoveIdsWithoutPackage.Any(m => m.OriginReturnedMoveId != null);
        }
        else
        {
            this.IsReturnPicking = false;
        }
    }

    public virtual void ComputeReturnLabel()
    {
        if (this.CarrierId != null)
        {
            this.ReturnLabelIds = Env.GetModel<Ir.Attachment>().Search(new[] { ("res_model", "=", "stock.picking"), ("res_id", "=", this.Id), ("name", "like", "%" + this.CarrierId.GetReturnLabelPrefix() + "%") });
        }
        else
        {
            this.ReturnLabelIds = null;
        }
    }

    public virtual string GetMultipleCarrierTracking()
    {
        try
        {
            return Json.Deserialize<string[]>(this.CarrierTrackingUrl);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public virtual void CalWeight()
    {
        this.Weight = this.MoveIds.Where(move => move.State != "cancel").Sum(move => move.Weight);
    }

    public virtual void SendConfirmationEmail()
    {
        if (this.CarrierId != null && this.CarrierId.IntegrationLevel == "rate_and_ship" && this.PickingTypeCode != "incoming" && string.IsNullOrEmpty(this.CarrierTrackingRef) && this.PickingTypeId.PrintLabel)
        {
            this.SendToShipper();
        }
        this.CheckCarrierDetailsCompliance();
    }

    public virtual bool PrePutInPackHook(List<Stock.MoveLine> moveLineIds)
    {
        if (!base.PrePutInPackHook(moveLineIds))
        {
            if (moveLineIds.CarrierId != null)
            {
                if (moveLineIds.CarrierId.Count > 1 || moveLineIds.Any(ml => ml.CarrierId == null))
                {
                    throw new UserError("You cannot pack products into the same package when they have different carriers (i.e. check that all of their transfers have a carrier assigned and are using the same carrier).");
                }
                return this.SetDeliveryPackageType(moveLineIds.PickingId.Count > 1);
            }
        }
        return true;
    }

    public virtual Dictionary<string, object> SetDeliveryPackageType(bool batchPack)
    {
        var viewId = Env.GetModel<Ir.Actions.ActWindow>().GetId("stock_delivery.choose_delivery_package_view_form");
        var context = new Dictionary<string, object>()
        {
            { "current_package_carrier_type", this.CarrierId.DeliveryType },
            { "default_picking_id", this.Id },
            { "batch_pack", batchPack }
        };
        if (context["current_package_carrier_type"] == "fixed" || context["current_package_carrier_type"] == "base_on_rule")
        {
            context["current_package_carrier_type"] = "none";
        }
        return new Dictionary<string, object>()
        {
            { "name", "Package Details" },
            { "type", "ir.actions.act_window" },
            { "view_mode", "form" },
            { "res_model", "choose.delivery.package" },
            { "view_id", viewId },
            { "views", new[] { (viewId, "form") } },
            { "target", "new" },
            { "context", context }
        };
    }

    public virtual void SendToShipper()
    {
        var res = this.CarrierId.SendShipping(this)[0];
        if (this.CarrierId.FreeOver && this.SaleId != null)
        {
            var amountWithoutDelivery = this.SaleId.ComputeAmountTotalWithoutDelivery();
            if (this.CarrierId.ComputeCurrency(this.SaleId, amountWithoutDelivery, "pricelist_to_company") >= this.CarrierId.Amount)
            {
                res["exact_price"] = 0.0;
            }
        }
        this.CarrierPrice = this.CarrierId.ApplyMargins(res["exact_price"]);
        if (!string.IsNullOrEmpty(res["tracking_number"]))
        {
            var relatedPickings = Env.GetModel<Stock.Picking>();
            if (!string.IsNullOrEmpty(this.CarrierTrackingRef) && res["tracking_number"] == this.CarrierTrackingRef)
            {
                relatedPickings = this;
            }
            var accessedMoves = this.MoveIds.MoveOrigIds;
            var previousMoves = this.MoveIds.MoveOrigIds;
            while (previousMoves.Count > 0)
            {
                relatedPickings |= previousMoves.PickingId;
                previousMoves = previousMoves.MoveOrigIds - accessedMoves;
                accessedMoves |= previousMoves;
            }
            accessedMoves = this.MoveIds.MoveDestIds;
            var nextMoves = this.MoveIds.MoveDestIds;
            while (nextMoves.Count > 0)
            {
                relatedPickings |= nextMoves.PickingId;
                nextMoves = nextMoves.MoveDestIds - accessedMoves;
                accessedMoves |= nextMoves;
            }
            var withoutTracking = relatedPickings.Where(p => string.IsNullOrEmpty(p.CarrierTrackingRef));
            withoutTracking.CarrierTrackingRef = res["tracking_number"];
            foreach (var p in relatedPickings - withoutTracking)
            {
                p.CarrierTrackingRef += "," + res["tracking_number"];
            }
        }
        var orderCurrency = this.SaleId.CurrencyId ?? this.CompanyId.CurrencyId;
        var msg = "Shipment sent to carrier " + this.CarrierId.Name + " for shipping with tracking number " + this.CarrierTrackingRef + "\nCost: " + this.CarrierPrice.ToString("0.00") + " " + orderCurrency.Name;
        this.MessagePost(msg);
        this.AddDeliveryCostToSO();
    }

    public virtual void CheckCarrierDetailsCompliance()
    {
        // Hook to check if a delivery is compliant in regard of the carrier.
    }

    public virtual void PrintReturnLabel()
    {
        this.CarrierId.GetReturnLabel(this);
    }

    public virtual List<Sale.OrderLine> GetMatchingDeliveryLines()
    {
        return this.SaleId.OrderLine.Where(
            l => l.IsDelivery && l.CurrencyId.IsZero(l.PriceUnit) && l.ProductId == this.CarrierId.ProductId
        ).ToList();
    }

    public virtual Dictionary<string, object> PrepareSaleDeliveryLineVals()
    {
        return new Dictionary<string, object>()
        {
            { "price_unit", this.CarrierPrice },
            { "name", this.CarrierId.WithContext(new Dictionary<string, object>() { { "lang", this.PartnerId.Lang } }).Name }
        };
    }

    public virtual void AddDeliveryCostToSO()
    {
        if (this.SaleId != null && this.CarrierId.InvoicePolicy == "real" && this.CarrierPrice > 0)
        {
            var deliveryLines = this.GetMatchingDeliveryLines();
            if (deliveryLines.Count == 0)
            {
                deliveryLines = this.SaleId.CreateDeliveryLine(this.CarrierId, this.CarrierPrice);
            }
            deliveryLines[0].Write(this.PrepareSaleDeliveryLineVals());
        }
    }

    public virtual Dictionary<string, object> OpenWebsiteUrl()
    {
        if (string.IsNullOrEmpty(this.CarrierTrackingUrl))
        {
            throw new UserError("Your delivery method has no redirect on courier provider's website to track this order.");
        }

        List<string[]> carrierTrackers = new List<string[]>();
        try
        {
            carrierTrackers = Json.Deserialize<List<string[]>>(this.CarrierTrackingUrl);
        }
        catch (Exception)
        {
            carrierTrackers = new List<string[]> { new string[] { this.CarrierTrackingUrl } };
        }
        if (carrierTrackers.Count > 1)
        {
            var msg = "Tracking links for shipment:\n";
            foreach (var tracker in carrierTrackers)
            {
                msg += "<a href=\"" + tracker[1] + "\">" + tracker[0] + "</a>\n";
            }
            this.MessagePost(msg);
            return Env.GetModel<Ir.Actions.Actions>().GetActionForXmlId("stock_delivery.act_delivery_trackers_url");
        }

        return new Dictionary<string, object>()
        {
            { "type", "ir.actions.act_url" },
            { "name", "Shipment Tracking Page" },
            { "target", "new" },
            { "url", this.CarrierTrackingUrl }
        };
    }

    public virtual void CancelShipment()
    {
        this.CarrierId.CancelShipment(this);
        var msg = "Shipment " + this.CarrierTrackingRef + " cancelled";
        this.MessagePost(msg);
        this.CarrierTrackingRef = null;
    }

    public virtual double GetEstimatedWeight()
    {
        double weight = 0.0;
        foreach (var move in this.MoveIds)
        {
            weight += move.ProductQty * move.ProductId.Weight;
        }
        return weight;
    }

    public virtual bool ShouldGenerateCommercialInvoice()
    {
        return this.PickingTypeId.WarehouseId.PartnerId.CountryId != this.PartnerId.CountryId;
    }
}
