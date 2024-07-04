C#
public partial class PosSession {

    public virtual PosSession LoadPosDataModels(long configId) {
        var data = base.LoadPosDataModels(configId);
        var config = Env.Get<PosConfig>().Browse(configId);
        if (config.ModulePosHr) {
            data.Add("hr.employee");
        }
        return data;
    }
}
