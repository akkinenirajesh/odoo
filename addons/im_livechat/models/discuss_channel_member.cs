csharp
public partial class ChannelMember
{
    [AutoVacuum]
    public void GcUnpinLivechatSessions()
    {
        var members = Env.Query<ChannelMember>()
            .Where(m => m.IsPinned == true &&
                        m.LastSeenDt <= DateTime.Now.AddDays(-1) &&
                        m.ChannelId.ChannelType == "livechat")
            .ToList();

        var sessionsToBeUnpinned = members.Where(m => m.MessageUnreadCounter == 0).ToList();

        foreach (var member in sessionsToBeUnpinned)
        {
            member.UnpinDt = DateTime.Now;
        }

        Env.SaveChanges();

        var bus = Env.Get<Bus.Bus>();
        foreach (var member in sessionsToBeUnpinned)
        {
            bus.SendOne(member.PartnerId, "discuss.channel/unpin", new { id = member.ChannelId.Id });
        }
    }

    public void PartnerDataToStore(Store store, List<string> fields = null)
    {
        if (ChannelId.ChannelType == "livechat")
        {
            var data = new Dictionary<string, object>
            {
                ["active"] = PartnerId.Active,
                ["id"] = PartnerId.Id,
                ["type"] = "partner",
                ["is_public"] = PartnerId.IsPublic,
                ["is_bot"] = ChannelId.LivechatChannelId.RuleIds
                    .SelectMany(r => r.ChatbotScriptId.OperatorPartnerId)
                    .Any(id => id == PartnerId.Id)
            };

            if (!string.IsNullOrEmpty(PartnerId.UserLivechatUsername))
            {
                data["user_livechat_username"] = PartnerId.UserLivechatUsername;
            }
            else
            {
                data["name"] = PartnerId.Name;
            }

            if (!PartnerId.IsPublic && PartnerId.CountryId != null)
            {
                data["country"] = new
                {
                    code = PartnerId.CountryId.Code,
                    id = PartnerId.CountryId.Id,
                    name = PartnerId.CountryId.Name
                };
            }
            else
            {
                data["country"] = false;
            }

            store.Add("Persona", data);
        }
        else
        {
            base.PartnerDataToStore(store, fields);
        }
    }
}
