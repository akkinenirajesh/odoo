C#
public partial class Rating
{
    public Rating()
    {
    }

    private string DefaultAccessToken()
    {
        return Guid.NewGuid().ToString("N");
    }

    private string SelectionTargetModel()
    {
        var models = Env.GetModel("ir.model").Search(new List<object>());
        return string.Join(",", models.Select(x => string.Format("{0},{1}", x.Model, x.Name)));
    }

    public void ComputeResName()
    {
        if (ResModel != null && ResId != 0)
        {
            ResName = Env.GetModel(ResModel).Browse(ResId).Name;
        }
    }

    public void ComputeResourceRef()
    {
        if (ResModel != null && Env.GetModel(ResModel) != null)
        {
            ResourceRef = string.Format("{0},{1}", ResModel, ResId);
        }
    }

    public void ComputeParentRef()
    {
        if (ParentResModel != null && Env.GetModel(ParentResModel) != null)
        {
            ParentRef = string.Format("{0},{1}", ParentResModel, ParentResId);
        }
    }

    public void ComputeParentResName()
    {
        if (ParentResModel != null && ParentResId != 0)
        {
            ParentResName = Env.GetModel(ParentResModel).Browse(ParentResId).Name;
        }
    }

    private string GetRatingImageFilename()
    {
        return string.Format("rating_{0}.png", RatingData.RatingToThreshold(Rating));
    }

    public void ComputeRatingImage()
    {
        var imagePath = string.Format("rating/static/src/img/{0}", GetRatingImageFilename());
        RatingImageUrl = string.Format("/{0}", imagePath);
        try
        {
            RatingImage = Convert.ToBase64String(File.ReadAllBytes(imagePath));
        }
        catch (Exception)
        {
            RatingImage = null;
        }
    }

    public void ComputeRatingText()
    {
        RatingText = RatingData.RatingToText(Rating);
    }

    public Rating Create(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("ResModelId") && vals.ContainsKey("ResId"))
        {
            vals.Update(FindParentData(vals));
        }
        return Env.GetModel("Rating.Rating").Create(vals);
    }

    public void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("ResModelId") && vals.ContainsKey("ResId"))
        {
            vals.Update(FindParentData(vals));
        }
        this.Write(vals);
    }

    public void Unlink()
    {
        Env.GetModel("mail.message").Search(new List<object>() { ["RatingIds", "in", new List<object>() { this.Id }] }).Unlink();
        this.Unlink();
    }

    private Dictionary<string, object> FindParentData(Dictionary<string, object> values)
    {
        var currentModelName = Env.GetModel("ir.model").Browse(values["ResModelId"]).Model;
        var currentRecord = Env.GetModel(currentModelName).Browse(values["ResId"]);
        var data = new Dictionary<string, object>()
        {
            ["ParentResModelId"] = false,
            ["ParentResId"] = false
        };
        if (currentRecord.GetType().GetMethod("_RatingGetParentFieldName") != null)
        {
            var currentRecordParent = currentRecord.GetType().GetMethod("_RatingGetParentFieldName").Invoke(currentRecord, null);
            if (currentRecordParent != null)
            {
                var parentResModel = currentRecord.GetType().GetProperty(currentRecordParent.ToString()).GetValue(currentRecord);
                data["ParentResModelId"] = Env.GetModel(parentResModel.GetType().Name).Id;
                data["ParentResId"] = parentResModel.Id;
            }
        }
        return data;
    }

    public void Reset()
    {
        Rating = 0;
        AccessToken = DefaultAccessToken();
        Feedback = null;
        Consumed = false;
        this.Write(new Dictionary<string, object>() { ["Rating"] = Rating, ["AccessToken"] = AccessToken, ["Feedback"] = Feedback, ["Consumed"] = Consumed });
    }

    public void ActionOpenRatedObject()
    {
        var action = new Dictionary<string, object>()
        {
            ["type"] = "ir.actions.act_window",
            ["res_model"] = ResModel,
            ["res_id"] = ResId,
            ["views"] = new List<object>() { new List<object>() { false, "form" } }
        };
        Env.ExecuteAction(action);
    }

    public Dictionary<string, object> ClassifyByModel()
    {
        var dataByModel = new Dictionary<string, object>();
        foreach (var rating in this.Search(new List<object>() { ["ResModel", "!=", null], ["ResId", "!=", 0] }))
        {
            if (!dataByModel.ContainsKey(rating.ResModel))
            {
                dataByModel.Add(rating.ResModel, new Dictionary<string, object>()
                {
                    ["Ratings"] = new List<Rating>(),
                    ["RecordIds"] = new List<object>()
                });
            }
            ((List<Rating>)dataByModel[rating.ResModel]["Ratings"]).Add(rating);
            ((List<object>)dataByModel[rating.ResModel]["RecordIds"]).Add(rating.ResId);
        }
        return dataByModel;
    }

    public int Id { get; set; }
    public DateTime CreateDate { get; set; }
    public string ResName { get; set; }
    public int ResModelId { get; set; }
    public string ResModel { get; set; }
    public int ResId { get; set; }
    public string ResourceRef { get; set; }
    public string ParentResName { get; set; }
    public int ParentResModelId { get; set; }
    public string ParentResModel { get; set; }
    public int ParentResId { get; set; }
    public string ParentRef { get; set; }
    public int RatedPartnerId { get; set; }
    public string RatedPartnerName { get; set; }
    public int PartnerId { get; set; }
    public double Rating { get; set; }
    public byte[] RatingImage { get; set; }
    public string RatingImageUrl { get; set; }
    public string RatingText { get; set; }
    public string Feedback { get; set; }
    public int MessageId { get; set; }
    public bool IsInternal { get; set; }
    public string AccessToken { get; set; }
    public bool Consumed { get; set; }

    public BuviContext Env { get; set; }
}
