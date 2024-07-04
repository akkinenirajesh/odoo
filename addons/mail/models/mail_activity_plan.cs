C#
public partial class MailActivityPlan {
    public string Name { get; set; }
    public ResCompany CompanyId { get; set; }
    public List<MailActivityPlanTemplate> TemplateIds { get; set; }
    public bool Active { get; set; }
    public IrModel ResModelId { get; set; }
    public string ResModel { get; set; }
    public int StepsCount { get; set; }
    public bool HasUserOnDemand { get; set; }

    public List<string> GetModelSelection() {
        return Env.Get<IrModel>().Search(m => m.IsMailThread && !m.Transient)
          .Select(m => new string[] { m.Model, m.Name }).ToList().SelectMany(s => s).ToList();
    }

    public void ComputeResModelId() {
        ResModelId = Env.Get<IrModel>().GetId(ResModel);
    }

    public void CheckResModelCompatibilityWithTemplates() {
        foreach (MailActivityPlanTemplate template in TemplateIds) {
            template.CheckActivityTypeResModel();
        }
    }

    public void ComputeStepsCount() {
        StepsCount = TemplateIds.Count;
    }

    public void ComputeHasUserOnDemand() {
        HasUserOnDemand = TemplateIds.Any(t => t.ResponsibleType == "on_demand");
    }

    public List<MailActivityPlan> CopyData(Dictionary<string, object> defaultValues = null) {
        List<MailActivityPlan> copiedPlans = new List<MailActivityPlan>();
        foreach (MailActivityPlan plan in this) {
            MailActivityPlan copiedPlan = plan.Copy();
            if (defaultValues == null || !defaultValues.ContainsKey("Name")) {
                copiedPlan.Name = $"{plan.Name} (copy)";
            }
            copiedPlans.Add(copiedPlan);
        }
        return copiedPlans;
    }
}
