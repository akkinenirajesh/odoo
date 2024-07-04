csharp
public partial class DiscussChannelRtcSession {
    public DiscussChannelRtcSession() { }

    public async Task<DiscussChannelRtcSession> CreateAsync(Dictionary<string, object> values) {
        var rtcSessions = await Env.CreateMultipleAsync<DiscussChannelRtcSession>(values);
        var notifications = new List<(DiscussChannel, string, object)>();
        var rtcSessionsByChannel = new Dictionary<DiscussChannel, List<DiscussChannelRtcSession>>();
        foreach (var rtcSession in rtcSessions) {
            if (!rtcSessionsByChannel.ContainsKey(rtcSession.ChannelId)) {
                rtcSessionsByChannel.Add(rtcSession.ChannelId, new List<DiscussChannelRtcSession>());
            }
            rtcSessionsByChannel[rtcSession.ChannelId].Add(rtcSession);
        }
        foreach (var (channel, rtcSessionsForChannel) in rtcSessionsByChannel) {
            var store = new Store(rtcSessionsForChannel);
            var channelInfo = new {
                Id = channel.Id,
                Model = "discuss.channel",
                RtcSessions = new List<object> {
                    new {
                        Action = "ADD",
                        Values = rtcSessionsForChannel.Select(x => new { Id = x.Id }).ToList(),
                    }
                }
            };
            store.Add("Thread", channelInfo);
            notifications.Add((channel, "mail.record/insert", store.GetResult()));
        }
        await Env.Bus.SendManyAsync(notifications);
        return this;
    }

    public async Task UnlinkAsync() {
        var channels = this.ChannelId;
        foreach (var channel in channels) {
            if (channel.RtcSessionIds.Any() && channel.RtcSessionIds.Except(this).Any()) {
                await channel.RtcCancelInvitationsAsync();
                channel.SfuChannelUuid = null;
                channel.SfuServerUrl = null;
            }
        }
        var notifications = new List<(IEntity, string, object)>();
        var rtcSessionsByChannel = new Dictionary<DiscussChannel, List<DiscussChannelRtcSession>>();
        foreach (var rtcSession in this) {
            if (!rtcSessionsByChannel.ContainsKey(rtcSession.ChannelId)) {
                rtcSessionsByChannel.Add(rtcSession.ChannelId, new List<DiscussChannelRtcSession>());
            }
            rtcSessionsByChannel[rtcSession.ChannelId].Add(rtcSession);
        }
        foreach (var (channel, rtcSessionsForChannel) in rtcSessionsByChannel) {
            var channelInfo = new {
                Id = channel.Id,
                Model = "discuss.channel",
                RtcSessions = new List<object> {
                    new {
                        Action = "DELETE",
                        Values = rtcSessionsForChannel.Select(x => new { Id = x.Id }).ToList(),
                    }
                }
            };
            var store = new Store("Thread", channelInfo);
            notifications.Add((channel, "mail.record/insert", store.GetResult()));
        }
        foreach (var rtcSession in this) {
            var target = rtcSession.GuestId ?? rtcSession.PartnerId;
            notifications.Add((target, "discuss.channel.rtc.session/ended", new { SessionId = rtcSession.Id }));
        }
        await Env.Bus.SendManyAsync(notifications);
        await Env.DeleteAsync(this);
    }

    public async Task UpdateAndBroadcastAsync(Dictionary<string, object> values) {
        var validValues = new string[] { "IsScreenSharingOn", "IsCameraOn", "IsMuted", "IsDeaf" };
        var updateValues = values.Where(x => validValues.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        await this.WriteAsync(updateValues);
        var store = new Store(this, true);
        await Env.Bus.SendOneAsync(this.ChannelId, "discuss.channel.rtc.session/update_and_broadcast", new { Data = store.GetResult(), ChannelId = this.ChannelId.Id });
    }

    public async Task GcInactiveSessionsAsync() {
        var inactiveSessions = await Env.SearchAsync<DiscussChannelRtcSession>(x => x.WriteDate < DateTime.Now.AddMinutes(-1));
        await inactiveSessions.UnlinkAsync();
    }

    public async Task ActionDisconnectAsync() {
        var sessionIdsByChannelByUrl = new Dictionary<string, Dictionary<string, List<int>>>();
        foreach (var rtcSession in this) {
            var sfuChannelUuid = rtcSession.ChannelId.SfuChannelUuid;
            var url = rtcSession.ChannelId.SfuServerUrl;
            if (!string.IsNullOrEmpty(sfuChannelUuid) && !string.IsNullOrEmpty(url)) {
                if (!sessionIdsByChannelByUrl.ContainsKey(url)) {
                    sessionIdsByChannelByUrl.Add(url, new Dictionary<string, List<int>>());
                }
                if (!sessionIdsByChannelByUrl[url].ContainsKey(sfuChannelUuid)) {
                    sessionIdsByChannelByUrl[url].Add(sfuChannelUuid, new List<int>());
                }
                sessionIdsByChannelByUrl[url][sfuChannelUuid].Add(rtcSession.Id);
            }
        }
        var key = Discuss.GetSfuKey(Env);
        if (!string.IsNullOrEmpty(key)) {
            using (var requestsSession = new HttpClient()) {
                foreach (var (url, sessionIdsByChannel) in sessionIdsByChannelByUrl) {
                    try {
                        var data = new { SessionIdsByChannel = sessionIdsByChannel };
                        var token = Jwt.Sign(data, key, 20, Jwt.Algorithm.HS256);
                        var response = await requestsSession.PostAsync(url + "/v1/disconnect", new StringContent(token, Encoding.UTF8, "application/json"));
                        response.EnsureSuccessStatusCode();
                    } catch (HttpRequestException error) {
                        _logger.Warning($"Could not disconnect sessions at sfu server {url}: {error}");
                    }
                }
            }
        }
        await this.UnlinkAsync();
    }

    public async Task DeleteInactiveRtcSessionsAsync() {
        var inactiveSessions = await Env.SearchAsync<DiscussChannelRtcSession>(x => x.WriteDate < DateTime.Now.AddMinutes(-1));
        await inactiveSessions.UnlinkAsync();
    }

    public async Task NotifyPeersAsync(List<(List<int>, string)> notifications) {
        var payloadByTarget = new Dictionary<IEntity, Dictionary<string, object>>();
        foreach (var (targetSessionIds, content) in notifications) {
            foreach (var targetSessionId in targetSessionIds) {
                var targetSession = await Env.GetAsync<DiscussChannelRtcSession>(targetSessionId);
                if (targetSession != null) {
                    var target = targetSession.GuestId ?? targetSession.PartnerId;
                    if (!payloadByTarget.ContainsKey(target)) {
                        payloadByTarget.Add(target, new Dictionary<string, object> {
                            { "Sender", this.Id },
                            { "Notifications", new List<string>() }
                        });
                    }
                    payloadByTarget[target]["Notifications"].Add(content);
                }
            }
        }
        var sendManyTasks = payloadByTarget.Select(x => Env.Bus.SendOneAsync(x.Key, "discuss.channel.rtc.session/peer_notification", x.Value)).ToList();
        await Task.WhenAll(sendManyTasks);
    }

    public void ToStore(Store store, bool extra = false) {
        store.Add(this.ChannelMemberId, new {
            Id = true,
            Channel = new { },
            Persona = new {
                Partner = new {
                    Id = true,
                    Name = true,
                    ImStatus = true
                },
                Guest = new {
                    Id = true,
                    Name = true,
                    ImStatus = true
                }
            }
        });
        foreach (var rtcSession in this) {
            var vals = new {
                Id = rtcSession.Id,
                ChannelMember = new { Id = rtcSession.ChannelMemberId.Id }
            };
            if (extra) {
                vals = new {
                    Id = rtcSession.Id,
                    ChannelMember = new { Id = rtcSession.ChannelMemberId.Id },
                    IsCameraOn = rtcSession.IsCameraOn,
                    IsDeaf = rtcSession.IsDeaf,
                    IsSelfMuted = rtcSession.IsMuted,
                    IsScreenSharingOn = rtcSession.IsScreenSharingOn
                };
            }
            store.Add("RtcSession", vals);
        }
    }

    public static Expression<Func<DiscussChannelRtcSession, bool>> InactiveRtcSessionDomain() {
        return x => x.WriteDate < DateTime.Now.AddMinutes(-1);
    }
}
