C#
public partial class PosOrder {
    public virtual PosConfig ConfigId { get; set; }

    public virtual List<Payment> PaymentIds { get; set; }

    public virtual bool SetTipAfterPayment { get; set; }

    public virtual object ActionPosOrderPaid() {
        var res = Env.Call("PosOrder", "ActionPosOrderPaid", this);

        if (!this.SetTipAfterPayment) {
            var paymentLines = this.PaymentIds.Where(line => line.PaymentMethodId.UsePaymentTerminal == "adyen").ToList();

            foreach (var paymentLine in paymentLines) {
                Env.Call("Payment", "_AdyenCapture", paymentLine);
            }
        }

        return res;
    }
}
