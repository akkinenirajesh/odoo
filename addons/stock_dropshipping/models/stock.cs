C#
public partial class StockRule {
    public virtual object GetProcurementsToMergeGroupBy(object procurement)
    {
        // Implementation for _get_procurements_to_merge_groupby
        object saleLineId = procurement.GetValue("SaleLineId");
        object result = Env.Call("stock.stock.rule", "_get_procurements_to_merge_groupby", procurement);
        return new object[] { saleLineId, result };
    }
}

public partial class ProcurementGroup {
    public virtual object GetRuleDomain(object location, object values)
    {
        // Implementation for _get_rule_domain
        object domain = Env.Call("stock.procurement.group", "_get_rule_domain", location, values);
        if (values.ContainsKey("SaleLineId") && values.ContainsKey("CompanyId"))
        {
            domain = Env.Call("expression", "AND", new object[] { domain, new object[] { new object[] { "CompanyId", "=", values["CompanyId"].GetValue("Id") } } });
        }
        return domain;
    }
}

public partial class StockPicking {
    public virtual void ComputeIsDropship()
    {
        // Implementation for _compute_is_dropship
        IsDropship = LocationDestId.GetValue("Usage") == "customer" && LocationId.GetValue("Usage") == "supplier";
    }

    public virtual object IsToExternalLocation()
    {
        // Implementation for _is_to_external_location
        object result = Env.Call("stock.stock.picking", "_is_to_external_location", this);
        return result || IsDropship;
    }
}

public partial class StockPickingType {
    public virtual void ComputeDefaultLocationSrcId()
    {
        // Implementation for _compute_default_location_src_id
        if (Code == "Dropship")
        {
            DefaultLocationSrcId = Env.Ref("stock.stock_location_suppliers");
        }
        else
        {
            Env.Call("stock.stock.picking.type", "_compute_default_location_src_id", this);
        }
    }

    public virtual void ComputeDefaultLocationDestId()
    {
        // Implementation for _compute_default_location_dest_id
        if (Code == "Dropship")
        {
            DefaultLocationDestId = Env.Ref("stock.stock_location_customers");
        }
        else
        {
            Env.Call("stock.stock.picking.type", "_compute_default_location_dest_id", this);
        }
    }

    public virtual void ComputeWarehouseId()
    {
        // Implementation for _compute_warehouse_id
        Env.Call("stock.stock.picking.type", "_compute_warehouse_id", this);
        if (DefaultLocationSrcId.GetValue("Usage") == "supplier" && DefaultLocationDestId.GetValue("Usage") == "customer")
        {
            WarehouseId = null;
        }
    }

    public virtual void ComputeShowPickingType()
    {
        // Implementation for _compute_show_picking_type
        Env.Call("stock.stock.picking.type", "_compute_show_picking_type", this);
        if (Code == "Dropship")
        {
            ShowPickingType = true;
        }
    }
}

public partial class StockLot {
    public virtual void ComputeLastDeliveryPartnerId()
    {
        // Implementation for _compute_last_delivery_partner_id
        Env.Call("stock.stock.lot", "_compute_last_delivery_partner_id", this);
        if (DeliveryCount > 0)
        {
            var lastDelivery = DeliveryIds.Max(x => x.GetValue("DateDone"));
            if (lastDelivery.GetValue("IsDropship"))
            {
                LastDeliveryPartnerId = lastDelivery.GetValue("SaleId").GetValue("PartnerId");
            }
        }
    }

    public virtual object GetOutgoingDomain()
    {
        // Implementation for _get_outgoing_domain
        object res = Env.Call("stock.stock.lot", "_get_outgoing_domain", this);
        return Env.Call("expression", "OR", new object[] { res, new object[] {
            new object[] { "LocationDestId.Usage", "=", "customer" },
            new object[] { "LocationId.Usage", "=", "supplier" },
        } });
    }
}
