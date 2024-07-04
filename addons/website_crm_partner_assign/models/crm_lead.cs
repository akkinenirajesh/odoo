csharp
public partial class CrmLead {
    public void ComputeDatePartnerAssign() {
        if (this.PartnerAssignedId == null) {
            this.DatePartnerAssign = null;
        } else {
            this.DatePartnerAssign = Env.Context.Today;
        }
    }

    public List<string> MergeGetFields() {
        var fieldsList = base.MergeGetFields();
        fieldsList.Add("PartnerLatitude");
        fieldsList.Add("PartnerLongitude");
        fieldsList.Add("PartnerAssignedId");
        fieldsList.Add("DatePartnerAssign");
        return fieldsList;
    }

    public void AssignSalesmanOfAssignedPartner() {
        if (this.Active && this.Probability < 100 && this.PartnerAssignedId != null && this.PartnerAssignedId.User != this.User) {
            this.User = this.PartnerAssignedId.User;
        }
    }

    public void ActionAssignPartner() {
        var leadsWithCountry = this.Where(lead => lead.Country != null);
        var leadsWithoutCountry = this.Except(leadsWithCountry);
        if (leadsWithoutCountry.Any()) {
            Env.Bus.SendOne(Env.User.Partner, "simple_notification", new {
                type = "danger",
                title = "Warning",
                message = $"There is no country set in addresses for {string.Join(", ", leadsWithoutCountry.Select(lead => lead.Name))}"
            });
        }
        leadsWithCountry.AssignPartner(null);
    }

    public void AssignPartner(int? partnerId) {
        Dictionary<int, int?> partnerDict = null;
        if (!partnerId.HasValue) {
            partnerDict = SearchGeoPartner();
        }
        foreach (var lead in this) {
            if (!partnerId.HasValue) {
                partnerId = partnerDict[lead.Id];
            }
            if (!partnerId.HasValue) {
                var tagToAdd = Env.Ref("website_crm_partner_assign.tag_portal_lead_partner_unavailable");
                if (tagToAdd != null) {
                    lead.TagIds.Add(tagToAdd);
                }
                continue;
            }
            lead.AssignGeoLocalize(lead.PartnerLatitude, lead.PartnerLongitude);
            var partner = Env.Get<Res.Partner>().Get(partnerId.Value);
            if (partner.User != null) {
                lead.HandleSalesmenAssignment(partner.User.Id);
            }
            lead.PartnerAssignedId = partner;
        }
    }

    public bool AssignGeoLocalize(float? latitude, float? longitude) {
        if (latitude.HasValue && longitude.HasValue) {
            this.PartnerLatitude = latitude.Value;
            this.PartnerLongitude = longitude.Value;
            return true;
        }
        foreach (var lead in this) {
            if (lead.PartnerLatitude.HasValue && lead.PartnerLongitude.HasValue) {
                continue;
            }
            if (lead.Country != null) {
                var result = Env.Get<Res.Partner>().GeoLocalize(lead.Street, lead.Zip, lead.City, lead.State.Name, lead.Country.Name);
                if (result != null) {
                    lead.PartnerLatitude = result[0];
                    lead.PartnerLongitude = result[1];
                }
            }
        }
        return true;
    }

    public Dictionary<int, int?> SearchGeoPartner() {
        var partner = Env.Get<Res.Partner>();
        Dictionary<int, int?> resPartnerIds = new Dictionary<int, int?>();
        this.AssignGeoLocalize();
        foreach (var lead in this) {
            List<int> partnerIds = new List<int>();
            if (lead.Country == null) {
                continue;
            }
            var latitude = lead.PartnerLatitude;
            var longitude = lead.PartnerLongitude;
            if (latitude.HasValue && longitude.HasValue) {
                // 1. first way: in the same country, small area
                partnerIds = partner.Search(new[] {
                    new SearchCondition("PartnerWeight", ">", 0),
                    new SearchCondition("PartnerLatitude", ">", latitude - 2),
                    new SearchCondition("PartnerLatitude", "<", latitude + 2),
                    new SearchCondition("PartnerLongitude", ">", longitude - 1.5),
                    new SearchCondition("PartnerLongitude", "<", longitude + 1.5),
                    new SearchCondition("Country", "=", lead.Country.Id),
                    new SearchCondition("Id", "not in", lead.PartnerDeclinedIds.Select(p => p.Id))
                }).Select(p => p.Id).ToList();

                // 2. second way: in the same country, big area
                if (!partnerIds.Any()) {
                    partnerIds = partner.Search(new[] {
                        new SearchCondition("PartnerWeight", ">", 0),
                        new SearchCondition("PartnerLatitude", ">", latitude - 4),
                        new SearchCondition("PartnerLatitude", "<", latitude + 4),
                        new SearchCondition("PartnerLongitude", ">", longitude - 3),
                        new SearchCondition("PartnerLongitude", "<", longitude + 3),
                        new SearchCondition("Country", "=", lead.Country.Id),
                        new SearchCondition("Id", "not in", lead.PartnerDeclinedIds.Select(p => p.Id))
                    }).Select(p => p.Id).ToList();
                }

                // 3. third way: in the same country, extra large area
                if (!partnerIds.Any()) {
                    partnerIds = partner.Search(new[] {
                        new SearchCondition("PartnerWeight", ">", 0),
                        new SearchCondition("PartnerLatitude", ">", latitude - 8),
                        new SearchCondition("PartnerLatitude", "<", latitude + 8),
                        new SearchCondition("PartnerLongitude", ">", longitude - 8),
                        new SearchCondition("PartnerLongitude", "<", longitude + 8),
                        new SearchCondition("Country", "=", lead.Country.Id),
                        new SearchCondition("Id", "not in", lead.PartnerDeclinedIds.Select(p => p.Id))
                    }).Select(p => p.Id).ToList();
                }

                // 5. fifth way: anywhere in same country
                if (!partnerIds.Any()) {
                    // still haven't found any, let's take all partners in the country!
                    partnerIds = partner.Search(new[] {
                        new SearchCondition("PartnerWeight", ">", 0),
                        new SearchCondition("Country", "=", lead.Country.Id),
                        new SearchCondition("Id", "not in", lead.PartnerDeclinedIds.Select(p => p.Id))
                    }).Select(p => p.Id).ToList();
                }

                // 6. sixth way: closest partner whatsoever, just to have at least one result
                if (!partnerIds.Any()) {
                    // warning: point() type takes (longitude, latitude) as parameters in this order!
                    Env.Execute($"SELECT id, distance FROM (select id, (point(partner_longitude, partner_latitude) <-> point({longitude},{latitude})) AS distance FROM res_partner WHERE active AND partner_longitude IS NOT NULL AND partner_latitude IS NOT NULL AND partner_weight > 0 AND id not in (select partner_id from crm_lead_declined_partner where lead_id = {lead.Id})) AS d ORDER BY distance LIMIT 1");
                    var res = Env.FetchOne<Dictionary<string, object>>();
                    if (res != null) {
                        partnerIds = new List<int>() { (int)res["id"] };
                    }
                }

                if (partnerIds.Any()) {
                    resPartnerIds[lead.Id] = partnerIds.OrderBy(id => Random.Shared.Next()).First();
                }
            }
        }
        return resPartnerIds;
    }

    public void PartnerInterested(string comment) {
        var message = $"<p>I am interested by this lead.</p>";
        if (!string.IsNullOrEmpty(comment)) {
            message += $"<p>{comment}</p>";
        }
        foreach (var lead in this) {
            lead.MessagePost(message);
            lead.ConvertOpportunity(lead.Partner);
        }
    }

    public void PartnerDesinterested(string comment, bool contacted, bool spam) {
        var message = contacted ? "<p>I am not interested by this lead. I contacted the lead.</p>" : "<p>I am not interested by this lead. I have not contacted the lead.</p>";
        var partnerIds = Env.Get<Res.Partner>().Search(new[] { new SearchCondition("Id", "child_of", Env.User.Partner.CommercialPartner.Id) }).Select(p => p.Id).ToList();
        this.MessageUnsubscribe(partnerIds);
        if (!string.IsNullOrEmpty(comment)) {
            message += $"<p>{comment}</p>";
        }
        this.MessagePost(message);
        var values = new Dictionary<string, object>() {
            { "PartnerAssignedId", null }
        };
        if (spam) {
            var tagSpam = Env.Ref("website_crm_partner_assign.tag_portal_lead_is_spam");
            if (tagSpam != null && !this.TagIds.Contains(tagSpam)) {
                values.Add("TagIds", new List<int>() { tagSpam.Id });
            }
        }
        if (partnerIds.Any()) {
            values.Add("PartnerDeclinedIds", partnerIds.Select(p => new Many2ManyValue(p)).ToList());
        }
        this.Write(values);
    }

    public void UpdateLeadPortal(Dictionary<string, object> values) {
        this.CheckAccessRights("write");
        foreach (var lead in this) {
            var leadValues = new Dictionary<string, object>() {
                { "ExpectedRevenue", values["expected_revenue"] },
                { "Probability", values["probability"] },
                { "Priority", values["priority"] },
                { "DateDeadline", values["date_deadline"] }
            };
            // As activities may belong to several users, only the current portal user activity
            // will be modified by the portal form. If no activity exist we create a new one instead
            // that we assign to the portal user.
            var userActivity = lead.ActivityIds.Where(activity => activity.User == Env.User).FirstOrDefault();
            if (values.ContainsKey("activity_date_deadline")) {
                if (userActivity != null) {
                    userActivity.Write(new Dictionary<string, object>() {
                        { "ActivityTypeId", values["activity_type_id"] },
                        { "Summary", values["activity_summary"] },
                        { "DateDeadline", values["activity_date_deadline"] }
                    });
                } else {
                    Env.Get<Mail.Activity>().Create(new Dictionary<string, object>() {
                        { "ResModelId", Env.Ref("crm.model_crm_lead").Id },
                        { "ResId", lead.Id },
                        { "UserId", Env.User.Id },
                        { "ActivityTypeId", values["activity_type_id"] },
                        { "Summary", values["activity_summary"] },
                        { "DateDeadline", values["activity_date_deadline"] }
                    });
                }
            }
            lead.Write(leadValues);
        }
    }

    public void UpdateContactDetailsFromPortal(Dictionary<string, object> values) {
        this.CheckAccessRights("write");
        var fields = new List<string>() { "PartnerName", "Phone", "Mobile", "EmailFrom", "Street", "Street2", "City", "Zip", "StateId", "CountryId" };
        if (values.Keys.Any(key => !fields.Contains(key))) {
            throw new Exception($"Not allowed to update the following field(s): {string.Join(", ", values.Keys.Where(key => !fields.Contains(key)))}");
        }
        this.Write(values);
    }

    public Dictionary<string, object> CreateOppPortal(Dictionary<string, object> values) {
        if (Env.User.Partner.Grade == null && Env.User.CommercialPartner.Grade == null) {
            throw new Exception("Access Denied");
        }
        if (!(values.ContainsKey("contact_name") && values.ContainsKey("description") && values.ContainsKey("title"))) {
            return new Dictionary<string, object>() {
                { "errors", "All fields are required!" }
            };
        }
        var tagOwn = Env.Ref("website_crm_partner_assign.tag_portal_lead_own_opp");
        var valuesToCreate = new Dictionary<string, object>() {
            { "ContactName", values["contact_name"] },
            { "Name", values["title"] },
            { "Description", values["description"] },
            { "Priority", "2" },
            { "PartnerAssignedId", Env.User.CommercialPartner.Id }
        };
        if (tagOwn != null) {
            valuesToCreate.Add("TagIds", new List<int>() { tagOwn.Id });
        }
        var lead = Env.Get<CrmLead>().Create(valuesToCreate);
        lead.AssignSalesmanOfAssignedPartner();
        lead.ConvertOpportunity(lead.Partner);
        return new Dictionary<string, object>() {
            { "id", lead.Id }
        };
    }

    //
    //   DO NOT FORWARD PORT IN MASTER
    //   instead, crm.lead should implement portal.mixin
    //
    public Dictionary<string, object> GetAccessAction(int? accessUid, bool forceWebsite) {
        if (accessUid.HasValue) {
            try {
                this.CheckAccessRights("read");
                this.CheckAccessRule("read");
            } catch (Exception) {
                return base.GetAccessAction(accessUid, forceWebsite);
            }
            var user = Env.Get<Res.Users>().Get(accessUid.Value);
            this.WithUser(user);
        }
        if (Env.User.Share || forceWebsite) {
            try {
                this.CheckAccessRights("read");
                this.CheckAccessRule("read");
            } catch (Exception) {
            }
            return new Dictionary<string, object>() {
                { "type", "ir.actions.act_url" },
                { "url", $"/my/opportunity/{this.Id}" }
            };
        }
        return base.GetAccessAction(accessUid, forceWebsite);
    }
}
