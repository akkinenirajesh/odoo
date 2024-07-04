csharp
public partial class WebsiteSlidesSurvey.Channel
{
    public void RemoveMembership(List<long> partnerIds)
    {
        if (this != null)
        {
            var removedChannelPartnerDomain = new List<List<object>>();
            foreach (var channel in this)
            {
                removedChannelPartnerDomain.Add(new List<object>() { "PartnerId", "in", partnerIds, "ChannelId", "=", channel.Id });
            }
            var slidePartnersSudo = Env.Model("slide.slide.partner").Sudo().Search(removedChannelPartnerDomain);
            slidePartnersSudo.UserInputIds.SlidePartnerId = false;
        }
        base.RemoveMembership(partnerIds);
    }

    public void ComputeMembersCertifiedCount()
    {
        var channelsCount = Env.Model("slide.channel.partner").Sudo().ReadGroup(
            new List<object>() { "ChannelId", "in", this.Ids, "SurveyCertificationSuccess", "=", true },
            new List<string>() { "ChannelId" },
            new List<string>() { "__count" }
        );
        var mappedData = new Dictionary<long, int>();
        foreach (var channel in channelsCount)
        {
            mappedData.Add((long)channel["ChannelId"], (int)channel["__count"]);
        }
        foreach (var channel in this)
        {
            channel.MembersCertifiedCount = mappedData.ContainsKey(channel.Id) ? mappedData[channel.Id] : 0;
        }
    }

    public void ActionRedirectToCertifiedMembers()
    {
        var action = ActionRedirectToMembers("certified");
        action["help"] = $"<p class=\"o_view_nocontent_smiling_face\">{Env.Translate("No Attendee passed this course certification yet!")}</p>";
        return action;
    }
}
