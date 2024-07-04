csharp
public partial class UtmCampaign {
    public void ComputeName() {
        var newNames = Env.Mixin.GetUniqueNames("Utm.UtmCampaign", this.Title);
        this.Name = newNames[0];
    }

    public void Create(List<Dictionary<string, object>> valsList) {
        foreach (var vals in valsList) {
            if (!vals.ContainsKey("Title") && vals.ContainsKey("Name")) {
                vals["Title"] = vals["Name"];
            }
        }
        var newNames = Env.Mixin.GetUniqueNames("Utm.UtmCampaign", valsList.Select(x => (string)x["Name"]).ToList());
        for (int i = 0; i < valsList.Count; i++) {
            if (newNames[i] != null) {
                valsList[i]["Name"] = newNames[i];
            }
        }
        base.Create(valsList);
    }

    public List<Utm.Stage> GroupExpandStageIds(List<Utm.Stage> stages, List<object> domain) {
        var stageIds = stages.Search(new List<object>(), new Dictionary<string, object>() { { "Order", stages.Order } }, Env.SuperUserId);
        return stages.Browse(stageIds);
    }
}
