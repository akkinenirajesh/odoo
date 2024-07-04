C#
public partial class MailMessage 
{
    public void ComputeRatingValue()
    {
        var ratings = Env.GetRecords<Rating.Rating>().Search(r => r.MessageId.IsIn(this.Id) && r.Consumed == true, "CreateDate DESC");
        var mapping = ratings.ToDictionary(r => r.MessageId.Id, r => r.Rating);
        this.RatingValue = mapping.GetValueOrDefault(this.Id, 0.0);
    }

    public object SearchRatingValue(string operator, object operand)
    {
        var ratings = Env.GetRecords<Rating.Rating>().Search(r => r.Rating.Operator(operator, operand) && r.MessageId != null);
        return new { Id = ratings.Select(r => r.MessageId.Id).ToList() };
    }

    public object MessageFormat(bool formatReply = true, object msgVals = null, bool forCurrentUser = false)
    {
        var messageValues = (object)Env.CallMethod("mail.message", "_message_format", new object[] { formatReply, msgVals, forCurrentUser });

        var ratingMixinMessages = this.Filter(m => m.Model != null && m.ResId != null && Env.IsSubclassOf(m.Model, "rating.mixin"));

        if (ratingMixinMessages != null)
        {
            var ratings = Env.GetRecords<Rating.Rating>().Search(r => r.MessageId.IsIn(ratingMixinMessages.Select(m => m.Id).ToList()) && r.Consumed == true);
            var ratingByMessageId = ratings.ToDictionary(r => r.MessageId.Id, r => r);

            foreach (var vals in (object[])messageValues)
            {
                if (((object)vals).GetPropertyValue("Id") is int id && ratingByMessageId.ContainsKey(id))
                {
                    var rating = ratingByMessageId[id];

                    ((object)vals).SetPropertyValue("rating", new
                    {
                        id = rating.Id,
                        ratingImageUrl = rating.RatingImageUrl,
                        ratingText = rating.RatingText
                    });
                }
            }
        }

        return messageValues;
    }
}
