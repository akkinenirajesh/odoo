csharp
public partial class AccountMove
{
    public virtual List<PointOfSale.PosOrder> PosOrderIds { get; set; }
    public virtual List<PointOfSale.PosPayment> PosPaymentIds { get; set; }
    public virtual List<AccountMove> PosRefundedInvoiceIds { get; set; }
    public virtual PointOfSale.PosOrder ReversedPosOrderId { get; set; }

    public virtual List<Stock.StockMove> _StockAccountGetLastStepStockMoves()
    {
        var stockMoves = Env.CallMethod("Account.AccountMove", "_StockAccountGetLastStepStockMoves", this);
        if (this.MoveType == "out_invoice")
        {
            stockMoves.AddRange(this.Sudo().PosOrderIds.SelectMany(o => o.PickingIds.SelectMany(p => p.MoveIds)).Where(m => m.State == "done" && m.LocationDestId.Usage == "customer"));
        }
        if (this.MoveType == "out_refund")
        {
            stockMoves.AddRange(this.Sudo().PosOrderIds.SelectMany(o => o.PickingIds.SelectMany(p => p.MoveIds)).Where(m => m.State == "done" && m.LocationId.Usage == "customer"));
        }
        return stockMoves;
    }

    public virtual List<Dictionary<string, object>> _GetInvoicedLotValues()
    {
        if (this.State == "draft")
        {
            return Env.CallMethod("Account.AccountMove", "_GetInvoicedLotValues", this);
        }
        var lotValues = Env.CallMethod("Account.AccountMove", "_GetInvoicedLotValues", this);
        foreach (var order in this.Sudo().PosOrderIds)
        {
            foreach (var line in order.Lines)
            {
                if (line.PackLotIds.Any())
                {
                    foreach (var lot in line.PackLotIds)
                    {
                        lotValues.Add(new Dictionary<string, object>
                        {
                            { "ProductName", lot.ProductId.Name },
                            { "Quantity", lot.ProductId.Tracking == "lot" ? line.Qty : 1.0 },
                            { "UomName", line.ProductUomId.Name },
                            { "LotName", lot.LotName },
                            { "PosLotId", lot.Id }
                        });
                    }
                }
            }
        }
        return lotValues;
    }

    public virtual void _ComputePaymentsWidgetReconciledInfo()
    {
        Env.CallMethod("Account.AccountMove", "_ComputePaymentsWidgetReconciledInfo", this);
        if (this.InvoicePaymentsWidget != null && this.State == "posted" && this.IsInvoice(true))
        {
            var reconciledPartials = this._GetAllReconciledInvoicePartials();
            for (int i = 0; i < reconciledPartials.Count; i++)
            {
                var reconciledPartial = reconciledPartials[i];
                var counterpartLine = reconciledPartial["aml"] as AccountMoveLine;
                var posPayment = counterpartLine.MoveId.Sudo().PosPaymentIds;
                this.InvoicePaymentsWidget["content"][i].Add("pos_payment_name", posPayment.PaymentMethodId.Name);
            }
        }
    }

    public virtual void _ComputeAmount()
    {
        Env.CallMethod("Account.AccountMove", "_ComputeAmount", this);
        if (this.MoveType == "entry" && this.ReversedPosOrderId != null)
        {
            this.AmountTotalSigned *= -1;
        }
    }

    public virtual AccountMove Sudo()
    {
        return Env.CallMethod("Account.AccountMove", "Sudo", this);
    }

    public virtual bool IsInvoice(bool includeReceipts)
    {
        return Env.CallMethod("Account.AccountMove", "IsInvoice", this, includeReceipts);
    }

    public virtual List<Dictionary<string, object>> _GetAllReconciledInvoicePartials()
    {
        return Env.CallMethod("Account.AccountMove", "_GetAllReconciledInvoicePartials", this);
    }
}

public partial class AccountMoveLine
{
    public virtual decimal _StockAccountGetAngloSaxonPriceUnit()
    {
        if (this.ProductId == null)
        {
            return this.PriceUnit;
        }
        var priceUnit = Env.CallMethod("Account.AccountMoveLine", "_StockAccountGetAngloSaxonPriceUnit", this);
        var sudoOrder = this.MoveId.Sudo().PosOrderIds;
        if (sudoOrder != null)
        {
            priceUnit = sudoOrder._GetPosAngloSaxonPriceUnit(this.ProductId, this.MoveId.PartnerId.Id, this.Quantity);
        }
        return priceUnit;
    }
}
