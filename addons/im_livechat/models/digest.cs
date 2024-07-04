csharp
public partial class Digest
{
    public void ComputeKpiLivechatRatingValue()
    {
        var channels = Env.Search<Discuss.Channel>(new[] { ("ChannelType", "=", "livechat") });
        var (start, end, _) = GetKpiComputeParameters();
        var domain = new[]
        {
            ("CreateDate", ">=", start),
            ("CreateDate", "<", end)
        };
        var ratings = channels.RatingGetGrades(domain);
        KpiLivechatRatingValue = ratings["great"] * 100 / ratings.Values.Sum();
    }

    public void ComputeKpiLivechatConversationsValue()
    {
        var (start, end, _) = GetKpiComputeParameters();
        KpiLivechatConversationsValue = Env.Search<Discuss.Channel>(new[]
        {
            ("ChannelType", "=", "livechat"),
            ("CreateDate", ">=", start),
            ("CreateDate", "<", end)
        }).Count();
    }

    public void ComputeKpiLivechatResponseValue()
    {
        var (start, end, _) = GetKpiComputeParameters();
        var responseTime = Env.ReadGroup<ImLivechat.ReportChannel>(new[]
        {
            ("StartDate", ">=", start),
            ("StartDate", "<", end)
        }, new string[] { }, new[] { "TimeToAnswer:avg" });
        KpiLivechatResponseValue = responseTime[0][0];
    }

    public Dictionary<string, string> ComputeKpisActions(Core.Company company, Core.User user)
    {
        var res = base.ComputeKpisActions(company, user);
        res["KpiLivechatRating"] = "im_livechat.rating_rating_action_livechat_report";
        res["KpiLivechatConversations"] = "im_livechat.im_livechat_report_operator_action";
        res["KpiLivechatResponse"] = "im_livechat.im_livechat_report_channel_time_to_answer_action";
        return res;
    }

    private (DateTime start, DateTime end, object __) GetKpiComputeParameters()
    {
        // Implementation of _get_kpi_compute_parameters
        throw new NotImplementedException();
    }
}
