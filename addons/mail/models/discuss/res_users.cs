csharp
public partial class Mail.ResUsers {
    public Mail.ResUsers Create(Dictionary<string, object> valsList) {
        var users = Env.Create(this, valsList);
        Env.Search<Discuss.Channel>("group_ids", "in", users.GroupsId.Select(g => g.Id)).SubscribeUsersAutomatically();
        return users;
    }

    public Mail.ResUsers Write(Dictionary<string, object> vals) {
        var res = Env.Write(this, vals);
        if (vals.ContainsKey("Active") && !(bool)vals["Active"]) {
            UnsubscribeFromNonPublicChannels();
        }
        var selGroups = vals.Where(k => k.Key.EndsWith("Id") && vals[k.Key] != null).Select(k => vals[k.Key]).ToList();
        if (vals.ContainsKey("GroupsId")) {
            // form: {'group_ids': [(3, 10), (3, 3), (4, 10), (4, 3)]} or {'group_ids': [(6, 0, [ids]}
            var userGroupIds = vals["GroupsId"].OfType<List<object>>().SelectMany(command => command.OfType<int>().Where(command => command == 4)).ToList();
            userGroupIds.AddRange(vals["GroupsId"].OfType<List<object>>().Where(command => command[0] == 6).SelectMany(command => command[2].OfType<int>()).ToList());
            Env.Search<Discuss.Channel>("group_ids", "in", userGroupIds).SubscribeUsersAutomatically();
        } else if (selGroups.Any()) {
            Env.Search<Discuss.Channel>("group_ids", "in", selGroups).SubscribeUsersAutomatically();
        }
        return res;
    }

    public void Unlink() {
        UnsubscribeFromNonPublicChannels();
        Env.Unlink(this);
    }

    private void UnsubscribeFromNonPublicChannels() {
        var domain = new Dictionary<string, object> { { "PartnerId", "in", this.PartnerId.Id } };
        var currentCm = Env.Search<Discuss.ChannelMember>(domain).WithUser(Env.User);
        currentCm.Where(cm => cm.ChannelId.ChannelType == "channel" && cm.ChannelId.GroupPublicId).Unlink();
    }

    public void InitStoreData(dynamic store) {
        Env.InitStoreData(this, store);
        var getParam = Env.GetParam;
        store.Add(new Dictionary<string, object> {
            { "HasGifPickerFeature", getParam("discuss.tenor_api_key") },
            { "HasMessageTranslationFeature", getParam("mail.google_translate_api_key") },
            { "ChannelTypesWithSeenInfos", Env.Search<Discuss.Channel>().GetTypesAllowingSeenInfos().OrderBy(x => x).ToList() }
        });
    }

    public void InitMessaging(dynamic store) {
        var channels = Env.Search<Discuss.Channel>().GetChannelsAsMember().WithUser(this);
        var domain = new Dictionary<string, object> { { "ChannelId", "in", channels.Id }, { "IsSelf", true } };
        var members = Env.Search<Discuss.ChannelMember>(domain);
        var membersWithUnread = members.Where(member => member.MessageUnreadCounter > 0).ToList();
        Env.InitMessaging(this, store);
        store.Add(new Dictionary<string, object> { { "InitChannelsUnreadCounter", membersWithUnread.Count } });
    }
}
