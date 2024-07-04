csharp
public partial class ResPartner 
{
    public virtual int PartnerWeight { get; set; }
    public virtual WebsiteCrmPartnerAssign.ResPartnerGrade GradeId { get; set; }
    public virtual int GradeSequence { get; set; }
    public virtual WebsiteCrmPartnerAssign.ResPartnerActivation Activation { get; set; }
    public virtual DateTime? DatePartnership { get; set; }
    public virtual DateTime? DateReview { get; set; }
    public virtual DateTime? DateReviewNext { get; set; }
    public virtual ResPartner AssignedPartnerId { get; set; }
    public virtual ICollection<ResPartner> ImplementedPartnerIds { get; set; }
    public virtual int ImplementedPartnerCount { get; set; }

    public virtual void ComputeImplementedPartnerCount()
    {
        var rgResult = Env.GetModel<ResPartner>().ReadGroup(
            new[] {
                new SearchCriteria { FieldName = "AssignedPartnerId", Operator = "in", Values = new[] { this.Id } },
                new SearchCriteria { FieldName = "IsPublished", Operator = "=", Values = new[] { true } }
            },
            new[] { "AssignedPartnerId" },
            new[] { "__count" }
        );
        var rgData = rgResult.Select(r => new { AssignedPartnerId = (long)r[0], Count = (long)r[1] }).ToDictionary(r => r.AssignedPartnerId, r => r.Count);
        ImplementedPartnerCount = rgData.ContainsKey(this.Id) ? (int)rgData[this.Id] : 0;
    }

    public virtual void ComputePartnerWeight()
    {
        PartnerWeight = GradeId != null ? GradeId.PartnerWeight : 0;
    }

    public virtual void ComputeOpportunityCount()
    {
        var opportunityData = Env.GetModel<CrmLead>().WithContext(new { ActiveTest = false }).ReadGroup(
            new[] { new SearchCriteria { FieldName = "PartnerAssignedId", Operator = "in", Values = new[] { this.Id } } },
            new[] { "PartnerAssignedId" },
            new[] { "__count" }
        );
        var assignCounts = opportunityData.Select(r => new { PartnerAssignedId = (long)r[0], Count = (long)r[1] }).ToDictionary(r => r.PartnerAssignedId, r => r.Count);
        OpportunityCount += assignCounts.ContainsKey(this.Id) ? (int)assignCounts[this.Id] : 0;
    }

    public virtual void ActionViewOpportunity()
    {
        var action = Env.GetModel<ResPartner>().ActionViewOpportunity();
        var actionDomainOrigin = action.Get<object[]>("Domain");
        var actionContextOrigin = action.Get<Dictionary<string, object>>("Context");
        var actionDomainAssign = new[] { new SearchCriteria { FieldName = "PartnerAssignedId", Operator = "=", Values = new[] { this.Id } } };
        if (actionDomainOrigin == null)
        {
            action.Set("Domain", actionDomainAssign);
            return;
        }
        // perform searches independently as having OR with those leaves seems to
        // be counter productive
        var lead = Env.GetModel<CrmLead>().WithContext(actionContextOrigin ?? new Dictionary<string, object>(), new { ActiveTest = false });
        var idsOrigin = lead.Search(actionDomainOrigin).Select(r => r.Id).ToList();
        var idsNew = lead.Search(actionDomainAssign).Select(r => r.Id).ToList();
        action.Set("Domain", new[] { new SearchCriteria { FieldName = "Id", Operator = "in", Values = idsOrigin.Union(idsNew).OrderBy(i => i).ToArray() } });
    }

    public virtual Dictionary<string, object> DefaultGet(string[] fieldsList)
    {
        var defaultVals = base.DefaultGet(fieldsList);
        if (Env.Context.ContainsKey("PartnerSetDefaultGradeActivation"))
        {
            // sets the lowest grade and activation if no default values given, mainly useful while
            // creating assigned partner on the fly (to make it visible in same m2o again)
            if (fieldsList.Contains("GradeId") && !defaultVals.ContainsKey("GradeId"))
            {
                defaultVals["GradeId"] = Env.GetModel<ResPartnerGrade>().Search(new[] { new SearchCriteria { FieldName = "Sequence", Operator = "order", Values = new[] { "ASC" } } }, 1).FirstOrDefault().Id;
            }
            if (fieldsList.Contains("Activation") && !defaultVals.ContainsKey("Activation"))
            {
                defaultVals["Activation"] = Env.GetModel<ResPartnerActivation>().Search(new[] { new SearchCriteria { FieldName = "Sequence", Operator = "order", Values = new[] { "ASC" } } }, 1).FirstOrDefault().Id;
            }
        }
        return defaultVals;
    }

    public int OpportunityCount { get; set; }
}
