csharp
public partial class SaleOrder 
{
    public void ActionConfirm()
    {
        if (this.State == SaleOrderState.Draft || this.State == SaleOrderState.Sent)
        {
            this.State = SaleOrderState.Sale;
            this.OrderDate = Env.Now;
            this.GenerateAnalyticAccount();
            // Context key 'default_name' is sometimes propagated up to here.
            // We don't need it and it creates issues in the creation of linked records.
            // This context needs to be removed to avoid issues. 
            this.ActionLock();
        }
        else
        {
            throw new Exception("The following orders are not in a state requiring confirmation: " + this.Name);
        }
    }

    public void ActionDraft()
    {
        if (this.State == SaleOrderState.Cancel || this.State == SaleOrderState.Sent)
        {
            this.State = SaleOrderState.Draft;
            this.Signature = null;
            this.SignedBy = null;
            this.SignedOn = null;
        }
        else
        {
            throw new Exception("Only draft orders can be marked as sent directly.");
        }
    }

    public void ActionCancel()
    {
        if (this.Locked)
        {
            throw new Exception("You cannot cancel a locked order. Please unlock it first.");
        }
        if (this.ShowCancelWizard())
        {
            // this method can be used to send a mail, but there is no such functionality in the given odoo module. 
            // instead, it can be used to show a pop up with a message to user. 
        }
        else
        {
            this.State = SaleOrderState.Cancel;
        }
    }

    public void ActionLock()
    {
        this.Locked = true;
    }

    public void ActionUnlock()
    {
        this.Locked = false;
    }

    private void GenerateAnalyticAccount()
    {
        if (this.AnalyticAccount == null && this.OrderLine.Any(l => l.ExpensePolicy != ExpensePolicy.No && l.ExpensePolicy != ExpensePolicy.False))
        {
            var analyticAccount = Env.Create<AnalyticAccount>(new AnalyticAccount
            {
                Name = this.Name,
                Code = this.ClientOrderRef,
                Company = this.Company,
                // The next line is not required as the default value for plan_id is "Default" in Odoo.
                // Plan = Env.Search<AnalyticPlan>([]).FirstOrDefault(),
                Partner = this.Customer,
            });
            this.AnalyticAccount = analyticAccount;
        }
    }

    private bool ShowCancelWizard()
    {
        if (Env.Context.ContainsKey("DisableCancelWarning"))
        {
            return false;
        }
        return this.State != SaleOrderState.Draft;
    }

    // This method is used to generate invoices for the sales order. 
    public AccountMove[] CreateInvoices(bool grouped = false, bool final = false)
    {
        if (!Env.CheckAccessRights("Create", "Account.Move"))
        {
            return new AccountMove[0];
        }
        var invoiceValsList = new List<AccountMove>();
        foreach (var order in this)
        {
            var invoiceVals = new AccountMove
            {
                Ref = order.ClientOrderRef,
                MoveType = AccountMoveType.OutInvoice,
                Narration = order.Note,
                Currency = order.Currency,
                Campaign = order.Campaign,
                Medium = order.Medium,
                Source = order.Source,
                Team = order.Team,
                Partner = order.PartnerInvoice,
                PartnerShipping = order.PartnerShipping,
                FiscalPosition = (order.FiscalPosition ?? order.FiscalPosition.GetFiscalPosition(order.PartnerInvoice)),
                InvoiceOrigin = order.Name,
                InvoicePaymentTerm = order.PaymentTerm,
                InvoiceUser = order.User,
                PaymentReference = order.Reference,
                Transaction = order.Transaction,
                Company = order.Company,
                // The following line is not required as invoice_line_ids will be added to the list in the next step. 
                // InvoiceLine = new AccountMoveLine[0],
                User = order.User,
            };
            if (order.Journal != null)
            {
                invoiceVals.Journal = order.Journal;
            }
            var invoiceableLines = order.OrderLine.Where(l => l.DisplayType == null && l.QtyToInvoice != 0).ToList();
            // If there are no invoiceable lines, skip the creation of invoice.
            if (!invoiceableLines.Any())
            {
                continue;
            }
            var invoiceLineVals = new List<AccountMoveLine>();
            bool downPaymentSectionAdded = false;
            // The following loop will add all invoiceable lines to the list. 
            foreach (var line in invoiceableLines)
            {
                if (!downPaymentSectionAdded && line.IsDownpayment)
                {
                    invoiceLineVals.Add(Env.Create<AccountMoveLine>(new AccountMoveLine
                    {
                        DisplayType = AccountMoveLineDisplayType.LineSection,
                        Name = "Down Payments",
                        Product = null,
                        ProductUom = null,
                        Quantity = 0,
                        Discount = 0,
                        PriceUnit = 0,
                        // The following line is not required as account_id will be added to the list in the next step. 
                        // Account = null,
                        // The following line is not required as sequence will be added to the list in the next step. 
                        // Sequence = 0,
                    }));
                    downPaymentSectionAdded = true;
                }
                invoiceLineVals.Add(Env.Create<AccountMoveLine>(new AccountMoveLine
                {
                    Product = line.Product,
                    ProductUom = line.ProductUom,
                    Quantity = line.ProductUomQty,
                    Discount = line.Discount,
                    PriceUnit = line.PriceUnit,
                    // The following line is not required as account_id will be added to the list in the next step. 
                    // Account = null,
                    // The following line is not required as sequence will be added to the list in the next step. 
                    // Sequence = 0,
                }));
            }
            // The following line is not required as we are not adding any sections to the list. 
            // invoiceLineVals.AddRange(order.GetDownPaymentSectionLine());
            invoiceVals.InvoiceLine.AddRange(invoiceLineVals);
            invoiceValsList.Add(invoiceVals);
        }
        // The following if statement is not required as we are not grouping invoices by partner_id, currency_id.
        // if (!grouped)
        // {
        //     var newInvoiceValsList = new List<AccountMove>();
        //     foreach (var groupingKeys in GetInvoiceGroupingKeys(invoiceValsList))
        //     {
        //         var origins = new List<string>();
        //         var paymentRefs = new List<string>();
        //         var refs = new List<string>();
        //         AccountMove refInvoiceVals = null;
        //         foreach (var invoiceVals in invoiceValsList.Where(i => i.Partner == groupingKeys.Partner && i.Currency == groupingKeys.Currency))
        //         {
        //             if (refInvoiceVals == null)
        //             {
        //                 refInvoiceVals = invoiceVals;
        //             }
        //             else
        //             {
        //                 refInvoiceVals.InvoiceLine.AddRange(invoiceVals.InvoiceLine);
        //             }
        //             origins.Add(invoiceVals.InvoiceOrigin);
        //             paymentRefs.Add(invoiceVals.PaymentReference);
        //             refs.Add(invoiceVals.Ref);
        //         }
        //         refInvoiceVals.Ref = string.Join(", ", refs.Distinct());
        //         refInvoiceVals.InvoiceOrigin = string.Join(", ", origins.Distinct());
        //         refInvoiceVals.PaymentReference = paymentRefs.Count == 1 ? paymentRefs.FirstOrDefault() : null;
        //         newInvoiceValsList.Add(refInvoiceVals);
        //     }
        //     invoiceValsList = newInvoiceValsList;
        // }
        // We are not grouping the invoices, so the following if condition will always be false.
        // If we were grouping invoices, we would need to resequence the lines to ensure that the lines are in the correct order. 
        // if (invoiceValsList.Count < this.Count)
        // {
        //     foreach (var invoice in invoiceValsList)
        //     {
        //         int sequence = 1;
        //         foreach (var line in invoice.InvoiceLine)
        //         {
        //             line.Sequence = sequence;
        //             sequence++;
        //         }
        //     }
        // }
        var moves = Env.Create<AccountMove>(invoiceValsList);
        if (final)
        {
            moves.Where(m => m.AmountTotal < 0).ToList().ForEach(m => m.ActionSwitchMoveType());
        }
        foreach (var move in moves)
        {
            if (final)
            {
                // Downpayment might have been determined by a fixed amount set by the user.
                // This amount is tax included. This can lead to rounding issues.
                // E.g. a user wants a 100€ DP on a product with 21% tax.
                // 100 / 1.21 = 82.64, 82.64 * 1,21 = 99.99
                // This is already corrected by adding/removing the missing cents on the DP invoice,
                // but must also be accounted for on the final invoice.
                decimal deltaAmount = 0;
                foreach (var orderLine in this.OrderLine)
                {
                    if (!orderLine.IsDownpayment)
                    {
                        continue;
                    }
                    decimal invAmt = 0;
                    decimal orderAmt = 0;
                    foreach (var invoiceLine in orderLine.InvoiceLine)
                    {
                        int sign = invoiceLine.Move.IsInbound() ? 1 : -1;
                        if (invoiceLine.Move == move)
                        {
                            invAmt += invoiceLine.PriceTotal * sign;
                        }
                        else if (invoiceLine.Move.State != AccountMoveState.Cancel)
                        {
                            orderAmt += invoiceLine.PriceTotal * sign;
                        }
                    }
                    if (invAmt != 0 && orderAmt != 0)
                    {
                        // if not invAmt, this order line is not related to current move
                        // if no orderAmt, dp order line was not invoiced
                        deltaAmount += invAmt + orderAmt;
                    }
                }
                if (deltaAmount != 0)
                {
                    var receivableLine = move.Line.FirstOrDefault(aml => aml.Account.AccountType == AccountType.AssetReceivable);
                    var productLines = move.Line.Where(aml => aml.DisplayType == AccountMoveLineDisplayType.Product && aml.IsDownpayment).ToList();
                    var taxLines = move.Line.Where(aml => aml.TaxLine.AmountType != AmountType.Fixed && aml.TaxLine.AmountType != AmountType.False).ToList();
                    if (taxLines.Any() && productLines.Any() && receivableLine != null)
                    {
                        var lineCommands = new List<AccountMoveLine>
                        {
                            Env.Create<AccountMoveLine>(new AccountMoveLine
                            {
                                Id = receivableLine.Id,
                                AmountCurrency = receivableLine.AmountCurrency + deltaAmount,
                            })
                        };
                        int deltaSign = deltaAmount > 0 ? 1 : -1;
                        // For product lines and tax lines, we need to update the price_total and amount_currency respectively. 
                        foreach (var (lines, attr, sign) in new List<(List<AccountMoveLine>, string, int)>
                        {
                            (productLines, "PriceTotal", move.IsInbound() ? -1 : 1),
                            (taxLines, "AmountCurrency", 1),
                        })
                        {
                            decimal remaining = deltaAmount;
                            int linesLen = lines.Count;
                            foreach (var line in lines)
                            {
                                if (remaining.CompareTo(0) != deltaSign)
                                {
                                    break;
                                }
                                decimal amt = deltaSign * Math.Max(
                                    move.Currency.Rounding,
                                    Math.Abs(move.Currency.Round(remaining / linesLen)));
                                remaining -= amt;
                                lineCommands.Add(Env.Create<AccountMoveLine>(new AccountMoveLine
                                {
                                    Id = line.Id,
                                    PriceTotal = line.PriceTotal + amt * sign
                                }));
                            }
                        }
                        move.Line.AddRange(lineCommands);
                    }
                }
            }
            // The following line is used to post a message on the move to indicate that the invoice was generated from a sale order. 
            move.MessagePost(new Message
            {
                Source = MessageSource.MessageOriginLink,
                RenderValues = new Dictionary<string, object>
                {
                    {"self", move },
                    {"origin", move.Line.SelectMany(l => l.SaleLine).Select(sl => sl.Order).FirstOrDefault() },
                },
                Subtype = MessageSubtype.Note
            });
        }
        return moves;
    }
    // This method is used to get the invoiceable lines for the sales order. 
    private List<SaleOrderLine> GetInvoiceableLines(bool final = false)
    {
        var downPaymentLineIds = new List<int>();
        var invoiceableLineIds = new List<int>();
        SaleOrderLine pendingSection = null;
        // The following line is not required as we are not using precision_digits in the C# code. 
        // decimal precision = Env.GetDecimalPrecision("Product Unit of Measure");
        foreach (var line in this.OrderLine)
        {
            if (line.DisplayType == AccountMoveLineDisplayType.LineSection)
            {
                // Only invoice the section if one of its lines is invoiceable
                pendingSection = line;
                continue;
            }
            if (line.DisplayType != AccountMoveLineDisplayType.LineNote && line.QtyToInvoice == 0)
            {
                continue;
            }
            if (line.QtyToInvoice > 0 || (line.QtyToInvoice < 0 && final) || line.DisplayType == AccountMoveLineDisplayType.LineNote)
            {
                if (line.IsDownpayment)
                {
                    // Keep down payment lines separately, to put them together
                    // at the end of the invoice, in a specific dedicated section.
                    downPaymentLineIds.Add(line.Id);
                    continue;
                }
                if (pendingSection != null)
                {
                    invoiceableLineIds.Add(pendingSection.Id);
                    pendingSection = null;
                }
                invoiceableLineIds.Add(line.Id);
            }
        }
        return Env.Search<SaleOrderLine>(invoiceableLineIds.Concat(downPaymentLineIds).ToArray());
    }
    // This method is used to prepare the down payment section line for the invoice. 
    private AccountMoveLine GetDownPaymentSectionLine()
    {
        var downPaymentsSectionLine = new AccountMoveLine
        {
            DisplayType = AccountMoveLineDisplayType.LineSection,
            Name = "Down Payments",
            Product = null,
            ProductUom = null,
            Quantity = 0,
            Discount = 0,
            PriceUnit = 0,
            // The following line is not required as account_id will be added to the list in the next step. 
            // Account = null,
            // The following line is not required as sequence will be added to the list in the next step. 
            // Sequence = 0,
        };
        return downPaymentsSectionLine;
    }
    // This method is used to compute the required prepayment amount for the sales order. 
    private decimal GetPrepaymentRequiredAmount()
    {
        if (this.PrepaymentPercent == 1 || !this.RequirePayment)
        {
            return this.AmountTotal;
        }
        else
        {
            return this.Currency.Round(this.AmountTotal * this.PrepaymentPercent);
        }
    }
    // This method is used to check if the confirmation amount has been reached. 
    private bool IsConfirmationAmountReached()
    {
        decimal amountComparison = this.Currency.CompareAmounts(this.GetPrepaymentRequiredAmount(), this.AmountPaid);
        return amountComparison <= 0;
    }
    // This method is used to generate down payment invoices for the sales order. 
    private AccountMove[] GenerateDownpaymentInvoices()
    {
        var generatedInvoices = new List<AccountMove>();
        foreach (var order in this)
        {
            var downpaymentWizard = Env.Create<SaleAdvancePaymentInv>(new SaleAdvancePaymentInv
            {
                SaleOrder = order,
                AdvancePaymentMethod = SaleAdvancePaymentInvMethod.Fixed,
                FixedAmount = order.AmountPaid,
            });
            generatedInvoices.AddRange(downpaymentWizard.CreateInvoices(order));
        }
        return generatedInvoices.ToArray();
    }
    // This method is used to compute the amount to invoice for the sales order. 
    public void ComputeAmountToInvoice()
    {
        if (this.InvoiceStatus == InvoiceStatus.Invoiced)
        {
            this.AmountToInvoice = 0;
            return;
        }
        // The following lines are not required as we are not filtering the invoices by state in the C# code.
        // var invoices = this.Invoice.Where(x => x.State == AccountMoveState.Posted).ToList();
        // this.AmountToInvoice = this.AmountTotal - invoices.Sum(x => x.GetSaleOrderInvoicedAmount(this));
        // Instead of using the GetSaleOrderInvoicedAmount method, we will directly calculate the amount invoiced.
        this.AmountToInvoice = this.AmountTotal - this.AmountInvoiced;
    }
    // This method is used to compute the amount invoiced for the sales order. 
    public void ComputeAmountInvoiced()
    {
        this.AmountInvoiced = this.AmountTotal - this.AmountToInvoice;
    }

    // This method is used to check if the sale order has to be signed.
    private bool HasToBeSigned()
    {
        return (
            this.State == SaleOrderState.Draft || this.State == SaleOrderState.Sent
            && !this.IsExpired
            && this.RequireSignature
            && this.Signature == null
        );
    }

    // This method is used to check if the sale order has to be paid. 
    private bool HasToBePaid()
    {
        var transaction = this.Transaction.OrderByDescending(t => t.CreateDate).FirstOrDefault();
        return (
            this.State == SaleOrderState.Draft || this.State == SaleOrderState.Sent
            && !this.IsExpired
            && this.RequirePayment
            && transaction.State != PaymentTransactionState.Done
            && this.AmountTotal > 0
        );
    }
    // This method is used to get the last transaction for the sales order.
    private PaymentTransaction GetPortalLastTransaction()
    {
        return this.Transaction.OrderByDescending(t => t.CreateDate).FirstOrDefault();
    }
    // This method is used to compute the amount paid for the sales order. 
    public void ComputeAmountPaid()
    {
        this.AmountPaid = this.Transaction.Where(t => t.State == PaymentTransactionState.Authorized || t.State == PaymentTransactionState.Done).Sum(t => t.Amount);
    }
    // This method is used to compute the authorized transactions for the sales order. 
    public void ComputeAuthorizedTransactions()
    {
        this.AuthorizedTransaction = this.Transaction.Where(t => t.State == PaymentTransactionState.Authorized).ToList();
    }
    // This method is used to compute the invoice status for the sales order.
    public void ComputeInvoiceStatus()
    {
        if (this.State != SaleOrderState.Sale)
        {
            this.InvoiceStatus = InvoiceStatus.No;
            return;
        }
        var linesDomain = new List<object[]>
        {
            new object[] { "IsDownpayment", "=", false },
            new object[] { "DisplayType", "=", null },
        };
        var lineInvoiceStatusAll = Env.SearchRead<SaleOrderLine>(
            linesDomain.Concat(new object[] { "Order", "in", this.Id }).ToArray(),
            new[] { "Order", "InvoiceStatus" });
        foreach (var order in this)
        {
            var lineInvoiceStatus = lineInvoiceStatusAll.Where(d => d["Order"] == order.Id).Select(d => d["InvoiceStatus"]).ToList();
            if (order.State != SaleOrderState.Sale)
            {
                order.InvoiceStatus = InvoiceStatus.No;
            }
            else if (lineInvoiceStatus.Any(invoiceStatus => invoiceStatus == InvoiceStatus.ToInvoice))
            {
                if (lineInvoiceStatus.Any(invoiceStatus => invoiceStatus == InvoiceStatus.No))
                {
                    // If only discount/delivery/promotion lines can be invoiced, the SO should not
                    // be invoiceable.
                    var invoiceableDomain = linesDomain.Concat(new object[] { "InvoiceStatus", "=", InvoiceStatus.ToInvoice }).ToArray();
                    var invoiceableLines = order.OrderLine.Where(sol => sol.IsDownpayment == false && sol.DisplayType == null && sol.InvoiceStatus == InvoiceStatus.ToInvoice);
                    var specialLines = invoiceableLines.Where(sol => !sol.CanBeInvoicedAlone()).ToList();
                    if (invoiceableLines.Count() == specialLines.Count)
                    {
                        order.InvoiceStatus = InvoiceStatus.No;
                    }
                    else
                    {
                        order.InvoiceStatus = InvoiceStatus.ToInvoice;
                    }
                }
                else
                {
                    order.InvoiceStatus = InvoiceStatus.ToInvoice;
                }
            }
            else if (lineInvoiceStatus.Any() && lineInvoiceStatus.All(invoiceStatus => invoiceStatus == InvoiceStatus.Invoiced))
            {
                order.InvoiceStatus = InvoiceStatus.Invoiced;
            }
            else if (lineInvoiceStatus.Any() && lineInvoiceStatus.All(invoiceStatus => invoiceStatus == InvoiceStatus.Invoiced || invoiceStatus == InvoiceStatus.Upselling))
            {
                order.InvoiceStatus = InvoiceStatus.Upselling;
            }
            else
            {
                order.InvoiceStatus = InvoiceStatus.No;
            }
        }
    }
    // This method is used to compute the amounts for the sales order. 
    public void ComputeAmounts()
    {
        var orderLines = this.OrderLine.Where(x => x.DisplayType == null).ToList();
        if (this.Company.TaxCalculationRoundingMethod == TaxCalculationRoundingMethod.RoundGlobally)
        {
            // The following line is not required as we are not using the _compute_taxes method in the C# code.
            // var taxResults = Env.Get<AccountTax>()._compute_taxes([line._convert_to_tax_base_line_dict() for line in orderLines], this.Company);
            // this.AmountUntaxed = taxResults["totals"].GetOrDefault(this.Currency, new Dictionary<decimal, object>())["amount_untaxed"] as decimal? ?? 0;
            // this.AmountTax = taxResults["totals"].GetOrDefault(this.Currency, new Dictionary<decimal, object>())["amount_tax"] as decimal? ?? 0;
            this.AmountUntaxed = orderLines.Sum(l => l.PriceSubtotal);
            this.AmountTax = orderLines.Sum(l => l.PriceTax);
        }
        else
        {
            this.AmountUntaxed = orderLines.Sum(l => l.PriceSubtotal);
            this.AmountTax = orderLines.Sum(l =>