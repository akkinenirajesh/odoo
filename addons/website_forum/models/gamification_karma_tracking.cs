csharp
public partial class WebsiteForum.KarmaTracking 
{
    public virtual List<WebsiteForum.KarmaTracking> _get_origin_selection_values()
    {
        var result = Env.GetModel("gamification.karma.tracking").Call<List<WebsiteForum.KarmaTracking>>("_get_origin_selection_values");
        result.Add(new WebsiteForum.KarmaTracking { Origin = "forum.post", Name = Env.GetModel("ir.model").Call<string>("_get", "forum.post")});
        return result;
    }
}
