C#
public partial class MailMessage {
    public MailMessage() {
    }

    public virtual object PortalGetDefaultFormatPropertiesNames(object options) {
        var propertiesNames = Env.Call("mail.message", "_portal_get_default_format_properties_names", options);
        if (options != null && options.ContainsKey("rating_include") && (bool)options["rating_include"]) {
            propertiesNames = (object)new List<object>() { propertiesNames, "Rating", "RatingValue" };
        }
        return propertiesNames;
    }

    public virtual object PortalMessageFormat(object propertiesNames, object options) {
        var valsList = Env.Call("mail.message", "_portal_message_format", propertiesNames, options);
        if (!((List<object>)propertiesNames).Contains("Rating")) {
            return valsList;
        }

        var relatedRating = Env.SearchRead("rating.rating", new List<object>() { "message_id", "in", this.Id }, new List<string>() { "Id", "PublisherComment", "PublisherId", "PublisherDatetime", "MessageId" });
        var messageToRating = relatedRating.ToDictionary(r => r["message_id"][0], r => PortalMessageFormatRating(r));

        var result = new List<object>();
        foreach (var message in this) {
            var values = valsList[message.Id];
            values["Rating"] = messageToRating.GetValueOrDefault(message.Id);

            var record = Env.Browse(message.Model, message.ResId);
            if (record.HasMethod("rating_get_stats")) {
                values["RatingStats"] = record.Call("rating_get_stats");
            }

            result.Add(values);
        }

        return result;
    }

    public virtual object PortalMessageFormatRating(object ratingValues) {
        var publisherId = ratingValues["publisher_id"] != null ? (object)((List<object>)ratingValues["publisher_id"])[0] : null;
        var publisherName = ratingValues["publisher_id"] != null ? (object)((List<object>)ratingValues["publisher_id"])[1] : null;
        ratingValues["PublisherAvatar"] = publisherId != null ? Env.Call("res.partner", "web_image", new object[] { publisherId, "avatar_128", "50x50" }) : null;
        ratingValues["PublisherComment"] = ratingValues["publisher_comment"] ?? "";
        ratingValues["PublisherDatetime"] = Env.Call("tools", "format_datetime", new object[] { Env, ratingValues["publisher_datetime"] });
        ratingValues["PublisherId"] = publisherId;
        ratingValues["PublisherName"] = publisherName;

        return ratingValues;
    }

    public virtual object _portal_get_default_format_properties_names(object options) {
        return PortalGetDefaultFormatPropertiesNames(options);
    }

    public virtual object _portal_message_format(object propertiesNames, object options) {
        return PortalMessageFormat(propertiesNames, options);
    }

    public virtual object _portal_message_format_rating(object ratingValues) {
        return PortalMessageFormatRating(ratingValues);
    }
}
