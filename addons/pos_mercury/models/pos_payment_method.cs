C#
public partial class PosPaymentMethod {
    public virtual void _LoadPosDataFields(int configId) {
        var paramsList = Env.Call<List<string>>("super", "_LoadPosDataFields", configId);
        paramsList.Add("PosMercuryConfigId");
        Env.Return(paramsList);
    }
}
