C#
public partial class Website {

    public Website(Env env) {
    }

    public ImLivechat.Channel ChannelId { get; set; }

    public Dictionary<string, object> GetLivechatChannelInfo() {
        if (this.ChannelId != null) {
            var livechatInfo = this.ChannelId.GetLivechatInfo();
            if (livechatInfo["available"] != null && (bool)livechatInfo["available"]) {
                var livechatRequestSession = GetLivechatRequestSession();
                if (livechatRequestSession != null) {
                    livechatInfo["options"]["forceThread"] = livechatRequestSession;
                }
            }
            return livechatInfo;
        }
        return new Dictionary<string, object>();
    }

    private Dictionary<string, object> GetLivechatRequestSession() {
        var visitor = Env.Get("website.visitor")._GetVisitorFromRequest();
        if (visitor != null) {
            var chatRequestChannel = Env.Get("discuss.channel").Search(new[] {
                new SearchCondition("channel_type", "=", "livechat"),
                new SearchCondition("livechat_visitor_id", "=", visitor.Id),
                new SearchCondition("livechat_channel_id", "=", this.ChannelId.Id),
                new SearchCondition("livechat_active", "=", true),
                new SearchCondition("has_message", "=", true)
            }, new SearchOrder[] { new SearchOrder("create_date", "desc") }, 1);
            if (chatRequestChannel != null) {
                if (visitor.PartnerId == null) {
                    var currentGuest = Env.Get("mail.guest")._GetGuestFromContext();
                    var channelGuestMember = chatRequestChannel.ChannelMemberIds.Where(m => m.GuestId != null).FirstOrDefault();
                    if (currentGuest != null && currentGuest != channelGuestMember.GuestId) {
                        chatRequestChannel.Write(new Dictionary<string, object>() {
                            { "channel_member_ids", new List<Command> {
                                Command.Unlink(channelGuestMember.Id),
                                Command.Create(new Dictionary<string, object>() {
                                    {"guest_id", currentGuest.Id},
                                    {"fold_state", "open"}
                                })
                            }}
                        });
                    }
                    if (currentGuest == null && channelGuestMember != null) {
                        channelGuestMember.GuestId._SetAuthCookie();
                        chatRequestChannel = chatRequestChannel.WithContext(new Dictionary<string, object>() {
                            { "guest", channelGuestMember.GuestId }
                        });
                    }
                }
                if (chatRequestChannel.IsMember) {
                    return new Dictionary<string, object>() {
                        { "id", chatRequestChannel.Id },
                        { "model", "discuss.channel" }
                    };
                }
            }
        }
        return null;
    }

    public List<Tuple<string, string, string>> GetSuggestedControllers() {
        var suggestedControllers = base.GetSuggestedControllers();
        suggestedControllers.Add(new Tuple<string, string, string>("Live Support", "/livechat", "website_livechat"));
        return suggestedControllers;
    }
}
