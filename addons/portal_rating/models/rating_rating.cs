C#
public partial class PortalRating
{
    public PortalRating()
    {
    }

    public void Create(List<Dictionary<string, object>> valuesList)
    {
        foreach (var values in valuesList)
        {
            SynchronizePublisherValues(values);
        }
        var ratings = Env.Create<PortalRating>(valuesList);
        if (ratings.Any(r => r.PublisherComment != null))
        {
            ratings.CheckSynchronizePublisherValues();
        }
    }

    public void Write(Dictionary<string, object> values)
    {
        SynchronizePublisherValues(values);
        Env.Write(this, values);
    }

    private void CheckSynchronizePublisherValues()
    {
        var editorGroup = Env.GetRecordById<ResPartner>("website.group_website_restricted_editor");
        if (editorGroup != null && Env.User.HasGroup(editorGroup))
        {
            return;
        }

        var modelData = ClassifyByModel();
        foreach (var (model, data) in modelData)
        {
            var records = Env.GetRecords<object>(model, data["record_ids"]);
            try
            {
                records.CheckAccessRights("write");
                records.CheckAccessRule("write");
            }
            catch (Exception e)
            {
                throw new Exception("Updating rating comment require write access on related record.", e);
            }
        }
    }

    private Dictionary<string, object> SynchronizePublisherValues(Dictionary<string, object> values)
    {
        if (values.ContainsKey("PublisherComment"))
        {
            CheckSynchronizePublisherValues();
            if (!values.ContainsKey("PublisherDateTime"))
            {
                values["PublisherDateTime"] = DateTime.Now;
            }
            if (!values.ContainsKey("PublisherId"))
            {
                values["PublisherId"] = Env.User.PartnerId.Id;
            }
        }
        return values;
    }

    private Dictionary<string, Dictionary<string, object>> ClassifyByModel()
    {
        // Implement this method to classify records based on their model.
        // This is a placeholder, and you should replace it with the actual logic.
        return new Dictionary<string, Dictionary<string, object>>();
    }
}
