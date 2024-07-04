csharp
public partial class PosSession
{
    public virtual int VivaWalletTerminalId { get; set; }

    public virtual List<PosPayment> LoaderParamsPosPaymentMethod()
    {
        var result = Env.CallMethod<List<PosPayment>>("PosSession", "_loaderParamsPosPaymentMethod");
        result.ForEach(x => x.VivaWalletTerminalId = VivaWalletTerminalId);
        return result;
    }
}
