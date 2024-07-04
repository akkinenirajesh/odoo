csharp
public partial class WebsiteVisitor
{
    public WebsiteVisitor(Environment env)
    {
        Env = env;
    }

    public Environment Env { get; set; }

    public void ComputeLivechatOperatorId()
    {
        var results = Env.GetModel<DiscussChannel>().SearchRead(new[]
        {
            new Condition("LivechatVisitorId", this.Id, ConditionOperator.In),
            new Condition("LivechatActive", true, ConditionOperator.Equal)
        }, new[] { "LivechatVisitorId", "LivechatOperatorId" });
        var visitorOperatorMap = results.ToDictionary(
            result => int.Parse(result.GetValue("LivechatVisitorId").ToString()),
            result => int.Parse(result.GetValue("LivechatOperatorId").ToString())
        );
        LivechatOperatorId = visitorOperatorMap.GetValueOrDefault(this.Id, 0);
    }

    public void ComputeSessionCount()
    {
        var sessions = Env.GetModel<DiscussChannel>().Search(new Condition("LivechatVisitorId", this.Id, ConditionOperator.In));
        var sessionCount = new Dictionary<int, int>();
        foreach (var session in sessions.Where(c => c.MessageIds.Any()))
        {
            sessionCount[session.LivechatVisitorId.Id] += 1;
        }
        SessionCount = sessionCount.GetValueOrDefault(this.Id, 0);
    }

    public void ActionSendChatRequest()
    {
        // check if visitor is available
        var unavailableVisitorsCount = Env.GetModel<DiscussChannel>().SearchCount(new[]
        {
            new Condition("LivechatVisitorId", this.Id, ConditionOperator.In),
            new Condition("LivechatActive", true, ConditionOperator.Equal)
        });
        if (unavailableVisitorsCount > 0)
        {
            throw new UserError("Recipients are not available. Please refresh the page to get latest visitors status.");
        }

        // check if user is available as operator
        foreach (var website in this.WebsiteId)
        {
            if (website.ChannelId == null)
            {
                throw new UserError($"No Livechat Channel allows you to send a chat request for website {website.Name}.");
            }
        }

        this.WebsiteId.ChannelId.Write(new Dictionary<string, object> { { "UserIds", new[] { new Command(CommandOperation.Link, Env.User.Id) } } });

        // Create chat_requests and linked discuss_channels
        var discussChannelValsList = new List<Dictionary<string, object>>();
        foreach (var visitor in this)
        {
            var operatorId = Env.User.PartnerId.Id;
            var countryId = visitor.CountryId.Id;
            var visitorName = $"Visitor #{visitor.Id} ({visitor.CountryId.Name})" ?? $"Visitor #{visitor.Id}";
            var membersToAdd = new[] { new Command(CommandOperation.Link, operatorId) };
            if (visitor.PartnerId != null)
            {
                membersToAdd = membersToAdd.Append(new Command(CommandOperation.Link, visitor.PartnerId.Id)).ToArray();
            }
            discussChannelValsList.Add(new Dictionary<string, object>
            {
                { "ChannelPartnerIds", membersToAdd },
                { "LivechatChannelId", visitor.WebsiteId.ChannelId.Id },
                { "LivechatOperatorId", operatorId },
                { "ChannelType", "livechat" },
                { "CountryId", countryId },
                { "AnonymousName", visitorName },
                { "Name", $"{visitorName}, {Env.User.LivechatUsername ?? Env.User.Name}" },
                { "LivechatVisitorId", visitor.Id },
                { "LivechatActive", true },
            });
        }

        var discussChannels = Env.GetModel<DiscussChannel>().Create(discussChannelValsList);
        foreach (var channel in discussChannels)
        {
            if (channel.LivechatVisitorId.PartnerId == null)
            {
                // sudo: mail.guest - creating a guest in a dedicated channel created from livechat
                var guest = Env.GetModel<MailGuest>().WithUser(Env.Ref<ResUsers>("base.user_admin")).Create(
                    new Dictionary<string, object>
                    {
                        { "CountryId", countryId },
                        { "Lang", Env.GetLang(channel.Env).Code },
                        { "Name", $"Visitor #{channel.LivechatVisitorId.Id}" },
                        { "Timezone", visitor.Timezone },
                    });
                channel.AddMembers(new[] { new Command(CommandOperation.Link, guest.Id) }, postJoinedMessage: false);
            }
        }

        // Open empty chatter to allow the operator to start chatting with
        // the visitor. Also open the visitor's chat window in order for it
        // to be displayed at the next page load.
        var channelMembers = Env.GetModel<DiscussChannelMember>().WithUser(Env.Ref<ResUsers>("base.user_admin")).Search(new Condition("ChannelId", discussChannels.Ids, ConditionOperator.In));
        channelMembers.Write(new Dictionary<string, object> { { "FoldState", "open" } });

        var store = new Store(discussChannels);
        Env.GetModel<BusBus>().Sendone(Env.User.PartnerId, "website_livechat.send_chat_request", store.GetResult());
    }

    public void MergeVisitor(WebsiteVisitor target)
    {
        target.DiscussChannelIds.AddRange(this.DiscussChannelIds);
        this.DiscussChannelIds.ChannelPartnerIds = new[]
        {
            new Command(CommandOperation.Unlink, Env.Ref<ResPartner>("base.public_partner").Id),
            new Command(CommandOperation.Link, target.PartnerId.Id),
        };
    }

    public void UpsertVisitor(string accessToken, Dictionary<string, object> forceTrackValues = null)
    {
        var visitorId = 0;
        var upsert = "";
        // Call base implementation of _upsert_visitor, you might need to adapt the logic based on your implementation
        // ...

        if (upsert == "inserted")
        {
            var visitor = this.WithUser(Env.Ref<ResUsers>("base.user_admin")).Browse(visitorId);
            if (Env.Request.Cookies.TryGetValue("im_livechat_uuid", out var discussChannelUuid))
            {
                var discussChannel = Env.GetModel<DiscussChannel>().WithUser(Env.Ref<ResUsers>("base.user_admin")).Search(new Condition("Uuid", discussChannelUuid, ConditionOperator.Equal));
                discussChannel.Write(new Dictionary<string, object>
                {
                    { "LivechatVisitorId", visitor.Id },
                    { "AnonymousName", $"Visitor #{visitor.Id} ({visitor.CountryId.Name})" ?? $"Visitor #{visitor.Id}" },
                });
            }
        }
    }
}
