csharp
public partial class SlideChannelPartner
{
    public override string ToString()
    {
        return $"{Partner} - {Channel}";
    }

    public void Populate()
    {
        var random = new Random("slidechannelpartners");
        var partners = Env.Query<Core.Partner>().Where(p => !p.IsCompany).ToList();
        var channels = Env.Query<Website.SlideChannel>().ToList();

        var attendeesPartnerIds = partners.SelectMany(_ => channels.Select(c => c.Id)).ToList();
        random.Shuffle(attendeesPartnerIds);

        var coursesWeights = Enumerable.Range(1, channels.Count).Select(i => 1.0 / i).ToList();

        var partnersChannelIds = new Dictionary<int, HashSet<int>>();

        foreach (var (_, partnerId) in attendeesPartnerIds.Select((id, index) => (index, id)))
        {
            if (!partnersChannelIds.TryGetValue(partnerId, out var partnerChannels))
            {
                partnerChannels = new HashSet<int>();
                partnersChannelIds[partnerId] = partnerChannels;
            }

            var remainingChannelIds = channels.Select(c => c.Id).Except(partnerChannels).ToList();
            var channelId = random.Choices(
                remainingChannelIds,
                coursesWeights.Take(remainingChannelIds.Count).ToList(),
                1
            ).First();

            partnerChannels.Add(channelId);

            var newPartner = new SlideChannelPartner
            {
                Partner = Env.Find<Core.Partner>(partnerId),
                Channel = Env.Find<Website.SlideChannel>(channelId),
                Active = random.Choice(new[] { true, true, true, true, false }),
                MemberStatus = random.Choice(new[]
                {
                    SlideChannelPartnerStatus.Invited,
                    SlideChannelPartnerStatus.Joined,
                    SlideChannelPartnerStatus.Joined,
                    SlideChannelPartnerStatus.Joined,
                    SlideChannelPartnerStatus.Joined,
                    SlideChannelPartnerStatus.Joined,
                    SlideChannelPartnerStatus.Ongoing,
                    SlideChannelPartnerStatus.Ongoing,
                    SlideChannelPartnerStatus.Ongoing,
                    SlideChannelPartnerStatus.Completed,
                    SlideChannelPartnerStatus.Completed
                })
            };

            Env.Add(newPartner);
        }

        Env.SaveChanges();
    }
}
