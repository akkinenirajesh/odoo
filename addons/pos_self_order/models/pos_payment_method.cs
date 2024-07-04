csharp
public partial class PosPaymentMethod {
    public void PaymentRequestFromKiosk(PosOrder order) {
        // this.Env.
        // order.
    }

    public List<object> LoadPosSelfDataDomain(object data) {
        if ((string)data["pos.config"]["data"][0]["self_ordering_mode"] == "kiosk") {
            return new List<object> {
                new {
                    UsePaymentTerminal = new List<string> { "adyen", "stripe" } 
                }
            };
        } else {
            return new List<object> {
                new { Id = false }
            };
        }
    }
}
