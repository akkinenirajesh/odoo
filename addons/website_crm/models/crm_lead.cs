C#
public partial class WebsiteCrmLead {

    public int VisitorPageCount { get; set; }

    public void ComputeVisitorPageCount() {
        var mappedData = new Dictionary<int, int>();
        if (this.Id > 0) {
            Env.FlushModel("Website.CrmLead", "VisitorIds");
            Env.FlushModel("Website.Track", "VisitorId");
            var sql = @"
                SELECT l.Id as lead_id, count(*) as page_view_count
                FROM Website.CrmLead l
                JOIN website_lead_visitor_rel lv ON l.Id = lv.crm_lead_id
                JOIN Website.Visitor v ON v.Id = lv.website_visitor_id
                JOIN Website.Track p ON p.VisitorId = v.Id
                WHERE l.Id in (@Ids)
                GROUP BY l.Id";
            var pageData = Env.ExecuteQuery<dynamic>(sql, new { Ids = new List<int>() { this.Id } });
            mappedData = pageData.ToDictionary(data => (int)data.lead_id, data => (int)data.page_view_count);
        }
        this.VisitorPageCount = mappedData.ContainsKey(this.Id) ? mappedData[this.Id] : 0;
    }

    public dynamic ActionRedirectToPageViews() {
        var visitors = Env.GetRecord<Website.Visitor>(this.VisitorIds);
        var action = Env.GetAction("website.website_visitor_page_action");
        action.Domain = new List<dynamic> { new { Field = "VisitorId", Operator = "in", Value = visitors.Select(v => v.Id).ToList() } };
        if (visitors.SelectMany(v => v.WebsiteTrackIds).Count() > 15 && visitors.SelectMany(v => v.WebsiteTrackIds).Select(t => t.PageId).Count() > 1) {
            action.Context = new { search_default_group_by_page = true };
        }
        return action;
    }

    public List<dynamic> MergeGetFieldsSpecific() {
        var fieldsInfo = base.MergeGetFieldsSpecific();
        fieldsInfo.Add("VisitorIds", (fname, leads) => new List<dynamic> { new { Field = "VisitorIds", Operator = "=", Value = leads.SelectMany(l => l.VisitorIds).Select(v => v.Id).ToList() } });
        return fieldsInfo;
    }

    public Dictionary<string, dynamic> WebsiteFormInputFilter(dynamic request, Dictionary<string, dynamic> values) {
        values["MediumId"] = values.ContainsKey("MediumId") ? values["MediumId"] : this.GetDefaultValue("MediumId");
        values["TeamId"] = values.ContainsKey("TeamId") ? values["TeamId"] : request.Website.CrmDefaultTeamId.Id;
        values["UserId"] = values.ContainsKey("UserId") ? values["UserId"] : request.Website.CrmDefaultUserId.Id;
        if (values.ContainsKey("TeamId")) {
            values["Type"] = Env.GetRecord<Crm.Team>(values["TeamId"]).UseLeads ? "lead" : "opportunity";
        } else {
            values["Type"] = Env.User.HasGroup("crm.group_use_lead") ? "lead" : "opportunity";
        }
        return values;
    }
}
