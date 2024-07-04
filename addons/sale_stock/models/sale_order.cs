csharp
public partial class SaleOrder 
{
    public virtual void ComputeEffectiveDate()
    {
        var pickings = Env.Get<Stock.Picking>().Search(p => p.State == "Done" && p.LocationDestId.Usage == "Customer" && p.SaleId == this.Id);
        var datesList = pickings.Select(p => p.DateDone).Where(d => d != null).ToList();
        this.EffectiveDate = datesList.Any() ? datesList.Min() : null;
    }

    public virtual void ComputeDeliveryStatus()
    {
        var pickings = Env.Get<Stock.Picking>().Search(p => p.SaleId == this.Id);
        if (!pickings.Any() || pickings.All(p => p.State == "Cancel"))
        {
            this.DeliveryStatus = null;
        }
        else if (pickings.All(p => p.State == "Done" || p.State == "Cancel"))
        {
            this.DeliveryStatus = "Full";
        }
        else if (pickings.Any(p => p.State == "Done"))
        {
            this.DeliveryStatus = "Partial";
        }
        else
        {
            this.DeliveryStatus = "Pending";
        }
    }

    public virtual void ComputeExpectedDate()
    {
        // Call the parent class method
        base.ComputeExpectedDate();

        // Apply custom logic based on PickingPolicy
        if (this.PickingPolicy == "Direct")
        {
            // Call the base method's logic
            base.ComputeExpectedDate();
        }
        else
        {
            // Custom logic for PickingPolicy "One"
            var expectedDates = Env.Get<Sale.SaleOrderLine>().Search(l => l.OrderId == this.Id).Select(l => l.ExpectedDate).ToList();
            this.ExpectedDate = expectedDates.Max();
        }
    }

    public virtual void Write(Dictionary<string, object> values)
    {
        if (values.ContainsKey("OrderLine") && this.State == "Sale")
        {
            var preOrderLineQty = Env.Get<Sale.SaleOrderLine>().Search(l => l.OrderId == this.Id && !l.IsExpense).ToDictionary(l => l, l => l.ProductUomQty);

            // Apply your logic for order_line updates
        }

        if (values.ContainsKey("PartnerShippingId"))
        {
            var newPartner = Env.Get<Res.Partner>().Browse(values["PartnerShippingId"]);
            var pickings = Env.Get<Stock.Picking>().Search(p => p.SaleId == this.Id && p.State != "Done" && p.State != "Cancel");
            // Apply your logic for partner_shipping_id updates
        }

        if (values.ContainsKey("CommitmentDate"))
        {
            var deadlineDateTime = values["CommitmentDate"] as DateTime?;
            var orderLines = Env.Get<Sale.SaleOrderLine>().Search(l => l.OrderId == this.Id);
            // Apply your logic for commitment_date updates
        }

        // Call the base Write method
        base.Write(values);

        if (values.ContainsKey("OrderLine") && this.State == "Sale")
        {
            var rounding = Env.Get("decimal.precision").PrecisionGet("Product Unit of Measure");
            var toLog = new Dictionary<Sale.SaleOrderLine, Tuple<decimal, decimal>>();
            // Apply your logic for order_line updates and logging
        }
    }

    public virtual void ComputeJsonPopover()
    {
        var lateStockPicking = Env.Get<Stock.Picking>().Search(p => p.DelayAlertDate != null && p.SaleId == this.Id);
        // Apply your logic for computing JsonPopover
    }

    public virtual void ActionConfirm()
    {
        // Call the base method
        base.ActionConfirm();

        // Apply your logic for ActionConfirm
    }

    public virtual void ComputePickingIds()
    {
        this.DeliveryCount = Env.Get<Stock.Picking>().Search(p => p.SaleId == this.Id).Count();
    }

    public virtual void ComputeWarehouseId()
    {
        var defaultWarehouseId = Env.Get("ir.default").WithCompany(this.CompanyId).GetModelDefaults("sale.order")["warehouse_id"] as int?;
        // Apply your logic for computing WarehouseId
    }

    public virtual void OnChangePartnerShippingId()
    {
        var pickings = Env.Get<Stock.Picking>().Search(p => p.State != "Done" && p.State != "Cancel" && p.PartnerId != this.PartnerShippingId);
        // Apply your logic for OnChangePartnerShippingId
    }

    public virtual void ActionViewDelivery()
    {
        // Apply your logic for ActionViewDelivery
    }

    public virtual void ActionCancel()
    {
        // Apply your logic for ActionCancel
    }

    public virtual Dictionary<string, object> PrepareInvoice()
    {
        var invoiceVals = base.PrepareInvoice();
        // Apply your logic for preparing invoice
        return invoiceVals;
    }

    public virtual void LogDecreaseOrderedQuantity(Dictionary<Tuple<object, object>, Dictionary<string, object>> documents, bool cancel = false)
    {
        // Apply your logic for logging decrease in ordered quantity
    }

    public virtual void _ComputeWarehouseId()
    {
        // This is a private method that's not accessible outside of the class
    }
}
