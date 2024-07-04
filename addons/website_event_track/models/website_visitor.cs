csharp
public partial class WebsiteVisitor
{
    public void ComputeEventTrackWishlistedIds()
    {
        var results = Env.Get("event.track.visitor").ReadGroup(
            new[] {
                new[] { "visitor_id", "in", this.Id },
                new[] { "is_wishlisted", "=", true }
            },
            new[] { "visitor_id" },
            new[] { "track_id:array_agg" });

        var trackIdsMap = results.ToDictionary(x => (int)x["visitor_id"], x => (List<int>)x["track_id"]);

        this.EventTrackWishlistedIds = trackIdsMap.GetValueOrDefault(this.Id, new List<int>());
        this.EventTrackWishlistedCount = this.EventTrackWishlistedIds.Count;
    }

    public List<int> SearchEventTrackWishlistedIds(string operator, List<int> operand)
    {
        if (operator == "not in")
        {
            throw new NotImplementedException("Unsupported 'Not In' operation on track wishlist visitors");
        }

        var trackVisitors = Env.Get("event.track.visitor").Search(
            new[] {
                new[] { "track_id", operator, operand },
                new[] { "is_wishlisted", "=", true }
            });

        return trackVisitors.Select(x => x.Id).ToList();
    }

    public List<int> InactiveVisitorsDomain()
    {
        var domain = base.InactiveVisitorsDomain();
        return new List<int>() {
            domain,
            new[] { "EventTrackVisitorIds", "=", false }
        };
    }

    public void MergeVisitor(int target)
    {
        this.EventTrackVisitorIds.ForEach(x => x.VisitorId = target);

        var trackVisitorWoPartner = this.EventTrackVisitorIds.Where(x => x.PartnerId == 0).ToList();

        if (trackVisitorWoPartner.Any())
        {
            trackVisitorWoPartner.ForEach(x => x.PartnerId = target);
        }

        base.MergeVisitor(target);
    }
}
