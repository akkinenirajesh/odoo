csharp
public partial class PosPaymentMethod {
    public virtual string RazorpayTid { get; set; }
    public virtual Pos.RazorpayAllowedPaymentModes RazorpayAllowedPaymentModes { get; set; }
    public virtual string RazorpayUsername { get; set; }
    public virtual string RazorpayApiKey { get; set; }
    public virtual bool RazorpayTestMode { get; set; }

    public virtual object GetPaymentTerminalSelection() {
        // TODO: Implement this method.
        throw new NotImplementedException();
    }

    public virtual object RazorpayMakePaymentRequest(object data) {
        // TODO: Implement this method.
        throw new NotImplementedException();
    }

    public virtual object RazorpayFetchPaymentStatus(object data) {
        // TODO: Implement this method.
        throw new NotImplementedException();
    }

    public virtual object RazorpayCancelPaymentRequest(object data) {
        // TODO: Implement this method.
        throw new NotImplementedException();
    }

    public virtual void CheckRazorpayTerminal() {
        // TODO: Implement this method.
        throw new NotImplementedException();
    }
}
