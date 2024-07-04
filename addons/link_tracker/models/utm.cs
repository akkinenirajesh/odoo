C#
public partial class LinkTracker.UtmCampaign {
    public int ClickCount { get; set; }

    public void ComputeClicksCount()
    {
        var clickData = Env.Get("link.tracker.click").ReadGroup(
            new List<object> { new { campaign_id = this.Id } }, 
            new List<string> { "campaign_id" }, 
            new List<string> { "__count" });

        var mappedData = clickData.ToDictionary(x => (int)x["campaign_id"], x => (int)x["__count"]);

        ClickCount = mappedData.ContainsKey(this.Id) ? mappedData[this.Id] : 0;
    }
}
