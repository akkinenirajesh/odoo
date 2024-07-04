csharp
public partial class PosOrder 
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }
    public virtual string LastOrderPreparationChange { get; set; }
    public virtual DateTime DateOrder { get; set; }
    public virtual ResUsers UserId { get; set; }
    public virtual double AmountTax { get; set; }
    public virtual double AmountTotal { get; set; }
    public virtual double AmountPaid { get; set; }
    public virtual double AmountReturn { get; set; }
    public virtual double Margin { get; set; }
    public virtual double MarginPercent { get; set; }
    public virtual bool IsTotalCostComputed { get; set; }
    public virtual ICollection<PosOrderLine> Lines { get; set; }
    public virtual ResCompany CompanyId { get; set; }
    public virtual string CountryCode { get; set; }
    public virtual ProductPricelist PricelistId { get; set; }
    public virtual ResPartner PartnerId { get; set; }
    public virtual int SequenceNumber { get; set; }
    public virtual PosSession SessionId { get; set; }
    public virtual PosConfig ConfigId { get; set; }
    public virtual ResCurrency CurrencyId { get; set; }
    public virtual double CurrencyRate { get; set; }
    public virtual string State { get; set; }
    public virtual AccountMove AccountMove { get; set; }
    public virtual ICollection<StockPicking> PickingIds { get; set; }
    public virtual int PickingCount { get; set; }
    public virtual bool FailedPickings { get; set; }
    public virtual StockPickingType PickingTypeId { get; set; }
    public virtual ProcurementGroup ProcurementGroupId { get; set; }
    public virtual string Note { get; set; }
    public virtual int NbPrint { get; set; }
    public virtual string PosReference { get; set; }
    public virtual AccountJournal SaleJournal { get; set; }
    public virtual AccountFiscalPosition FiscalPositionId { get; set; }
    public virtual ICollection<PosPayment> PaymentIds { get; set; }
    public virtual AccountMove SessionMoveId { get; set; }
    public virtual bool ToInvoice { get; set; }
    public virtual DateTime ShippingDate { get; set; }
    public virtual bool IsInvoiced { get; set; }
    public virtual bool IsTipped { get; set; }
    public virtual double TipAmount { get; set; }
    public virtual int RefundOrdersCount { get; set; }
    public virtual PosOrder RefundedOrderId { get; set; }
    public virtual bool HasRefundableLines { get; set; }
    public virtual string TicketCode { get; set; }
    public virtual string TrackingNumber { get; set; }
    public virtual string Uuid { get; set; }

    public virtual void ComputeCurrencyRate()
    {
        this.CurrencyRate = Env.ResCurrency.GetConversionRate(this.CompanyId.CurrencyId, this.CurrencyId, this.CompanyId, this.DateOrder.Date);
    }

    public virtual void ComputePickingCount()
    {
        this.PickingCount = this.PickingIds.Count;
        this.FailedPickings = this.PickingIds.Any(p => p.State != "done");
    }

    public virtual void ComputeIsInvoiced()
    {
        this.IsInvoiced = this.AccountMove != null;
    }

    public virtual void ComputeRefundRelatedFields()
    {
        this.RefundOrdersCount = this.Lines.SelectMany(l => l.RefundOrderlineIds.Select(r => r.OrderId)).Distinct().Count();
        this.RefundedOrderId = this.Lines.Select(l => l.RefundedOrderlineId.OrderId).FirstOrDefault();
    }

    public virtual void ComputeHasRefundableLines()
    {
        this.HasRefundableLines = this.Lines.Any(l => l.Qty > l.RefundedQty);
    }

    public virtual void ComputeTrackingNumber()
    {
        this.TrackingNumber = (this.SessionId.Id % 10) * 100 + this.SequenceNumber % 100;
        this.TrackingNumber = this.TrackingNumber.ToString().PadLeft(3, '0');
    }

    public virtual List<object> SearchTrackingNumber(string operator, object value)
    {
        if (operator == "ilike" || operator == "=")
        {
            string valueStr = value as string;
            if (valueStr.StartsWith("%") && valueStr.EndsWith("%"))
            {
                valueStr = valueStr[1..^1];
            }
            valueStr = valueStr.PadLeft(3, '0');
            string search = "% ____" + valueStr[0] + "-___-__" + valueStr[1..];
            return new List<object> { "PosReference", operator, search };
        }
        else
        {
            throw new NotImplementedException("Unsupported search operation");
        }
    }

    public virtual double _AmountLineTax(PosOrderLine line, AccountFiscalPosition fiscalPositionId)
    {
        ICollection<AccountTax> taxes = line.TaxIds.Where(t => Env.AccountTax.CheckCompanyDomain(t, this.CompanyId)).ToList();
        taxes = fiscalPositionId.MapTax(taxes);
        double price = line.PriceUnit * (1 - (line.Discount ?? 0.0) / 100.0);
        taxes = taxes.ComputeAll(price, this.CurrencyId, line.Qty, line.ProductId, this.PartnerId).Taxes;
        return taxes.Sum(t => t.Amount);
    }

    public virtual void _OnchangeAmountAll()
    {
        if (this.CurrencyId == null)
        {
            throw new UserError("You can't: create a pos order from the backend interface, or unset the pricelist, or create a pos.order in a python test with Form tool, or edit the form view in studio if no PoS order exist");
        }
        this.AmountPaid = this.PaymentIds.Sum(p => p.Amount);
        this.AmountReturn = this.PaymentIds.Where(p => p.Amount < 0).Sum(p => p.Amount);
        this.AmountTax = this.CurrencyId.Round(this.Lines.Sum(l => _AmountLineTax(l, this.FiscalPositionId)));
        double amountUntaxed = this.CurrencyId.Round(this.Lines.Sum(l => l.PriceSubtotal));
        this.AmountTotal = this.AmountTax + amountUntaxed;
    }

    public virtual void _ComputeBatchAmountAll()
    {
        Dictionary<int, Dictionary<string, double>> amounts = this.Select(o => o.Id).ToDictionary(orderId => orderId, orderId => new Dictionary<string, double> { { "paid", 0 }, { "return", 0 }, { "taxed", 0 }, { "taxes", 0 } });
        foreach (var posOrder in Env.PosPayment.ReadGroup(new List<object> { "pos_order_id", "in", this.Select(o => o.Id).ToList() }, new List<object> { "pos_order_id" }, new List<string> { "amount:sum" }))
        {
            amounts[(int)posOrder[0]]["paid"] = (double)posOrder[1];
        }
        foreach (var posOrder in Env.PosPayment.ReadGroup(new List<object> { "&", "pos_order_id", "in", this.Select(o => o.Id).ToList(), "amount", "<", 0 }, new List<object> { "pos_order_id" }, new List<string> { "amount:sum" }))
        {
            amounts[(int)posOrder[0]]["return"] = (double)posOrder[1];
        }
        foreach (var order in Env.PosOrderLine.ReadGroup(new List<object> { "order_id", "in", this.Select(o => o.Id).ToList() }, new List<object> { "order_id" }, new List<string> { "price_subtotal:sum", "price_subtotal_incl:sum" }))
        {
            amounts[(int)order[0]]["taxed"] = (double)order[1];
            amounts[(int)order[0]]["taxes"] = (double)order[2] - (double)order[1];
        }
        foreach (PosOrder order in this)
        {
            order.AmountPaid = amounts[order.Id]["paid"];
            order.AmountReturn = amounts[order.Id]["return"];
            order.AmountTax = this.CurrencyId.Round(amounts[order.Id]["taxes"]);
            order.AmountTotal = this.CurrencyId.Round(amounts[order.Id]["taxed"]);
        }
    }

    public virtual void _OnchangePartnerId()
    {
        if (this.PartnerId != null)
        {
            this.PricelistId = this.PartnerId.PropertyProductPricelist;
        }
    }

    public virtual void _UnlinkExceptDraftOrCancel()
    {
        foreach (PosOrder posOrder in this.Where(o => o.State != "draft" && o.State != "cancel"))
        {
            throw new UserError("In order to delete a sale, it must be new or cancelled.");
        }
    }

    public virtual void ActionStockPicking()
    {
        Action action = Env.IrActionsActWindow.GetForXmlId("stock.action_picking_tree_ready");
        action.DisplayName = "Pickings";
        action.Context = new Dictionary<string, object>();
        action.Domain = new List<object> { "id", "in", this.PickingIds.Select(p => p.Id).ToList() };
        // Do something with action
    }

    public virtual void ActionViewInvoice()
    {
        Dictionary<string, object> action = new Dictionary<string, object>
        {
            { "name", "Customer Invoice" },
            { "view_mode", "form" },
            { "view_id", Env.Ref("account.view_move_form").Id },
            { "res_model", "account.move" },
            { "context", "{ 'move_type':'out_invoice' }" },
            { "type", "ir.actions.act_window" },
            { "res_id", this.AccountMove.Id },
        };
        // Do something with action
    }

    public virtual void ActionViewRefundedOrder()
    {
        Dictionary<string, object> action = new Dictionary<string, object>
        {
            { "name", "Refunded Order" },
            { "view_mode", "form" },
            { "view_id", Env.Ref("point_of_sale.view_pos_pos_form").Id },
            { "res_model", "pos.order" },
            { "type", "ir.actions.act_window" },
            { "res_id", this.RefundedOrderId.Id },
        };
        // Do something with action
    }

    public virtual void ActionViewRefundOrders()
    {
        Dictionary<string, object> action = new Dictionary<string, object>
        {
            { "name", "Refund Orders" },
            { "view_mode", "tree,form" },
            { "res_model", "pos.order" },
            { "type", "ir.actions.act_window" },
            { "domain", new List<object> { "id", "in", this.Lines.SelectMany(l => l.RefundOrderlineIds.Select(r => r.OrderId)).ToList() } },
        };
        // Do something with action
    }

    public virtual bool _IsPosOrderPaid()
    {
        double amountTotal = this.AmountTotal;
        if (this.RefundedOrderId != null && this.CurrencyId.IsZero(this.RefundedOrderId.AmountTotal + amountTotal))
        {
            amountTotal = -this.RefundedOrderId.AmountPaid;
        }
        return this.CurrencyId.IsZero(this.CurrencyId.Round(amountTotal) - this.AmountPaid);
    }

    public virtual double _GetRoundedAmount(double amount, bool forceRound = false)
    {
        if (this.ConfigId.CashRounding && (forceRound || (!this.ConfigId.OnlyRoundCashMethod || this.PaymentIds.Any(p => p.PaymentMethodId.IsCashCount))))
        {
            amount = this.ConfigId.RoundingMethod.Round(amount, this.ConfigId.RoundingMethod.RoundingMethod);
        }
        return this.CurrencyId != null ? this.CurrencyId.Round(amount) : amount;
    }

    public virtual int _GetPartnerBankId()
    {
        int bankPartnerId = 0;
        if (this.AmountTotal <= 0 && this.PartnerId.BankIds.Any())
        {
            bankPartnerId = this.PartnerId.BankIds.First().Id;
        }
        else if (this.AmountTotal >= 0 && this.CompanyId.PartnerId.BankIds.Any())
        {
            bankPartnerId = this.CompanyId.PartnerId.BankIds.First().Id;
        }
        return bankPartnerId;
    }

    public virtual AccountMove _CreateInvoice(Dictionary<string, object> moveVals)
    {
        AccountMove newMove = Env.AccountMove.WithCompany(this.CompanyId).WithContext(new Dictionary<string, object> { { "default_move_type", moveVals["move_type"] } }).Create(moveVals);
        string message = $"This invoice has been created from the point of sale session: {this._GetHtmlLink()}";
        newMove.MessagePost(message);
        if (this.ConfigId.CashRounding)
        {
            using (new AccountMove.CheckBalancedScope(newMove))
            {
                double roundingApplied = this.ConfigId.RoundingMethod.Round(this.AmountPaid - this.AmountTotal, newMove.CurrencyId.Rounding);
                AccountMoveLine roundingLine = newMove.LineIds.FirstOrDefault(l => l.DisplayType == "rounding");
                if (roundingLine != null)
                {
                    double roundingLineDifference = roundingLine.Debit + roundingApplied;
                    if (roundingLine.Credit > 0)
                    {
                        roundingLineDifference = -roundingLine.Credit + roundingApplied;
                    }
                }
                else
                {
                    roundingLineDifference = roundingApplied;
                }
                if (roundingApplied != 0)
                {
                    int accountId = roundingApplied > 0.0 ? newMove.InvoiceCashRoundingId.LossAccountId.Id : newMove.InvoiceCashRoundingId.ProfitAccountId.Id;
                    if (roundingLine != null)
                    {
                        if (roundingLineDifference != 0)
                        {
                            roundingLine.WithContext(new Dictionary<string, object> { { "skip_invoice_sync", true } }).Write(new Dictionary<string, object>
                            {
                                { "Debit", roundingApplied < 0.0 ? -roundingApplied : 0.0 },
                                { "Credit", roundingApplied > 0.0 ? roundingApplied : 0.0 },
                                { "AccountId", accountId },
                                { "PriceUnit", roundingApplied },
                            });
                        }
                    }
                    else
                    {
                        Env.AccountMoveLine.WithContext(new Dictionary<string, object> { { "skip_invoice_sync", true } }).Create(new Dictionary<string, object>
                        {
                            { "Balance", -roundingApplied },
                            { "Quantity", 1.0 },
                            { "PartnerId", newMove.PartnerId.Id },
                            { "MoveId", newMove.Id },
                            { "CurrencyId", newMove.CurrencyId.Id },
                            { "CompanyId", newMove.CompanyId.Id },
                            { "CompanyCurrencyId", newMove.CompanyId.CurrencyId.Id },
                            { "DisplayType", "rounding" },
                            { "Sequence", 9999 },
                            { "Name", this.ConfigId.RoundingMethod.Name },
                            { "AccountId", accountId },
                        });
                    }
                }
                else if (roundingLine != null)
                {
                    roundingLine.WithContext(new Dictionary<string, object> { { "skip_invoice_sync", true } }).Unlink();
                }
                if (roundingLineDifference != 0)
                {
                    AccountMoveLine existingTermsLine = newMove.LineIds.FirstOrDefault(l => l.AccountId.AccountType == "asset_receivable" || l.AccountId.AccountType == "liability_payable");
                    double existingTermsLineNewVal = this.ConfigId.RoundingMethod.Round(existingTermsLine.Balance + roundingLineDifference, newMove.CurrencyId.Rounding);
                    existingTermsLine.WithContext(new Dictionary<string, object> { { "skip_invoice_sync", true } }).Balance = existingTermsLineNewVal;
                }
            }
        }
        return newMove;
    }

    public virtual bool ActionPosOrderPaid()
    {
        double total = this.ConfigId.CashRounding ? this.ConfigId.RoundingMethod.Round(this.AmountTotal, this.ConfigId.RoundingMethod.RoundingMethod) : this.AmountTotal;
        bool isPaid = this.CurrencyId.IsZero(total - this.AmountPaid);
        if (!isPaid && !this.ConfigId.CashRounding)
        {
            throw new UserError($"Order {this.Name} is not fully paid.");
        }
        else if (!isPaid && this.ConfigId.CashRounding)
        {
            double maxDiff = this.ConfigId.RoundingMethod.RoundingMethod == "HALF-UP" ? this.CurrencyId.Round(this.ConfigId.RoundingMethod.Rounding / 2) : this.CurrencyId.Round(this.ConfigId.RoundingMethod.Rounding);
            double diff = this.CurrencyId.Round(this.AmountTotal - this.AmountPaid);
            if (Math.Abs(diff) > maxDiff)
            {
                throw new UserError($"Order {this.Name} is not fully paid.");
            }
        }
        this.State = "paid";
        return true;
    }

    public virtual Dictionary<string, object> _PrepareInvoiceVals()
    {
        TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById(Env.Context.Get<string>("tz") ?? Env.User.Tz ?? "UTC");
        DateTime invoiceDate = this.SessionId.State == "closed" ? DateTime.Now : this.DateOrder;
        List<int> posRefundedInvoiceIds = this.Lines.Where(l => l.RefundedOrderlineId != null && l.RefundedOrderlineId.OrderId.AccountMove != null).Select(l => l.RefundedOrderlineId.OrderId.AccountMove.Id).ToList();
        Dictionary<string, object> vals = new Dictionary<string, object>
        {
            { "InvoiceOrigin", this.Name },
            { "PosRefundedInvoiceIds", posRefundedInvoiceIds },
            { "JournalId", this.SessionId.ConfigId.InvoiceJournalId.Id },
            { "MoveType", this.AmountTotal >= 0 ? "out_invoice" : "out_refund" },
            { "Ref", this.Name },
            { "PartnerId", this.PartnerId.AddressGet(new List<string> { "invoice" })["invoice"] },
            { "PartnerBankId", this._GetPartnerBankId() },
            { "CurrencyId", this.CurrencyId.Id },
            { "InvoiceUserId", this.UserId.Id },
            { "InvoiceDate", TimeZoneInfo.ConvertTime(invoiceDate, timezone).Date },
            { "FiscalPositionId", this.FiscalPositionId.Id },
            { "InvoiceLineIds", this._PrepareInvoiceLines() },
            { "InvoicePaymentTermId", this.PartnerId.PropertyPaymentTermId?.Id },
            { "InvoiceCashRoundingId", this.ConfigId.CashRounding && (!this.ConfigId.OnlyRoundCashMethod || this.PaymentIds.Any(p => p.PaymentMethodId.IsCashCount)) ? this.ConfigId.RoundingMethod.Id : null },
        };
        if (this.RefundedOrderId != null && this.RefundedOrderId.AccountMove != null)
        {
            vals["Ref"] = $"Reversal of: {this.RefundedOrderId.AccountMove.Name}";
            vals["ReversedEntryId"] = this.RefundedOrderId.AccountMove.Id;
        }
        if (!string.IsNullOrEmpty(this.Note))
        {
            vals["Narration"] = this.Note;
        }
        return vals;
    }

    public virtual List<object> _PrepareInvoiceLines()
    {
        int sign = this.AmountTotal >= 0 ? 1 : -1;
        List<Dictionary<string, object>> lineValuesList = this._PrepareTaxBaseLineValues(sign);
        List<object> invoiceLines = new List<object>();
        foreach (Dictionary<string, object> lineValues in lineValuesList)
        {
            PosOrderLine line = (PosOrderLine)lineValues["record"];
            Dictionary<string, object> invoiceLinesValues = this._GetInvoiceLinesValues(lineValues, line);
            invoiceLines.Add(new List<object> { 0, null, invoiceLinesValues });
            if (this.PricelistId.DiscountPolicy == "without_discount" && this.CurrencyId.Compare(line.PriceUnit, line.ProductId.LstPrice) < 0)
            {
                invoiceLines.Add(new List<object> { 0, null,