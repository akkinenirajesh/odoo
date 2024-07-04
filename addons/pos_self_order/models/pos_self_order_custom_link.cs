csharp
public partial class PosSelfOrderCustomLink {
    public virtual string Name { get; set; }
    public virtual string Url { get; set; }
    public virtual ICollection<PosConfig> PosConfigIds { get; set; }
    public virtual PosSelfOrderCustomLinkStyle Style { get; set; }
    public virtual string LinkHtml { get; set; }
    public virtual int Sequence { get; set; }

    public void ComputeLinkHtml() {
        if (this.Name != null) {
            this.LinkHtml = $"<a class=\"btn btn-{this.Style.Value} w-100\">{this.Name}</a>";
        }
    }

    public static Domain LoadPosSelfDataDomain(Dictionary<string, object> data) {
        var posConfigData = (Dictionary<string, object>)data["pos.config"]["data"][0];
        var posConfigId = (int)posConfigData["id"];
        return new Domain("PosConfigIds", "in", new List<int> { posConfigId });
    }

    public static List<string> LoadPosSelfDataFields(int configId) {
        return new List<string> { "Name", "Url", "Style", "LinkHtml", "Sequence" };
    }
}
