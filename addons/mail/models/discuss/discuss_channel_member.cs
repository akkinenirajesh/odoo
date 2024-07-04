csharp
public partial class DiscussChannelMember {
    public DiscussChannelMember() {
    }

    public void ContrainsNoPublicMember() {
        if (Env.Context.Get("mail_create_bypass_create_check") != this) {
            return;
        }
        if (Env.Get<ResPartner>(Partner).UserIds.Any(user => user.IsPublic())) {
            throw new ValidationError("Channel members cannot include public users.");
        }
    }

    public void ComputeIsSelf() {
        var currentPartner = Env.Get<ResPartner>().GetCurrentPersonaPartner();
        var currentGuest = Env.Get<ResPartner>().GetCurrentPersonaGuest();
        IsSelf = false;
        if (currentPartner != null && Partner == currentPartner) {
            IsSelf = true;
        }
        if (currentGuest != null && Guest == currentGuest) {
            IsSelf = true;
        }
    }

    public object[] SearchIsSelf(string operator, object operand) {
        var is_in = (operator == "=" && (bool)operand) || (operator == "!=" && !(bool)operand);
        var currentPartner = Env.Get<ResPartner>().GetCurrentPersonaPartner();
        var currentGuest = Env.Get<ResPartner>().GetCurrentPersonaGuest();
        if (is_in) {
            return new object[] {
                "|",
                new object[] { "Partner", "=", currentPartner.Id } ,
                new object[] { "Guest", "=", currentGuest.Id }
            };
        } else {
            return new object[] {
                "&",
                new object[] { "Partner", "!=", currentPartner.Id } ,
                new object[] { "Guest", "!=", currentGuest.Id }
            };
        }
    }

    public object[] SearchIsPinned(string operator, object operand) {
        if ((operator == "=" && (bool)operand) || (operator == "!=" && !(bool)operand)) {
            return new object[] {
                "|",
                new object[] { "UnpinDt", "=", null },
                new object[] {
                    "&",
                    new object[] { "LastInterestDt", ">=", UnpinDt } ,
                    new object[] { "Channel.LastInterestDt", ">=", UnpinDt }
                }
            };
        } else {
            return new object[] {
                "&",
                new object[] { "UnpinDt", "!=", null },
                new object[] {
                    "&",
                    new object[] { "LastInterestDt", "<", UnpinDt } ,
                    new object[] { "Channel.LastInterestDt", "<", UnpinDt }
                }
            };
        }
    }

    public void ComputeMessageUnread() {
        if (Id != 0) {
            Env.Get<MailMessage>().FlushModel();
            Env.FlushRecordset(new[] { "Channel", "NewMessageSeparator" });
            var unreadCounterByMember = Env.Cr.Execute("SELECT count(mail_message.id) AS count, discuss_channel_member.id FROM mail_message INNER JOIN discuss_channel_member ON discuss_channel_member.channel_id = mail_message.res_id WHERE mail_message.model = 'discuss.channel' AND mail_message.message_type NOT IN ('notification', 'user_notification') AND mail_message.id >= discuss_channel_member.new_message_separator AND discuss_channel_member.id IN @ids GROUP BY discuss_channel_member.id", new Dictionary<string, object>() { { "@ids", new[] { Id } } }).ToDictionary<IDictionary<string, object>, int, int>(r => (int)r["id"], r => (int)r["count"]);
            MessageUnreadCounter = unreadCounterByMember.GetValueOrDefault(Id);
        } else {
            MessageUnreadCounter = 0;
        }
    }

    public void ComputeDisplayName() {
        DisplayName = string.Format(
            "“{0}” in “{1}”",
            Partner != null ? Partner.Name : Guest.Name,
            Channel.DisplayName
        );
    }

    public void ComputeIsPinned() {
        IsPinned = (
            UnpinDt == null ||
            (
                LastInterestDt != null && LastInterestDt >= UnpinDt
            ) ||
            (
                Channel.LastInterestDt != null && Channel.LastInterestDt >= UnpinDt
            )
        );
    }

    public void Init() {
        Env.Cr.Execute("CREATE UNIQUE INDEX IF NOT EXISTS discuss_channel_member_partner_unique ON %s (channel_id, partner_id) WHERE partner_id IS NOT NULL", new object[] { _table });
        Env.Cr.Execute("CREATE UNIQUE INDEX IF NOT EXISTS discuss_channel_member_guest_unique ON %s (channel_id, guest_id) WHERE guest_id IS NOT NULL", new object[] { _table });
    }

    public DiscussChannelMember Create(Dictionary<string, object> vals) {
        if (Env.Context.Get("mail_create_bypass_create_check") != this) {
            return Env.Sudo().Create(vals);
        }
        if (!vals.ContainsKey("Channel")) {
            throw new UserError(
                string.Format(
                    "It appears you're trying to create a channel member, but it seems like you forgot to specify the related channel. "
                    "To move forward, please make sure to provide the necessary channel information."
                )
            );
        }
        var channel = Env.Get<DiscussChannel>(vals["Channel"]);
        if (channel.ChannelType == "chat" && channel.ChannelMemberIds.Count > 0) {
            throw new UserError(
                "Adding more members to this chat isn't possible; it's designed for just two people."
            );
        }
        var res = base.Create(vals);
        // help the ORM to detect changes
        res.Partner.InvalidateRecordset(new[] { "ChannelIds" });
        res.Guest.InvalidateRecordset(new[] { "ChannelIds" });
        return res;
    }

    public void Write(Dictionary<string, object> vals) {
        foreach (var field_name in new[] { "Channel", "Partner", "Guest" }) {
            if (vals.ContainsKey(field_name) && (int)vals[field_name] != this[field_name].Id) {
                throw new AccessError(string.Format("You can not write on {0}.", field_name));
            }
        }
        base.Write(vals);
    }

    public void Unlink() {
        // sudo: discuss.channel.rtc.session - cascade unlink of sessions for self member
        Env.Sudo().Get<DiscussChannelRtcSession>(RtcSessionIds).Unlink();  // ensure unlink overrides are applied
        base.Unlink();
    }

    public void NotifyTyping(bool isTyping) {
        foreach (var member in this) {
            var store = new Store(member);
            store.Add("ChannelMember", new Dictionary<string, object>() { { "id", member.Id }, { "isTyping", isTyping } });
            Env.Get<BusBus>().SendMany(new[] { new object[] { member.Channel, "mail.record/insert", store.GetResult() } });
        }
    }

    public void Unmute() {
        // Unmute notifications for the all the channel members whose mute date is passed.
        var members = Env.Get<DiscussChannelMember>().Search(new object[] { ("MuteUntilDt", "<=", DateTime.Now) });
        members.Write(new Dictionary<string, object>() { { "MuteUntilDt", null } });
        foreach (var member in members) {
            var channelData = new Dictionary<string, object>() {
                { "id", member.Channel.Id },
                { "model", "discuss.channel" },
                { "mute_until_dt", null },
            };
            Env.Get<BusBus>().SendMany(new[] { new object[] { member.Partner, "mail.record/insert", new Dictionary<string, object>() { { "Thread", channelData } } } });
        }
    }

    public void ToStore(Store store, Dictionary<string, object> fields = null, Dictionary<string, object> extraFields = null) {
        if (fields == null) {
            fields = new Dictionary<string, object>() {
                { "channel", new Dictionary<string, object>() },
                { "create_date", true },
                { "fetched_message_id", true },
                { "id", true },
                { "persona", new Dictionary<string, object>() },
                { "seen_message_id", true },
            };
        }
        if (extraFields != null) {
            foreach (var kvp in extraFields) {
                fields.Add(kvp.Key, kvp.Value);
            }
        }
        if (fields.ContainsKey("message_unread_counter")) {
            var busLastId = fields.GetValueOrDefault("message_unread_counter_bus_id");
            if (busLastId == null) {
                busLastId = Env.Get<BusBus>().Sudo()._bus_last_id();
            }
        }
        foreach (var member in this) {
            var data = new Dictionary<string, object>();
            if (fields.ContainsKey("id")) {
                data["id"] = member.Id;
            }
            if (fields.ContainsKey("channel")) {
                data["thread"] = member.Channel._channel_format(fields: fields.GetValueOrDefault("channel")).GetValueOrDefault(member.Channel);
            }
            if (fields.ContainsKey("create_date")) {
                data["create_date"] = DateTime.ToOADate(member.CreateDate);
            }
            if (fields.ContainsKey("persona")) {
                if (member.Partner != null) {
                    Env.Sudo()._partner_data_to_store(store, fields: fields.GetValueOrDefault("persona").GetValueOrDefault("partner"));
                    data["persona"] = new Dictionary<string, object>() { { "id", member.Partner.Id }, { "type", "partner" } };
                }
                if (member.Guest != null) {
                    store.Add("Persona", member.Guest.Sudo()._guest_format(fields: fields.GetValueOrDefault("persona").GetValueOrDefault("guest")).GetValueOrDefault(member.Guest));
                    data["persona"] = new Dictionary<string, object>() { { "id", member.Guest.Id }, { "type", "guest" } };
                }
            }
            if (fields.ContainsKey("custom_notifications")) {
                data["custom_notifications"] = member.CustomNotifications;
            }
            if (fields.ContainsKey("mute_until_dt")) {
                data["mute_until_dt"] = member.MuteUntilDt;
            }
            if (fields.ContainsKey("fetched_message_id")) {
                data["fetched_message_id"] = member.FetchedMessageId != null ? new Dictionary<string, object>() { { "id", member.FetchedMessageId.Id } } : null;
            }
            if (fields.ContainsKey("seen_message_id")) {
                data["seen_message_id"] = member.SeenMessageId != null ? new Dictionary<string, object>() { { "id", member.SeenMessageId.Id } } : null;
            }
            if (fields.ContainsKey("new_message_separator")) {
                data["new_message_separator"] = member.NewMessageSeparator;
            }
            if (fields.ContainsKey("message_unread_counter")) {
                data["message_unread_counter"] = member.MessageUnreadCounter;
                data["message_unread_counter_bus_id"] = busLastId;
            }
            if (fields.GetValueOrDefault("last_interest_dt") != null) {
                data["last_interest_dt"] = DateTime.ToOADate(member.LastInterestDt);
            }
            store.Add("ChannelMember", data);
        }
    }

    public void PartnerDataToStore(Store store, Dictionary<string, object> fields = null) {
        store.Add("Persona", Partner.MailPartnerFormat(fields: fields).GetValueOrDefault(Partner));
    }

    public void ChannelFold(string state, int stateCount) {
        if (FoldState == state) {
            return;
        }
        FoldState = state;
        Env.Get<BusBus>().SendOne(Partner ?? Guest, "discuss.Thread/fold_state", new Dictionary<string, object>() {
            { "foldStateCount", stateCount },
            { "id", Channel.Id },
            { "model", "discuss.channel" },
            { "fold_state", FoldState },
        });
    }

    public void RtcJoinCall(Store store = null, int[] checkRtcSessionIds = null) {
        checkRtcSessionIds = checkRtcSessionIds ?? new int[0];
        Channel.RtcCancelInvitations(memberIds: new[] { Id });
        RtcSessionIds.Unlink();
        var rtcSession = Env.Get<DiscussChannelRtcSession>().Create(new Dictionary<string, object>() { { "ChannelMemberId", Id } });
        var (currentRtcSessions, outdatedRtcSessions) = RtcSyncSessions(checkRtcSessionIds: checkRtcSessionIds);
        var iceServers = Env.Get<MailIceServer>().GetIceServers();
        JoinSfu(iceServers);
        if (store != null) {
            store.Add("Thread", new Dictionary<string, object>() {
                { "id", Channel.Id },
                { "model", "discuss.channel" },
                { "rtcSessions", new object[] {
                    ("ADD", currentRtcSessions.Select(s => new Dictionary<string, object>() { { "id", s.Id } }).ToArray()),
                    ("DELETE", outdatedRtcSessions.Select(s => new Dictionary<string, object>() { { "id", s.Id } }).ToArray()),
                } },
            });
            store.Add(currentRtcSessions);
            store.Add("Rtc", new Dictionary<string, object>() {
                { "iceServers", iceServers },
                { "selfSession", new Dictionary<string, object>() { { "id", rtcSession.Id } } },
                { "serverInfo", GetRtcServerInfo(rtcSession, iceServers) },
            });
        }
        if (Channel.RtcSessionIds.Count == 1 && Channel.ChannelType in new[] { "chat", "group" }) {
            Channel.MessagePost(body: string.Format("{0} started a live conference", Partner.Name ?? Guest.Name), messageType: "notification");
            RtcInviteMembers();
        }
    }

    public void JoinSfu(object[] iceServers = null) {
        if (Channel.RtcSessionIds.Count < 3) {
            if (Channel.SfuChannelUuid != null) {
                Channel.SfuChannelUuid = null;
                Channel.SfuServerUrl = null;
            }
            return;
        } else if (Channel.SfuChannelUuid != null && Channel.SfuServerUrl != null) {
            return;
        }
        var sfuServerUrl = Discuss.GetSfuUrl(Env);
        if (sfuServerUrl == null) {
            return;
        }
        var sfuLocalKey = Env.Get<IrConfigParameter>().Sudo().GetParam("mail.sfu_local_key");
        if (sfuLocalKey == null) {
            sfuLocalKey = Guid.NewGuid().ToString();
            Env.Get<IrConfigParameter>().Sudo().SetParam("mail.sfu_local_key", sfuLocalKey);
        }
        var jsonWebToken = Jwt.Sign(
            new Dictionary<string, object>() { { "iss", $"{GetBaseUrl()}:channel:{Channel.Id}" }, { "key", sfuLocalKey } },
            key: Discuss.GetSfuKey(Env),
            ttl: 30,
            algorithm: Jwt.Algorithm.HS256
        );
        try {
            var response = Requests.Get(
                sfuServerUrl + "/v1/channel",
                headers: new Dictionary<string, string>() { { "Authorization", "jwt " + jsonWebToken } },
                timeout: 3
            );
            response.RaiseForStatus();
        } catch (Requests.Exceptions.RequestException error) {
            _logger.Warning("Failed to obtain a channel from the SFU server, user will stay in p2p: {0}", error);
            return;
        }
        var responseDict = response.Json();
        Channel.SfuChannelUuid = responseDict["uuid"];
        Channel.SfuServerUrl = responseDict["url"];
        Env.Get<BusBus>().SendMany(
            Channel.RtcSessionIds.Select(session => new object[] {
                session.Guest ?? session.Partner,
                "discuss.channel.rtc.session/sfu_hot_swap",
                new Dictionary<string, object>() { { "serverInfo", GetRtcServerInfo(session, iceServers, key: sfuLocalKey) } },
            }).ToArray()
        );
    }

    public object GetRtcServerInfo(DiscussChannelRtcSession rtcSession, object[] iceServers = null, string key = null) {
        var sfuChannelUuid = Channel.SfuChannelUuid;
        var sfuServerUrl = Channel.SfuServerUrl;
        if (sfuChannelUuid == null || sfuServerUrl == null) {
            return null;
        }
        if (key == null) {
            key = Env.Get<IrConfigParameter>().Sudo().GetParam("mail.sfu_local_key");
        }
        var claims = new Dictionary<string, object>() {
            { "session_id", rtcSession.Id },
            { "ice_servers", iceServers },
        };
        var jsonWebToken = Jwt.Sign(claims, key: key, ttl: 60 * 60 * 8, algorithm: Jwt.Algorithm.HS256);  // 8 hours
        return new Dictionary<string, object>() { { "url", sfuServerUrl }, { "channelUUID", sfuChannelUuid }, { "jsonWebToken", jsonWebToken } };
    }

    public void RtcLeaveCall() {
        if (RtcSessionIds.Count > 0) {
            RtcSessionIds.Unlink();
        } else {
            Channel.RtcCancelInvitations(memberIds: new[] { Id });
        }
    }

    public (DiscussChannelRtcSession[], DiscussChannelRtcSession[]) RtcSyncSessions(int[] checkRtcSessionIds = null) {
        Channel.RtcSessionIds._delete_inactive_rtc_sessions();
        var checkRtcSessions = Env.Get<DiscussChannelRtcSession>().Browse(checkRtcSessionIds.Select(id => (int)id).ToArray());
        return (Channel.RtcSessionIds.ToArray(), (checkRtcSessions - Channel.RtcSessionIds).ToArray());
    }

    public DiscussChannelMember[] RtcInviteMembers(int[] memberIds = null) {
        var channelMemberDomain = new object[] {
            ("Channel", "=", Channel.Id),
            ("RtcInvitingSessionId", "=", null),
            ("RtcSessionIds", "=", null),
        };
        if (memberIds != null) {
            channelMemberDomain = new object[] {
                "&",
                channelMemberDomain,
                new object[] { ("id", "in", memberIds) }
            };
        }
        var invitationNotifications = new List<object[]>();
        var members = Env.Get<DiscussChannelMember>().Search(channelMemberDomain);
        foreach (var member in members) {
            member.RtcInvitingSessionId = RtcSessionIds.Id;
            var target = member.Partner ?? member.Guest;
            var channelData = new Dictionary<string, object>() {
                { "id", Channel.Id },
                { "model", "discuss.channel" },
                { "rtcInvitingSession", new Dictionary<string, object>() { { "id", member.RtcInvitingSessionId.Id } } },
            };
            var store = new Store("Thread", channelData);
            store.Add(member.RtcInvitingSessionId);
            invitationNotifications.Add(new object[] { target, "mail.record/insert", store.GetResult() });
        }
        Env.Get<BusBus>().SendMany(invitationNotifications.ToArray());
        if (members.Count > 0) {
            var channelData = new Dictionary<string, object>() {
                { "id", Channel.Id },
                { "model", "discuss.channel" },
                { "invitedMembers", new object[] {
                    ("ADD", members.Select(member => new Dictionary<string, object>() { { "id", member.Id } }).ToArray()),
                } },
            };
            var store = new Store("Thread", channelData);
            store.Add(members, fields: new Dictionary<string, object>() {
                { "id", true },
                { "channel", new Dictionary<string, object>() },
                { "persona", new Dictionary<string, object>() {
                    { "partner", new Dictionary<string, object>() {
                        { "id", true },
                        { "name", true },
                        { "im_status", true },
                    } },
                    { "guest", new Dictionary<string, object>() {
                        { "id", true },
                        { "name", true },
                        { "im_status", true },
                    } },
                } },
            });
            Env.Get<BusBus>().SendOne(Channel, "mail.record/insert", store.GetResult());
        }
        return members.ToArray();
    }

    public void MarkAsRead(int lastMessageId, bool sync = false) {
        var domain = new object[] {
            ("model", "=", "discuss.channel"),
            ("res_id", "=", Channel.Id),
            ("id", "<=", lastMessageId),
        };
        var lastMessage = Env.Get<MailMessage>().Search(domain, order: "id DESC", limit: 1);
        if (lastMessage == null) {
            return;
        }
        SetLastSeenMessage(lastMessage);
        SetNewMessageSeparator(lastMessage.Id + 1, sync: sync);
    }

    public void SetLastSeenMessage(MailMessage message, bool notify = true) {
        if (SeenMessageId.Id >= message.Id) {
            return;
        }
        FetchedMessageId = FetchedMessageId.Id >= message.Id ? FetchedMessageId : message;
        SeenMessageId = message;
        LastSeenDt = DateTime.Now;
        if (!notify) {
            return;
        }
        var target = Partner ?? Guest;
        if (Channel.ChannelType in Channel._types_allowing_seen_infos()) {
            target = Channel;
        }
        var store = new Store(this, fields: new Dictionary<string, object>() {
            { "id", true },
            { "channel", new Dictionary<string, object>() },
            { "persona", new Dictionary<string, object>() {
                { "partner", new Dictionary<string, object>() {
                    { "id", true },
                    { "name", true },
                } },
                { "guest", new Dictionary<string, object>() {
                    { "id", true },
                    { "name", true },
                } },
            } },
            { "seen_message_id", true },
        });
        Env.Get<BusBus>().SendOne(target, "mail.record/insert", store.GetResult());
    }

    public void SetNewMessageSeparator(int messageId, bool sync = false) {
        if (messageId == NewMessageSeparator) {
            return;
        }
        NewMessageSeparator = messageId;
        var target = Partner ?? Guest;
        var store = new Store(this, fields: new Dictionary<string, object>() {
            { "id", true },
            { "channel", new Dictionary<string, object>() },
            { "message_unread_counter", true },
            { "new_message_separator", true },
            { "persona", new Dictionary<string, object>() {
                { "partner", new Dictionary<string, object>() {
                    { "id", true },
                    { "name", true },
                } },
                { "guest", new Dictionary<string, object>() {
                    { "id", true },
                    { "name", true },
                } },
            } },
        });
        store.Add("ChannelMember", new Dictionary<string, object>() { { "id", Id }, { "syncUnread", sync } });
        Env.Get<BusBus>().SendOne(target, "mail.record/insert", store.GetResult());
    }

    private string GetBaseUrl() {
        // logic to get base url
        return "";
    }
}
