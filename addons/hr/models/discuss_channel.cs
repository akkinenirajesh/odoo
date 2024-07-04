csharp
public partial class DiscussChannel
{
    public void ConstraintSubscriptionDepartmentIdsChannel()
    {
        var failingChannels = Env.DiscussChannel.Search(new Domain()
            .And(x => x.ChannelType != "channel")
            .And(x => x.SubscriptionDepartmentIds.Any()));

        if (failingChannels.Any())
        {
            var channelNames = string.Join(", ", failingChannels.Select(ch => ch.Name));
            throw new ValidationException($"For {channelNames}, channel_type should be 'channel' to have the department auto-subscription.");
        }
    }

    public Dictionary<int, List<int>> SubscribeUsersAutomaticallyGetMembers()
    {
        var newMembers = base.SubscribeUsersAutomaticallyGetMembers();

        var channelId = this.Id;
        var existingMemberIds = new HashSet<int>(this.ChannelPartnerIds.Select(p => p.Id));
        var departmentMemberIds = this.SubscriptionDepartmentIds
            .SelectMany(d => d.MemberIds)
            .Where(m => m.UserId != null && m.UserId.PartnerId != null && m.UserId.PartnerId.Active)
            .Select(m => m.UserId.PartnerId.Id)
            .Except(existingMemberIds)
            .ToList();

        newMembers[channelId] = newMembers[channelId].Union(departmentMemberIds).ToList();

        return newMembers;
    }

    public override bool Write(Dictionary<string, object> vals)
    {
        var result = base.Write(vals);

        if (vals.ContainsKey(nameof(SubscriptionDepartmentIds)))
        {
            this.SubscribeUsersAutomatically();
        }

        return result;
    }
}
