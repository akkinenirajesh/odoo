csharp
public partial class ResPartner
{
    public void ComputeSlideChannelValues()
    {
        var data = Env.Get<SlideChannelPartner>().SearchReadGroup(
            new[] { ("PartnerId", "in", this.Ids), ("MemberStatus", "!=", "invited") },
            new[] { "PartnerId", "MemberStatus" },
            new[] { ("ChannelId", "array_agg") }
        );

        foreach (var partner in this)
        {
            var slideChannelIds = data.Where(d => d.PartnerId == partner.Id && (d.MemberStatus == "joined" || d.MemberStatus == "ongoing" || d.MemberStatus == "completed")).SelectMany(d => d.ChannelId).ToList();
            partner.SlideChannelIds = slideChannelIds;
            partner.SlideChannelCompletedIds = Env.Get<SlideChannel>().Browse(data.Where(d => d.PartnerId == partner.Id && d.MemberStatus == "completed").SelectMany(d => d.ChannelId).ToList());
            partner.SlideChannelCount = slideChannelIds.Count;
        }
    }

    public List<int> SearchSlideChannelCompletedIds(string operator, int value)
    {
        var cpDone = Env.Get<SlideChannelPartner>().Search(new[] { ("ChannelId", operator, value), ("MemberStatus", "=", "completed") });
        return cpDone.Select(cp => cp.PartnerId).ToList();
    }

    public List<int> SearchSlideChannelIds(string operator, int value)
    {
        var cpEnrolled = Env.Get<SlideChannelPartner>().Search(new[] { ("ChannelId", operator, value), ("MemberStatus", "!=", "invited") });
        return cpEnrolled.Select(cp => cp.PartnerId).ToList();
    }

    public void ComputeSlideChannelCompanyCount()
    {
        foreach (var partner in this)
        {
            if (partner.IsCompany)
            {
                partner.SlideChannelCompanyCount = Env.Get<SlideChannel>().SearchCount(new[] { ("PartnerIds", "in", partner.ChildIds.Ids) });
            }
            else
            {
                partner.SlideChannelCompanyCount = 0;
            }
        }
    }

    public dynamic ActionViewCourses()
    {
        var action = Env.Get<IrActionsActions>()._ForXmlId("website_slides.slide_channel_partner_action");
        action.DisplayName = "Courses";
        action.Domain = new[] { ("MemberStatus", "!=", "invited") };
        if (this.Count == 1 && this.IsCompany)
        {
            action.Domain = new[] { ("MemberStatus", "!=", "invited"), ("PartnerId", "in", this.ChildIds.Ids) };
        }
        else if (this.Count == 1)
        {
            action.Context = new { SearchDefaultPartnerId = this.Id };
        }
        else
        {
            action.Domain = new[] { ("MemberStatus", "!=", "invited"), ("PartnerId", "in", this.Ids) };
        }
        return action;
    }
}
