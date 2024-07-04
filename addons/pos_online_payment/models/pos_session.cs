csharp
public partial class PosSession {
    public virtual void _AccumulateAmounts(dynamic data) {
        data = Env.Call("super", "_AccumulateAmounts", data);
        var amounts = () => new { Amount = 0.0, AmountConverted = 0.0 };
        var splitReceivablesOnline = new Dictionary<dynamic, dynamic>();
        var currencyRounding = Env.Get("currency_id").CurrencyRounding;
        foreach (var order in _GetClosedOrders()) {
            foreach (var payment in order.PaymentIds) {
                var amount = payment.Amount;
                if (Tools.FloatIsZero(amount, currencyRounding)) {
                    continue;
                }
                var date = payment.PaymentDate;
                var paymentMethod = payment.PaymentMethodId;
                var paymentType = paymentMethod.Type;
                if (paymentType == "online") {
                    splitReceivablesOnline[payment] = _UpdateAmounts(splitReceivablesOnline[payment], new { Amount = amount }, date);
                }
            }
        }
        data.SplitReceivablesOnline = splitReceivablesOnline;
        return data;
    }
    public virtual void _CreateBankPaymentMoves(dynamic data) {
        data = Env.Call("super", "_CreateBankPaymentMoves", data);
        var splitReceivablesOnline = data.SplitReceivablesOnline;
        var moveLine = data.MoveLine;
        var onlinePaymentToReceivableLines = new Dictionary<dynamic, dynamic>();
        foreach (var payment in splitReceivablesOnline) {
            var splitReceivableLine = moveLine.Create(_GetSplitReceivableOpVals(payment, payment.Value.Amount, payment.Value.AmountConverted));
            var accountPayment = payment.Key.OnlineAccountPaymentId;
            var paymentReceivableLine = accountPayment.MoveId.LineIds.Where(line => line.AccountId == accountPayment.DestinationAccountId);
            onlinePaymentToReceivableLines[payment.Key] = splitReceivableLine | paymentReceivableLine;
        }
        data.OnlinePaymentToReceivableLines = onlinePaymentToReceivableLines;
        return data;
    }
    public virtual dynamic _GetSplitReceivableOpVals(dynamic payment, double amount, double amountConverted) {
        var partner = payment.OnlineAccountPaymentId.PartnerId;
        var accountingPartner = Env.Call("res.partner", "_FindAccountingPartner", partner);
        if (accountingPartner == null) {
            throw new UserError(_("The partner of the POS online payment (id=%d) could not be found", payment.Id));
        }
        var partialVals = new {
            AccountId = accountingPartner.PropertyAccountReceivableId.Id,
            MoveId = MoveId.Id,
            PartnerId = accountingPartner.Id,
            Name = $"{Name} - {payment.PaymentMethodId.Name} ({payment.OnlineAccountPaymentId.PaymentMethodLineId.PaymentProviderId.Name})"
        };
        return _DebitAmounts(partialVals, amount, amountConverted);
    }
    public virtual void _ReconcileAccountMoveLines(dynamic data) {
        data = Env.Call("super", "_ReconcileAccountMoveLines", data);
        var onlinePaymentToReceivableLines = data.OnlinePaymentToReceivableLines;
        foreach (var payment in onlinePaymentToReceivableLines) {
            if (payment.Key.OnlineAccountPaymentId.PartnerId.PropertyAccountReceivableId.Reconcile) {
                payment.Value.Where(line => !line.Reconciled).Reconcile();
            }
        }
        return data;
    }
    private dynamic _GetClosedOrders() {
        // TODO: Implement this method
        return null;
    }
    private dynamic _UpdateAmounts(dynamic amounts, dynamic amountData, dynamic date) {
        // TODO: Implement this method
        return null;
    }
    private dynamic _DebitAmounts(dynamic partialVals, double amount, double amountConverted) {
        // TODO: Implement this method
        return null;
    }
}
