csharp
public partial class Bus
{
    public void GcMessages()
    {
        var timeoutAgo = DateTime.Now.AddSeconds(-TIMEOUT * 2);
        var domain = new List<object>
        {
            new List<object> { "CreateDate", "<", timeoutAgo }
        };
        Env.Sudo().Search<Bus>(domain).Unlink();
    }

    public void Sendmany(List<List<object>> notifications)
    {
        var channels = new HashSet<object>();
        var values = new List<Dictionary<string, string>>();

        foreach (var notification in notifications)
        {
            var target = notification[0];
            var notificationType = notification[1];
            var message = notification[2];

            var channel = ChannelWithDb(Env.Cr.DbName, target);
            channels.Add(channel);
            values.Add(new Dictionary<string, string>
            {
                { "Channel", JsonDump(channel) },
                { "Message", JsonDump(new Dictionary<string, object>
                {
                    { "type", notificationType },
                    { "payload", message }
                })}
            });
        }

        Env.Sudo().Create<Bus>(values);

        if (channels.Count > 0)
        {
            Env.Cr.Postcommit.Add(Notify);
        }

        void Notify()
        {
            using (var cr = Env.SqlDb.Connect("postgres").Cursor())
            {
                var query = "SELECT {0}('imbus', @p)";
                var payloads = GetNotifyPayloads(channels.ToList());
                if (payloads.Count > 1)
                {
                    Env.Logger.Info($"The imbus notification payload was too large, it's been split into {payloads.Count} payloads.");
                }
                foreach (var payload in payloads)
                {
                    cr.Execute(query, new { p = payload });
                }
            }
        }
    }

    public void Sendone(object channel, string notificationType, object message)
    {
        Sendmany(new List<List<object>> { new List<object> { channel, notificationType, message } });
    }

    public List<Dictionary<string, object>> Poll(List<object> channels, long last = 0)
    {
        List<object> domain;
        if (last == 0)
        {
            var timeoutAgo = DateTime.Now.AddSeconds(-TIMEOUT);
            domain = new List<object> { new List<object> { "CreateDate", ">", timeoutAgo } };
        }
        else
        {
            domain = new List<object> { new List<object> { "Id", ">", last } };
        }

        var channelList = channels.Select(c => JsonDump(ChannelWithDb(Env.Cr.DbName, c))).ToList();
        domain.Add(new List<object> { "Channel", "in", channelList });

        var notifications = Env.Sudo().SearchRead<Bus>(domain);
        var result = new List<Dictionary<string, object>>();

        foreach (var notif in notifications)
        {
            result.Add(new Dictionary<string, object>
            {
                { "id", notif.Id },
                { "message", JsonConvert.DeserializeObject(notif.Message) }
            });
        }

        return result;
    }

    public long BusLastId()
    {
        var last = Env.Search<Bus>(new List<object>(), orderBy: "Id desc", limit: 1).FirstOrDefault();
        return last?.Id ?? 0;
    }

    private string JsonDump(object v)
    {
        return JsonConvert.SerializeObject(v, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        });
    }

    private object ChannelWithDb(string dbname, object channel)
    {
        // Implementation of channel_with_db logic
        // This would need to be adapted based on the specific types and structures used in your C# project
        throw new NotImplementedException();
    }

    private List<string> GetNotifyPayloads(List<object> channels)
    {
        // Implementation of get_notify_payloads logic
        // This would need to be adapted based on the specific types and structures used in your C# project
        throw new NotImplementedException();
    }
}
