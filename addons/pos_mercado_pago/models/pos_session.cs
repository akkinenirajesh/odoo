csharp
public partial class PosSession
{
    public virtual object _LoaderParamsPosPaymentMethod()
    {
        var result = Env.Call("super", "_LoaderParamsPosPaymentMethod");
        var searchParams = (Dictionary<string, object>)result["search_params"];
        searchParams["fields"].Add("MpIdPointSmart");
        return result;
    }
}
