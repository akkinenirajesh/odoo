csharp
public partial class KarmaRank
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeRankUsersCount()
    {
        var requestsData = Env.Core.User.ReadGroup(
            filter: u => u.Rank != null,
            groupBy: u => u.Rank,
            aggregates: new[] { Aggregate.Count() }
        );

        var requestsMappedData = requestsData.ToDictionary(
            g => g.Key.Id,
            g => g.Count
        );

        RankUsersCount = requestsMappedData.TryGetValue(Id, out var count) ? count : 0;
    }

    public override void OnCreate()
    {
        base.OnCreate();

        if (KarmaMin > 0)
        {
            var users = Env.Core.User.Search(u => u.Karma >= Math.Max(KarmaMin, 1));
            if (users.Any())
            {
                users.ForEach(u => u.RecomputeRank());
            }
        }
    }

    public override void OnWrite()
    {
        var previousKarmaMin = OriginalValues.KarmaMin;

        base.OnWrite();

        if (KarmaMin != previousKarmaMin)
        {
            var previousRanks = Env.Gamification.KarmaRank.Search().OrderByDescending(r => r.KarmaMin).Select(r => r.Id).ToList();
            var low = Math.Min(KarmaMin, previousKarmaMin);
            var high = Math.Max(KarmaMin, previousKarmaMin);

            var afterRanks = Env.Gamification.KarmaRank.Search().OrderByDescending(r => r.KarmaMin).Select(r => r.Id).ToList();

            IEnumerable<Core.User> users;
            if (!previousRanks.SequenceEqual(afterRanks))
            {
                users = Env.Core.User.Search(u => u.Karma >= Math.Max(low, 1));
            }
            else
            {
                users = Env.Core.User.Search(u => u.Karma >= Math.Max(low, 1) && u.Karma <= high);
            }

            users.ForEach(u => u.RecomputeRank());
        }
    }
}
