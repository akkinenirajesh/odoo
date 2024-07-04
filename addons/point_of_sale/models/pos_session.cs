csharp
public partial class PosSession {
    public void Login() {
        this.LoginNumber += 1;
        this.Write(new Dictionary<string, object>() {
            { "LoginNumber", this.LoginNumber }
        });
    }

    public bool ActionPosSessionOpen() {
        if (this.State == "OpeningControl") {
            Dictionary<string, object> values = new Dictionary<string, object>();
            if (this.StartAt == null) {
                values.Add("StartAt", DateTime.Now);
            }
            if (this.ConfigId.CashControl && !this.Rescue) {
                PosSession lastSession = Env.Model<PosSession>().Search(new[] {
                    new Tuple<string, object>("ConfigId", this.ConfigId.Id),
                    new Tuple<string, object>("Id", "!=", this.Id)
                }, 1);
                this.CashRegisterBalanceStart = lastSession.CashRegisterBalanceEndReal;
            } else {
                values.Add("State", "Opened");
            }
            this.Write(values);
        }
        return true;
    }

    public PosSession ActionPosSessionClosingControl(Account.Account balancingAccount = null, double amountToBalance = 0, Dictionary<int, double> bankPaymentMethodDiffs = null) {
        if (bankPaymentMethodDiffs == null) {
            bankPaymentMethodDiffs = new Dictionary<int, double>();
        }
        if (this.OrderIds.Any(o => o.State == "Draft")) {
            throw new UserError(Env.Translate("You cannot close the POS when orders are still in draft"));
        }
        if (this.State == "Closed") {
            throw new UserError(Env.Translate("This session is already closed."));
        }
        this.Write(new Dictionary<string, object>() {
            { "State", "ClosingControl" },
            { "StopAt", DateTime.Now },
        });
        if (!this.ConfigId.CashControl) {
            return this.ActionPosSessionClose(balancingAccount, amountToBalance, bankPaymentMethodDiffs);
        }
        if (this.Rescue && this.ConfigId.CashControl) {
            PosPaymentMethod defaultCashPaymentMethod = this.PaymentMethodIds.Where(pm => pm.Type == "Cash").FirstOrDefault();
            List<PosOrder> orders = GetClosedOrders();
            double totalCash = orders.Sum(order => order.PaymentIds.Where(p => p.PaymentMethodId == defaultCashPaymentMethod).Sum(p => p.Amount))
                + this.CashRegisterBalanceStart;
            this.CashRegisterBalanceEndReal = totalCash;
        }
        return this.ActionPosSessionValidate(balancingAccount, amountToBalance, bankPaymentMethodDiffs);
    }

    public PosSession ActionPosSessionValidate(Account.Account balancingAccount = null, double amountToBalance = 0, Dictionary<int, double> bankPaymentMethodDiffs = null) {
        if (bankPaymentMethodDiffs == null) {
            bankPaymentMethodDiffs = new Dictionary<int, double>();
        }
        return this.ActionPosSessionClose(balancingAccount, amountToBalance, bankPaymentMethodDiffs);
    }

    public bool ActionPosSessionClose(Account.Account balancingAccount = null, double amountToBalance = 0, Dictionary<int, double> bankPaymentMethodDiffs = null) {
        if (bankPaymentMethodDiffs == null) {
            bankPaymentMethodDiffs = new Dictionary<int, double>();
        }
        return ValidateSession(balancingAccount, amountToBalance, bankPaymentMethodDiffs);
    }

    public bool ValidateSession(Account.Account balancingAccount = null, double amountToBalance = 0, Dictionary<int, double> bankPaymentMethodDiffs = null) {
        if (bankPaymentMethodDiffs == null) {
            bankPaymentMethodDiffs = new Dictionary<int, double>();
        }
        Dictionary<string, object> data = new Dictionary<string, object>();
        bool sudo = Env.User.IsInGroup("point_of_sale.group_pos_user");
        if (this.OrderIds.Any(o => o.State != "Cancel") || this.StatementLineIds.Any()) {
            this.CashRealTransaction = this.StatementLineIds.Sum(sl => sl.Amount);
            if (this.State == "Closed") {
                throw new UserError(Env.Translate("This session is already closed."));
            }
            CheckIfNoDraftOrders();
            CheckInvoicesArePosted();
            double cashDifferenceBeforeStatements = this.CashRegisterDifference;
            if (this.UpdateStockAtClosing) {
                CreatePickingAtEndOfSession();
                this.OrderIds.Where(o => !o.IsTotalCostComputed).ToList().ForEach(o => o.ComputeTotalCostAtSessionClosing(this.PickingIds.MoveIds));
            }
            try {
                using (Env.Cr.Savepoint()) {
                    data = this.WithCompany(this.CompanyId).WithContext(new Dictionary<string, object>() {
                        { "check_move_validity", false },
                        { "skip_invoice_sync", true },
                    })._CreateAccountMove(balancingAccount, amountToBalance, bankPaymentMethodDiffs);
                }
            } catch (AccessError e) {
                if (sudo) {
                    data = this.Sudo().WithCompany(this.CompanyId).WithContext(new Dictionary<string, object>() {
                        { "check_move_validity", false },
                        { "skip_invoice_sync", true },
                    })._CreateAccountMove(balancingAccount, amountToBalance, bankPaymentMethodDiffs);
                } else {
                    throw e;
                }
            }
            double balance = this.MoveId.LineIds.Sum(l => l.Balance);
            try {
                using (this.MoveId._CheckBalanced(new Dictionary<string, object>() {
                    { "records", this.MoveId.Sudo() }
                })) {
                    // do nothing
                }
            } catch (UserError) {
                Env.Cr.Rollback();
                return CloseSessionAction(balance);
            }
            this.Sudo()._PostStatementDifference(cashDifferenceBeforeStatements);
            if (this.MoveId.LineIds.Any()) {
                this.MoveId.Sudo().WithCompany(this.CompanyId)._Post();
                this.Env.Model<PosOrder>().Search(new[] {
                    new Tuple<string, object>("SessionId", this.Id),
                    new Tuple<string, object>("State", "Paid")
                }).Write(new Dictionary<string, object>() {
                    { "State", "Done" }
                });
            } else {
                this.MoveId.Sudo().Unlink();
            }
            this.Sudo().WithCompany(this.CompanyId)._ReconcileAccountMoveLines(data);
        } else {
            this.Sudo()._PostStatementDifference(this.CashRegisterDifference);
        }
        this.Write(new Dictionary<string, object>() {
            { "State", "Closed" }
        });
        return true;
    }

    public void PostClosingCashDetails(double countedCash) {
        CheckClosingSession();
        if (this.CashJournalId == null) {
            throw new UserError(Env.Translate("There is no cash register in this session."));
        }
        this.CashRegisterBalanceEndReal = countedCash;
    }

    public void UpdateClosingControlStateSession(string notes) {
        if (this.State == "Closed") {
            throw new UserError(Env.Translate("This session is already closed."));
        }
        this.Write(new Dictionary<string, object>() {
            { "State", "ClosingControl" },
            { "StopAt", DateTime.Now },
            { "ClosingNotes", notes }
        });
        this._PostCashDetailsMessage("Closing", this.CashRegisterBalanceEnd, this.CashRegisterDifference, notes);
    }

    public Dictionary<string, object> GetClosingControlData() {
        if (!Env.User.IsInGroup("point_of_sale.group_pos_user")) {
            throw new AccessError(Env.Translate("You don't have the access rights to get the point of sale closing control data."));
        }
        List<PosOrder> orders = GetClosedOrders();
        List<PosPayment> payments = orders.SelectMany(o => o.PaymentIds).Where(p => p.PaymentMethodId.Type != "PayLater").ToList();
        List<PosPaymentMethod> cashPaymentMethodIds = this.PaymentMethodIds.Where(pm => pm.Type == "Cash").ToList();
        PosPaymentMethod defaultCashPaymentMethod = cashPaymentMethodIds.FirstOrDefault();
        double totalDefaultCashPaymentAmount = defaultCashPaymentMethod != null
            ? payments.Where(p => p.PaymentMethodId == defaultCashPaymentMethod).Sum(p => p.Amount)
            : 0;
        List<PosPaymentMethod> otherPaymentMethodIds = defaultCashPaymentMethod != null
            ? this.PaymentMethodIds.Except(new List<PosPaymentMethod>() { defaultCashPaymentMethod }).ToList()
            : this.PaymentMethodIds.ToList();
        int cashInCount = 0;
        int cashOutCount = 0;
        List<Dictionary<string, object>> cashInOutList = new List<Dictionary<string, object>>();
        foreach (Account.BankStatementLine cashMove in this.Sudo().StatementLineIds.OrderBy(sl => sl.CreateDate)) {
            if (cashMove.Amount > 0) {
                cashInCount += 1;
            } else {
                cashOutCount += 1;
            }
            cashInOutList.Add(new Dictionary<string, object>() {
                { "Name", cashMove.PaymentRef ?? $"Cash {(cashMove.Amount > 0 ? "in" : "out")} {(cashMove.Amount > 0 ? cashInCount : cashOutCount)}" },
                { "Amount", cashMove.Amount }
            });
        }
        return new Dictionary<string, object>() {
            {
                "OrdersDetails",
                new Dictionary<string, object>() {
                    { "Quantity", orders.Count },
                    { "Amount", orders.Sum(o => o.AmountTotal) }
                }
            },
            { "OpeningNotes", this.OpeningNotes },
            {
                "DefaultCashDetails",
                defaultCashPaymentMethod != null
                    ? new Dictionary<string, object>() {
                        { "Name", defaultCashPaymentMethod.Name },
                        {
                            "Amount",
                            this.CashRegisterBalanceStart
                                + totalDefaultCashPaymentAmount
                                + this.Sudo().StatementLineIds.Sum(sl => sl.Amount)
                        },
                        { "Opening", this.CashRegisterBalanceStart },
                        { "PaymentAmount", totalDefaultCashPaymentAmount },
                        { "Moves", cashInOutList },
                        { "Id", defaultCashPaymentMethod.Id }
                    }
                    : null
            },
            {
                "OtherPaymentMethods",
                otherPaymentMethodIds.Select(pm => new Dictionary<string, object>() {
                    { "Name", pm.Name },
                    { "Amount", orders.SelectMany(o => o.PaymentIds).Where(p => p.PaymentMethodId == pm).Sum(p => p.Amount) },
                    { "Number", orders.SelectMany(o => o.PaymentIds).Where(p => p.PaymentMethodId == pm).Count() },
                    { "Id", pm.Id },
                    { "Type", pm.Type }
                }).ToList()
            },
            { "IsManager", Env.User.IsInGroup("point_of_sale.group_pos_manager") },
            { "AmountAuthorizedDiff", this.ConfigId.AmountAuthorizedDiff ?? null }
        };
    }

    public void SetCashboxPos(int cashboxValue, string notes) {
        this.State = "Opened";
        this.OpeningNotes = notes;
        double difference = cashboxValue - this.CashRegisterBalanceStart;
        this._PostCashDetailsMessage("Opening", this.CashRegisterBalanceStart, difference, notes);
        this.CashRegisterBalanceStart = cashboxValue;
    }

    public Dictionary<string, object> ActionViewOrder() {
        return new Dictionary<string, object>() {
            { "Name", Env.Translate("Orders") },
            { "ResModel", "PosOrder" },
            { "ViewMode", "tree,form" },
            {
                "Views",
                new List<Tuple<int, string>>() {
                    new Tuple<int, string>(Env.Ref("point_of_sale.view_pos_order_tree_no_session_id").Id, "tree"),
                    new Tuple<int, string>(Env.Ref("point_of_sale.view_pos_pos_form").Id, "form")
                }
            },
            { "Type", "ir.actions.act_window" },
            { "Domain", new[] { new Tuple<string, object>("SessionId", this.Id) } }
        };
    }

    public Dictionary<string, object> OpenFrontendCb() {
        if (!this.Ids.Any()) {
            return new Dictionary<string, object>();
        }
        return this.ConfigId.OpenUi();
    }

    public Dictionary<string, object> ShowCashRegister() {
        return new Dictionary<string, object>() {
            { "Name", Env.Translate("Cash register") },
            { "Type", "ir.actions.act_window" },
            { "ResModel", "Account.BankStatementLine" },
            { "ViewMode", "tree,kanban" },
            { "Domain", new[] { new Tuple<string, object>("Id", "in", this.StatementLineIds.Select(sl => sl.Id).ToList()) } }
        };
    }

    public Dictionary<string, object> ShowJournalItems() {
        List<Account.Move> allRelatedMoves = GetRelatedAccountMoves();
        return new Dictionary<string, object>() {
            { "Name", Env.Translate("Journal Items") },
            { "Type", "ir.actions.act_window" },
            { "ResModel", "Account.MoveLine" },
            { "ViewMode", "tree" },
            { "ViewId", Env.Ref("account.view_move_line_tree").Id },
            {
                "Domain",
                new[] { new Tuple<string, object>("Id", "in", allRelatedMoves.SelectMany(m => m.LineIds).Select(l => l.Id).ToList()) }
            },
            {
                "Context",
                new Dictionary<string, object>() {
                    { "journal_type", "general" },
                    { "search_default_group_by_move", 1 },
                    { "group_by", "move_id" },
                    { "search_default_posted", 1 }
                }
            }
        };
    }

    public Dictionary<string, object> ActionShowPaymentsList() {
        return new Dictionary<string, object>() {
            { "Name", Env.Translate("Payments") },
            { "Type", "ir.actions.act_window" },
            { "ResModel", "PosPayment" },
            { "ViewMode", "tree,form" },
            { "Domain", new[] { new Tuple<string, object>("SessionId", this.Id) } },
            { "Context", new Dictionary<string, object>() { { "search_default_group_by_payment_method", 1 } } }
        };
    }

    public void TryCashInOut(string type, double amount, string reason, Dictionary<string, object> extras) {
        int sign = type == "in" ? 1 : -1;
        List<PosSession> sessions = this.Where(s => s.CashJournalId != null).ToList();
        if (!sessions.Any()) {
            throw new UserError(Env.Translate("There is no cash payment method for this PoS Session"));
        }
        this.Env.Model<Account.BankStatementLine>().Create(sessions.Select(session => new Dictionary<string, object>() {
            { "PosSessionId", session.Id },
            { "JournalId", session.CashJournalId.Id },
            { "Amount", sign * amount },
            { "Date", DateTime.Now },
            { "PaymentRef", string.Join("-", new string[] { session.Name, extras["translatedType"].ToString(), reason }) }
        }).ToList());
    }

    private void _PostCashDetailsMessage(string state, double expected, double difference, string notes) {
        string message = (state + " difference: " + this.CurrencyId.Format(difference) + '\n' +
           state + " expected: " + this.CurrencyId.Format(expected) + '\n' +
           state + " counted: " + this.CurrencyId.Format(expected + difference) + '\n');
        if (notes != null) {
            message += notes;
        }
        if (message != null) {
            this.MessagePost(new Dictionary<string, object>() {
                { "Body", message }
            });
        }
    }

    private List<PosOrder> GetClosedOrders() {
        return this.OrderIds.Where(o => o.State != "Draft" && o.State != "Cancel").ToList();
    }

    private Dictionary<string, object> _GetInvoiceTotalList() {
        List<Dictionary<string, object>> invoiceList = new List<Dictionary<string, object>>();
        foreach (PosOrder order in this.OrderIds.Where(o => o.IsInvoiced).ToList()) {
            invoiceList.Add(new Dictionary<string, object>() {
                { "Total", order.AccountMove.AmountTotal },
                { "Name", order.AccountMove.Name },
                { "OrderRef", order.PosReference }
            });
        }
        return invoiceList;
    }

    private double _GetTotalInvoice() {
        double amount = 0;
        foreach (PosOrder order in this.OrderIds.Where(o => o.IsInvoiced).ToList()) {
            amount += order.AmountPaid;
        }
        return amount;
    }

    private void LogPartnerMessage(int partnerId, string action, string messageType) {
        string body = messageType switch {
            "ACTION_CANCELLED" => $"Action cancelled ({action})",
            "CASH_DRAWER_ACTION" => $"Cash drawer opened ({action})",
            _ => null
        };
        this.MessagePost(new Dictionary<string, object>() {
            { "Body", body },
            { "AuthorId", partnerId }
        });
    }

    private bool _PosHasValidProduct() {
        return Env.Model<Product.Product>().Sudo().SearchCount(new[] {
            new Tuple<string, object>("AvailableInPos", true),
            new Tuple<string, object>("ListPrice", ">=", 0),
            new Tuple<string, object>("Id", "not in", Env.Model<PosConfig>()._GetSpecialProducts().Select(p => p.Id).ToList()),
            new Tuple<string, object>("Active", "=", false),
            new Tuple<string, object>("Active", "=", true),
        }, 1) > 0;
    }

    private void CheckIfNoDraftOrders() {
        List<PosOrder> draftOrders = this.OrderIds.Where(order => order.State == "Draft").ToList();
        if (draftOrders.Any()) {
            throw new UserError(Env.Translate(
                "There are still orders in draft state in the session. "
                + "Pay or cancel the following orders to validate the session:\n%s",
                string.Join(", ", draftOrders.Select(o => o.Name).ToList())
            ));
        }
    }

    private void CheckClosingSession() {
        if (this.OrderIds.Any(o => o.State == "Draft")) {
            throw new UserError(Env.Translate("You cannot close the POS when orders are still in draft"));
        }
        if (this.State == "Closed") {
            throw new UserError(Env.Translate("The session has been already closed by another User. "
                + "All sales completed in the meantime have been saved in a "
                + "Rescue Session, which can be reviewed anytime and posted "
                + "to Accounting from Point of Sale's dashboard."));
        }
    }

    private void CheckInvoicesArePosted() {
        List<Account.Move> unpostedInvoices = GetClosedOrders().Sudo().WithCompany(this.CompanyId).AccountMove.Where(x => x.State != "Posted").ToList();
        if (unpostedInvoices.Any()) {
            throw new UserError(Env.Translate(
                "You cannot close the POS when invoices are not posted.\nInvoices: %s",
                string.Join("\n", unpostedInvoices.Select(invoice => $"{invoice.Name} - {invoice.State}").ToList())
            ));
        }
    }

    private void CreatePickingAtEndOfSession() {
        Dictionary<int, List<PosOrderLine>> linesGroupedByDestLocation = new Dictionary<int, List<PosOrderLine>>();
        Stock.PickingType pickingType = this.ConfigId.PickingTypeId;
        int sessionDestinationId = pickingType != null && pickingType.DefaultLocationDestId != null
            ? pickingType.DefaultLocationDestId.Id
            : Env.Model<Stock.Warehouse>()._GetPartnerLocations().FirstOrDefault().Id;
        foreach (PosOrder order in GetClosedOrders()) {
            if (order.CompanyId.AngloSaxonAccounting && order.IsInvoiced || order.ShippingDate != null) {
                continue;
            }
            int destinationId = order.PartnerId.PropertyStockCustomer.Id ?? sessionDestinationId;
            if (linesGroupedByDestLocation.ContainsKey(destinationId)) {
                linesGroupedByDestLocation[destinationId].AddRange(order.Lines);
            } else {
                linesGroupedByDestLocation.Add(destinationId, order.Lines);
            }
        }
        foreach (KeyValuePair<int, List<PosOrderLine>> entry in linesGroupedByDestLocation) {
            List<Stock.Picking> pickings = Env.Model<Stock.Picking>()._CreatePickingFromPosOrderLines(entry.Key, entry.Value, pickingType);
            pickings.ForEach(p => p.Write(new Dictionary<string, object>() {
                { "PosSessionId", this.Id },
                { "Origin", this.Name }
            }));
        }
    }

    private Dictionary<string, object> _GetBalancingLineVals(double imbalanceAmount, Account.Move move, Account.Account balancingAccount) {
        Dictionary<string, object> partialVals = new Dictionary<string, object>() {
            { "Name", Env.Translate("Difference at closing PoS session") },
            { "AccountId", balancingAccount.Id },
            { "MoveId", move.Id },
            { "PartnerId", false }
        };
        double imbalanceAmountSession = 0;
        if (!this.IsInCompanyCurrency) {
            imbalanceAmountSession = this.CompanyId.CurrencyId._Convert(imbalanceAmount, this.CurrencyId, this.CompanyId, DateTime.Now);
        }
        return _CreditAmounts(partialVals, imbalanceAmountSession, imbalanceAmount);
    }

    private Account.Account _GetBalancingAccount() {
        Res.Partner propertyAccount = Env.Model<Ir.Property>()._Get("property_account_receivable_id", "res.partner");
        return this.CompanyId.AccountDefaultPosReceivableAccountId ?? propertyAccount ?? Env.Model<Account.Account>();
    }

    private Dictionary<string, object> _CreateAccountMove(Account.Account balancingAccount = null, double amountToBalance = 0, Dictionary<int, double> bankPaymentMethodDiffs = null) {
        if (bankPaymentMethodDiffs == null) {
            bankPaymentMethodDiffs = new Dictionary<int, double>();
        }
        Account.Move accountMove = Env.Model<Account.Move>().Create(new Dictionary<string, object>() {
            { "JournalId", this.ConfigId.JournalId.Id },
            { "Date", DateTime.Now },
            { "Ref", this.Name }
        });
        this.Write(new Dictionary<string, object>() {
            { "MoveId", accountMove.Id }
        });
        Dictionary<string, object> data = new Dictionary<string, object>() {
            { "bank_payment_method_diffs", bankPaymentMethodDiffs ?? new Dictionary<int, double>() }
        };
        data = _AccumulateAmounts(data);
        data = _CreateNonReconciliableMoveLines(data);
        data = _CreateBankPaymentMoves(data);
        data = _CreatePayLaterReceivableLines(data);
        data = _CreateCashStatementLinesAndCashMoveLines(data);
        data = _CreateInvoiceReceivableLines(data);
        data = _CreateStockOutputLines(data);
        if (balancingAccount != null && amountToBalance != 0) {
            data = _CreateBalancingLine(data, balancingAccount, amountToBalance);
        }
        return data;
    }

    private Dictionary<string, object> _AccumulateAmounts(Dictionary<string, object> data) {
        Dictionary<string, object> amounts = () => new Dictionary<string, object>() { { "Amount", 0.0 }, { "AmountConverted", 0.0 } };
        Dictionary<string, object> taxAmounts = () => new Dictionary<string, object>() { { "Amount", 0.0 }, { "AmountConverted", 0.0 }, { "BaseAmount", 0.0 }, { "BaseAmountConverted", 0.0 } };
        Dictionary<PosPayment, Dictionary<string, object>> splitReceivablesBank = new Dictionary<PosPayment, Dictionary<string, object>>();
        Dictionary<PosPaymentMethod, Dictionary<string, object>> combineReceivablesBank = new Dictionary<PosPaymentMethod, Dictionary<string, object>>();
        Dictionary<PosPayment, Dictionary<string, object>> splitReceivablesCash = new Dictionary<PosPayment, Dictionary<string, object>>();
        Dictionary<PosPaymentMethod, Dictionary<string, object>> combineReceivablesCash = new Dictionary<PosPaymentMethod, Dictionary<string, object>>();
        Dictionary<PosPayment, Dictionary<string, object>> splitReceivablesPayLater = new Dictionary<PosPayment, Dictionary<string, object>>();
        Dictionary<PosPaymentMethod, Dictionary<string, object>> combineReceivablesPayLater = new Dictionary<PosPaymentMethod, Dictionary<string, object>>();
        Dictionary<PosPaymentMethod, Dictionary<string, object>> combineInvoiceReceivables = new Dictionary<PosPaymentMethod, Dictionary<string, object>>();
        Dictionary<PosPayment, Dictionary<string, object>> splitInvoiceReceivables = new Dictionary<PosPayment, Dictionary<string, object>>();
        Dictionary<Tuple<int, int, Tuple<int>, Tuple<int>>, Dictionary<string, object>> sales = new Dictionary<Tuple<int, int, Tuple<int>, Tuple<int>>, Dictionary<string, object>>();
        Dictionary<Tuple<int, int, Tuple<int>>, Dictionary<string, object>> taxes = new Dictionary<Tuple<int, int, Tuple<int>>, Dictionary<string, object>>();
        Dictionary<Account.Account, Dictionary<string, object>> stockExpense = new Dictionary<Account.Account, Dictionary<string, object>>();
        Dictionary<Account.Account, Dictionary<string, object>> stockReturn = new Dictionary<Account.Account, Dictionary<string, object>>();
        Dictionary<Account.Account, Dictionary<string, object>> stockOutput = new Dictionary<Account.Account, Dictionary<string, object>>();
        Dictionary<string, object> roundingDifference = new Dictionary<string, object>() { { "Amount", 0.0 }, { "AmountConverted", 0.0 } };
        Dictionary<PosPaymentMethod, Account.MoveLine> combineInvPaymentReceivableLines = new Dictionary<PosPaymentMethod, Account.MoveLine>();
        Dictionary<PosPayment, Account.MoveLine> splitInvPaymentReceivableLines = new Dictionary<PosPayment, Account.MoveLine>();
        Account.Account posReceivableAccount = this.CompanyId.AccountDefaultPosReceivableAccountId;
        double currencyRounding = this.CurrencyId.Rounding;
        List<PosOrder> closedOrders = GetClosedOrders();
        foreach (PosOrder order in closedOrders) {
            bool orderIsInvoiced = order.IsInvoiced;
            foreach (PosPayment payment in order.PaymentIds) {
                double amount = payment.Amount;
                if (Math.Abs(amount) < currencyRounding) {
                    continue;
                }
                DateTime date = payment.PaymentDate;
                PosPaymentMethod paymentMethod = payment.PaymentMethodId;
                bool isSplitPayment = payment.PaymentMethodId.SplitTransactions;
                string paymentType = paymentMethod.Type;
                if (paymentType != "PayLater") {
                    if (isSplitPayment && paymentType == "Cash") {
                        splitReceivablesCash[payment] = _UpdateAmounts(splitReceivablesCash.ContainsKey(payment) ? splitReceivablesCash[payment] : new Dictionary<string, object>(), new Dictionary<string, object>() { { "Amount", amount } }, date);
                    } else if (!isSplitPayment && paymentType == "Cash") {
                        combineReceivablesCash[paymentMethod] = _UpdateAmounts(combineReceivablesCash.ContainsKey(paymentMethod) ? combineReceivablesCash[paymentMethod] : new Dictionary<string, object>(), new Dictionary<string, object>() { { "Amount", amount } }, date);
                    } else if (isSplitPayment && paymentType == "Bank") {
                        splitReceivablesBank[payment] = _UpdateAmounts(splitReceivablesBank.ContainsKey(payment) ? splitReceivablesBank[payment] : new Dictionary<string, object>(), new Dictionary<string, object>() { { "Amount", amount } }, date);
                    } else if (!isSplitPayment && paymentType == "Bank") {
                        combineReceivablesBank[paymentMethod] = _UpdateAmounts(combineReceivablesBank.ContainsKey(paymentMethod) ? combineReceivablesBank[paymentMethod] : new Dictionary<string, object>(), new Dictionary<string, object>() { { "Amount", amount } }, date);
                    }
                    if (orderIsInvoiced) {
                        if (isSplitPayment) {
                            splitInvPaymentReceivableLines[payment] = payment.AccountMoveId.LineIds.Where(line => line.AccountId == posReceivableAccount).ToList();
                            splitInvoiceReceivables[payment] = _UpdateAmounts(splitInvoiceReceivables.ContainsKey(payment) ? splitInvoiceReceivables[payment] : new Dictionary<string, object>(), new Dictionary<string, object>() { { "Amount", payment.Amount } }, order.DateOrder);
                        } else {
                            combineInvPaymentReceivableLines[paymentMethod] = payment.Account