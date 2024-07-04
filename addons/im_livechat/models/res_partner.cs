csharp
using System;
using System.Linq;
using System.Collections.Generic;

public partial class Partner
{
    public string ComputeUserLivechatUsername()
    {
        return this.UserIds.FirstOrDefault()?.LivechatUsername;
    }

    public void SearchForChannelInviteToStore(Store store, DiscussChannel channel)
    {
        base.SearchForChannelInviteToStore(store, channel);
        if (channel.ChannelType != "livechat" || this == null)
        {
            return;
        }

        var langNameByCode = Env.Get<ResLang>().GetInstalled().ToDictionary(l => l.Code, l => l.Name);
        var inviteBySelfCountByPartnerId = Env.Get<DiscussChannelMember>()
            .ReadGroup(
                new[] { 
                    new[] { "CreateUid", "=", Env.User.Id }, 
                    new[] { "PartnerId", "in", new[] { this.Id } }
                },
                groupBy: new[] { "PartnerId" },
                aggregates: new[] { "__count" }
            )
            .ToDictionary(g => g.Key, g => g.Value);

        var activeLivechatPartners = Env.Get<ImLivechatChannel>()
            .Search(new object[0])
            .SelectMany(c => c.AvailableOperatorIds)
            .Select(o => o.PartnerId)
            .ToList();

        store.Add("Persona", new Dictionary<string, object>
        {
            { "invite_by_self_count", inviteBySelfCountByPartnerId.GetValueOrDefault(this, 0) },
            { "is_available", activeLivechatPartners.Contains(this) },
            { "lang_name", langNameByCode[this.Lang] },
            { "id", this.Id },
            { "type", "partner" }
        });
    }

    public Dictionary<long, Dictionary<string, object>> MailPartnerFormat(Dictionary<string, bool> fields = null)
    {
        var partnerFormat = base.MailPartnerFormat(fields);
        if (fields != null && fields.GetValueOrDefault("UserLivechatUsername", false))
        {
            if (!string.IsNullOrEmpty(this.UserLivechatUsername))
            {
                partnerFormat[this.Id]["user_livechat_username"] = this.UserLivechatUsername;
            }
            else
            {
                partnerFormat[this.Id]["name"] = this.Name;
            }
        }
        return partnerFormat;
    }
}
