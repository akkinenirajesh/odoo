csharp
public partial class MailMessage {
    // all the model methods are written here.

    public void Populate(int size)
    {
        var res = this.PopulateInternal(size);
        var admin = Env.Ref("base.user_admin").PartnerId;
        var random = new Random("mail.message in discuss");
        var channels = Env.Registry.PopulatedModels["Discuss.Channel"].Select(x => Env.Browse<Discuss.Channel>(x)).ToList();
        var messages = new List<MailMessage>();
        var bigDone = 0;
        foreach (var channel in channels.Where(c => c.ChannelMemberIds.Any()))
        {
            var big = size switch
            {
                "small" => 80,
                "medium" => 150,
                "large" => 300,
                _ => 0,
            };
            var smallBigRatio = size switch
            {
                "small" => 10,
                "medium" => 150,
                "large" => 1000,
                _ => 0,
            };
            var maxMessages = random.Next(1, smallBigRatio) == 1 ? big : 60;
            var adminIsMember = admin.IsIn(channel.ChannelMemberIds.Select(x => x.PartnerId).ToList());
            var numberMessages = bigDone < 2 && adminIsMember ? 500 : random.Next(maxMessages);
            if (numberMessages >= 500 && adminIsMember)
            {
                bigDone++;
            }
            for (var counter = 0; counter < numberMessages; counter++)
            {
                messages.Add(new MailMessage
                {
                    AuthorId = random.Choice(channel.ChannelMemberIds.Select(x => x.PartnerId)).Id,
                    Body = $"message_body_{counter}",
                    MessageType = "Comment",
                    Model = "Discuss.Channel",
                    ResId = channel.Id,
                });
            }
        }
        var batches = messages.Chunk(1000).ToList();
        var count = 0;
        foreach (var batch in batches)
        {
            count += batch.Count;
            Env.Logger.Info($"Batch of mail.message for discuss.channel: {count}/{messages.Count}");
            res += Env.Create<MailMessage>(batch);
        }
        return res;
    }

    private MailMessage PopulateInternal(int size)
    {
        return this;
    }

    public List<Discuss.Channel> _PopulateDependencies()
    {
        var dependencies = base._PopulateDependencies();
        dependencies.AddRange(new List<Discuss.Channel>() { Env.Ref<Discuss.Channel>("discuss.channel"), Env.Ref<Discuss.Channel.Member>("discuss.channel.member") });
        return dependencies;
    }
}
