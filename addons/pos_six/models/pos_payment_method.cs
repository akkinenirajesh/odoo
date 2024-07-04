csharp
public partial class PosPaymentMethod 
{
    public List<string> GetPaymentTerminalSelection()
    {
        var result = Env.Call("pos.payment.method", "_get_payment_terminal_selection");
        result.Add("six");
        return result;
    }

    public List<string> LoadPosDataFields(int configId)
    {
        var params = Env.Call("pos.payment.method", "_load_pos_data_fields", configId);
        params.Add("SixTerminalIP");
        return params;
    }
}
