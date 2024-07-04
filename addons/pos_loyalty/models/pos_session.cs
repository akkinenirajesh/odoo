C#
public partial class PosSession {
    public void _LoadPosDataModels(PosConfig configId) {
        var data = Env.Call("super", "_LoadPosDataModels", configId);
        data.AddRange(new string[] { "Loyalty.Program", "Loyalty.Rule", "Loyalty.Reward", "Loyalty.Card" });
        return data;
    }
}
