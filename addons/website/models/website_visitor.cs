C#
public partial class WebsiteVisitor 
{
    public string GetAccessToken() 
    {
        if (Env.Context.ContainsKey("Request")) 
        {
            if (!Env.User.IsPublic())
            {
                return Env.User.PartnerId.Id.ToString();
            }
            var request = Env.Context["Request"] as dynamic;
            var msg = string.Format(
                "({0},{1},{2})",
                request.httprequest.remote_addr,
                request.httprequest.environ.get("HTTP_USER_AGENT"),
                request.session.sid
            ).Encode("utf-8");
            return System.Security.Cryptography.SHA1.Create().ComputeHash(msg).ToHexadecimalString().Substring(0, 32);
        }
        throw new Exception("Visitors can only be created through the frontend.");
    }

    public string DisplayName 
    {
        get 
        {
            return PartnerId.Name != null ? PartnerId.Name : string.Format("Website Visitor #{0}", this.Id);
        }
    }

    public void ComputePartnerId()
    {
        var partnerId = AccessToken.Length != 32 ? int.Parse(AccessToken) : 0;
        this.PartnerId = Env.Ref<Core.ResPartner>(partnerId);
    }

    public void ComputeEmailPhone()
    {
        var results = Env.Ref<Core.ResPartner>().SearchRead(
            new[] { new Core.Domain("Id", "in", new[] { this.PartnerId.Id }) },
            new[] { "Id", "EmailNormalized", "Mobile", "Phone" }
        );
        var mappedData = results.ToDictionary(
            result => result["Id"],
            result => new { EmailNormalized = result["EmailNormalized"], Mobile = result["Mobile"] != null ? result["Mobile"] : result["Phone"] }
        );
        this.Email = mappedData.TryGetValue(this.PartnerId.Id, out var value) ? value.EmailNormalized : null;
        this.Mobile = mappedData.TryGetValue(this.PartnerId.Id, out value) ? value.Mobile : null;
    }

    public void ComputePageStatistics()
    {
        var results = Env.Ref<Website.WebsiteTrack>().ReadGroup(
            new[] { new Website.Domain("VisitorId", "in", new[] { this.Id }), new Website.Domain("Url", "!=", null) },
            new[] { "VisitorId", "PageId" },
            new[] { "__count" }
        );
        var mappedData = new Dictionary<int, Dictionary<string, object>>();
        foreach (var result in results)
        {
            var visitorId = (int)result["VisitorId"];
            var pageId = result["PageId"];
            var count = (int)result["__count"];
            if (!mappedData.ContainsKey(visitorId))
            {
                mappedData[visitorId] = new Dictionary<string, object> { { "PageCount", 0 }, { "VisitorPageCount", 0 }, { "PageIds", new List<int>() } };
            }
            mappedData[visitorId]["VisitorPageCount"] = (int)mappedData[visitorId]["VisitorPageCount"] + count;
            mappedData[visitorId]["PageCount"] = (int)mappedData[visitorId]["PageCount"] + 1;
            if (pageId != null)
            {
                mappedData[visitorId]["PageIds"].Add((int)pageId);
            }
        }
        this.VisitorPageCount = (int)mappedData[this.Id]["VisitorPageCount"];
        this.PageCount = (int)mappedData[this.Id]["PageCount"];
        this.PageIds = (int[])mappedData[this.Id]["PageIds"];
    }

    public void ComputeLastVisitedPageId()
    {
        var results = Env.Ref<Website.WebsiteTrack>().ReadGroup(
            new[] { new Website.Domain("VisitorId", "in", new[] { this.Id }), new Website.Domain("PageId", "!=", null) },
            new[] { "VisitorId", "PageId" },
            "VisitDateTime:max"
        );
        this.LastVisitedPageId = results.FirstOrDefault(result => (int)result["VisitorId"] == this.Id)["PageId"] != null ? Env.Ref<Website.WebsitePage>((int)results.FirstOrDefault(result => (int)result["VisitorId"] == this.Id)["PageId"]) : null;
    }

    public void ComputeTimeStatistics()
    {
        this.TimeSinceLastAction = Utils.FormatTimeAgo(Env, DateTime.Now - this.LastConnectionDateTime);
        this.IsConnected = DateTime.Now - this.LastConnectionDateTime < TimeSpan.FromMinutes(5);
    }

    // ... other methods ... 
}
