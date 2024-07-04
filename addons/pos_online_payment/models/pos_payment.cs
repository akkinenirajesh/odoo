csharp
public partial class PosPayment {
    public void Create(List<Dictionary<string, object>> valsList) {
        var onlineAccountPaymentsByPm = new Dictionary<int, HashSet<int>>();
        foreach (var vals in valsList) {
            var pmId = (int)vals["PaymentMethodId"];
            if (!onlineAccountPaymentsByPm.ContainsKey(pmId)) {
                onlineAccountPaymentsByPm[pmId] = new HashSet<int>();
            }
            onlineAccountPaymentsByPm[pmId].Add((int)vals["OnlineAccountPaymentId"]);
        }

        var opmsReadId = Env.SearchRead("Pos.PaymentMethod", new List<Dictionary<string, object>> {
            new Dictionary<string, object> { { "Field", "Id" }, { "Operator", "in" }, { "Value", onlineAccountPaymentsByPm.Keys.ToList() } },
            new Dictionary<string, object> { { "Field", "IsOnlinePayment" }, { "Operator", "=", }, { "Value", true } }
        }, new List<string> { "Id" });

        var opmsId = new HashSet<int>(opmsReadId.Select(opmReadId => (int)opmReadId["Id"]));
        var onlineAccountPaymentsToCheckId = new HashSet<int>();

        foreach (var (pmId, oapsId) in onlineAccountPaymentsByPm) {
            if (opmsId.Contains(pmId)) {
                if (oapsId.Contains(null)) {
                    throw new UserError("Cannot create a POS online payment without an accounting payment.");
                } else {
                    onlineAccountPaymentsToCheckId.UnionWith(oapsId);
                }
            } else if (oapsId.Any()) {
                throw new UserError("Cannot create a POS payment with a not online payment method and an online accounting payment.");
            }
        }

        if (onlineAccountPaymentsToCheckId.Any()) {
            var validOapAmount = Env.SearchCount("Account.AccountPayment", new List<Dictionary<string, object>> {
                new Dictionary<string, object> { { "Field", "Id" }, { "Operator", "in" }, { "Value", onlineAccountPaymentsToCheckId.ToList() } }
            });
            if (validOapAmount != onlineAccountPaymentsToCheckId.Count) {
                throw new UserError("Cannot create a POS online payment without an accounting payment.");
            }
        }

        var result = Env.Create("Pos.PosPayment", valsList);
        return result;
    }

    public void Write(Dictionary<string, object> vals) {
        if (vals.Keys.Any(key => new List<string> { "Amount", "PaymentDate", "PaymentMethodId", "OnlineAccountPaymentId", "PosOrderId" }.Contains(key)) && (this.OnlineAccountPaymentId != null || this.PaymentMethodId.IsOnlinePayment)) {
            throw new UserError("Cannot edit a POS online payment essential data.");
        }

        var result = Env.Write("Pos.PosPayment", new List<int> { this.Id }, vals);
    }

    public void CheckPaymentMethodId() {
        var bypassCheckPayments = this.Where(payment => payment.PaymentMethodId.IsOnlinePayment);
        if (bypassCheckPayments.Any(payment => payment.PaymentMethodId != payment.PosOrderId.OnlinePaymentMethodId)) {
            _logger.Warning("Allow to save a POS online payment with an unexpected online payment method");
        }

        var otherPayments = this.Except(bypassCheckPayments);
        otherPayments.CheckPaymentMethodId();
    }
}
