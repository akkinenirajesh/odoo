csharp
public partial class PaymentToken {
    public string GetRazorpayLimitExceedWarning(decimal amount, Currency currencyId) {
        if (amount == 0 || this.ProviderCode != "razorpay") {
            return "";
        }

        PaymentTransaction primaryTransaction = Env.Search<PaymentTransaction>(t => t.TokenId == this.Id && !t.Operation.IsIn("offline", "online_token")).FirstOrDefault();
        if (primaryTransaction != null) {
            decimal mandateMaxAmount = primaryTransaction.GetRazorpayMandateMaxAmount();
            return GetLimitExceedWarningMessage(amount, currencyId, mandateMaxAmount);
        } else {
            PaymentMethod primaryPaymentMethod = this.PaymentMethodId.PrimaryPaymentMethodId ?? this.PaymentMethodId;
            decimal mandateMaxAmountInr = Const.MandateMaxAmount.GetValueOrDefault(primaryPaymentMethod.Code, Const.MandateMaxAmount["card"]);
            decimal mandateMaxAmount = PaymentTransaction.ConvertInrToCurrency(mandateMaxAmountInr, currencyId);
            return GetLimitExceedWarningMessage(amount, currencyId, mandateMaxAmount);
        }
    }

    private string GetLimitExceedWarningMessage(decimal amount, Currency currencyId, decimal mandateMaxAmount) {
        if (amount > mandateMaxAmount) {
            return $"You can not pay amounts greater than {currencyId.Symbol} {mandateMaxAmount:N0} with this payment method";
        }
        return "";
    }
}
