C#
public partial class PointOfSale.PosPayment {

    public void ComputeDisplayName() {
        if (this.Name != null) {
            this.DisplayName = $"{this.Name} {Env.FormatLang(this.Amount, this.CurrencyId)}";
        } else {
            this.DisplayName = Env.FormatLang(this.Amount, this.CurrencyId);
        }
    }

    public void CheckPaymentMethodId() {
        if (!this.SessionId.ConfigId.PaymentMethods.Contains(this.PaymentMethodId)) {
            throw new Exception("The payment method selected is not allowed in the config of the POS session.");
        }
    }

    public Account.AccountMove CreatePaymentMoves(bool isReverse = false) {
        var result = Env.AccountMove;
        if (this.PaymentMethodId.Type == "pay_later" || Env.FloatIsZero(this.Amount, this.CurrencyId.Rounding)) {
            return result;
        }
        var accountingPartner = Env.ResPartner.FindAccountingPartner(this.PartnerId);
        var posSession = this.PosOrderId.SessionId;
        var journal = posSession.ConfigId.JournalId;
        var paymentMove = Env.AccountMove.WithContext(defaultJournalId: journal.Id).Create(new Dictionary<string, object>() {
            { "JournalId", journal.Id },
            { "Date", Env.Date.ContextToday(this.PosOrderId, this.PosOrderId.DateOrder) },
            { "Ref", $"Invoice payment for {this.PosOrderId.Name} ({this.PosOrderId.AccountMove.Name}) using {this.PaymentMethodId.Name}" },
            { "PosPaymentIds", new List<PointOfSale.PosPayment>() { this } }
        });
        result |= paymentMove;
        this.AccountMoveId = paymentMove;
        var amounts = posSession.UpdateAmounts(new Dictionary<string, object>() { { "Amount", 0 }, { "AmountConverted", 0 } }, new Dictionary<string, object>() { { "Amount", this.Amount } }, this.PaymentDate);
        var creditLineVals = posSession.CreditAmounts(new Dictionary<string, object>() {
            { "AccountId", accountingPartner.WithCompany(this.PosOrderId.CompanyId).PropertyAccountReceivableId.Id },
            { "PartnerId", accountingPartner.Id },
            { "MoveId", paymentMove.Id }
        }, amounts["Amount"], amounts["AmountConverted"]);
        var isSplitTransaction = this.PaymentMethodId.SplitTransactions;
        var reversedMoveReceivableAccountId = isSplitTransaction && isReverse ? accountingPartner.WithCompany(this.PosOrderId.CompanyId).PropertyAccountReceivableId.Id : isReverse ? this.PaymentMethodId.ReceivableAccountId.Id ?? this.CompanyId.AccountDefaultPosReceivableAccountId.Id : this.CompanyId.AccountDefaultPosReceivableAccountId.Id;
        var debitLineVals = posSession.DebitAmounts(new Dictionary<string, object>() {
            { "AccountId", reversedMoveReceivableAccountId },
            { "MoveId", paymentMove.Id },
            { "PartnerId", isSplitTransaction && isReverse ? accountingPartner.Id : null }
        }, amounts["Amount"], amounts["AmountConverted"]);
        Env.AccountMoveLine.Create(new List<Dictionary<string, object>>() { creditLineVals, debitLineVals });
        paymentMove.Post();
        return result;
    }

    public void LoadPosDataDomain(Dictionary<string, object> data) {
        var posOrders = data["pos.order"]["data"] as List<object>;
        return new List<Dictionary<string, object>>() { new Dictionary<string, object>() { { "PosOrderId", new List<object>(posOrders.Select(o => (o as Dictionary<string, object>)["id"])) } } };
    }
}
