csharp
public partial class PosLoyalty.LoyaltyProgram {

    public PosLoyalty.LoyaltyProgram() {
    }

    public string Name { get; set; }

    public PosLoyalty.LoyaltyProgramTrigger Trigger { get; set; }

    public PosLoyalty.LoyaltyProgramAppliesOn AppliesOn { get; set; }

    public PosLoyalty.LoyaltyProgramType ProgramType { get; set; }

    public List<Product.Pricelist> PricelistIds { get; set; }

    public DateTime DateFrom { get; set; }

    public DateTime DateTo { get; set; }

    public bool LimitUsage { get; set; }

    public int MaxUsage { get; set; }

    public bool IsNominative { get; set; }

    public bool PortalVisible { get; set; }

    public string PortalPointName { get; set; }

    public List<Product.Product> TriggerProductIds { get; set; }

    public List<PosLoyalty.LoyaltyRule> RuleIds { get; set; }

    public List<PosLoyalty.LoyaltyReward> RewardIds { get; set; }

    public List<PosLoyalty.LoyaltyMail> CommunicationPlanIds { get; set; }

    public List<Pos.PosConfig> PosConfigIds { get; set; }

    public int PosOrderCount { get; set; }

    public bool PosOk { get; set; }

    public Ir.ActionsReport PosReportPrintId { get; set; }

    public Mail.Template MailTemplateId { get; set; }

    public void ComputePosConfigIds() {
        if (!this.PosOk) {
            this.PosConfigIds = null;
        }
    }

    public void ComputePosOrderCount() {
        var readGroupRes = Env.Get<Pos.PosOrderLine>().ReadGroup(new List<object> {
            new Dictionary<string, object> { { "RewardId", new List<object> { this.RewardIds } } }
        }, new List<string> { "OrderId" }, new List<string> { "RewardId:array_agg" });

        this.PosOrderCount = readGroupRes.Sum(x => x.RewardId.Any(y => this.RewardIds.Contains(y)) ? 1 : 0);
    }

    public void ComputeTotalOrderCount() {
        // call the super method here
        this.TotalOrderCount += this.PosOrderCount;
    }

    public void ComputePosReportPrintId() {
        this.PosReportPrintId = this.CommunicationPlanIds.FirstOrDefault()?.PosReportPrintId;
    }

    public void InversePosReportPrintId() {
        if (this.ProgramType != PosLoyalty.LoyaltyProgramType.GiftCard && this.ProgramType != PosLoyalty.LoyaltyProgramType.EWallet) {
            return;
        }

        if (this.PosReportPrintId != null) {
            if (this.MailTemplateId == null) {
                throw new UserError(string.Format("You must set '{0}' before setting '{1}'.",
                    Env.Get<PosLoyalty.LoyaltyProgram>()._fields.Get("MailTemplateId").GetDescription().string,
                    Env.Get<PosLoyalty.LoyaltyProgram>()._fields.Get("PosReportPrintId").GetDescription().string));
            } else {
                if (this.CommunicationPlanIds == null) {
                    this.CommunicationPlanIds = new List<PosLoyalty.LoyaltyMail>() {
                        Env.Get<PosLoyalty.LoyaltyMail>().Create(new Dictionary<string, object>() {
                            { "ProgramId", this.Id },
                            { "Trigger", "create" },
                            { "MailTemplateId", this.MailTemplateId.Id },
                            { "PosReportPrintId", this.PosReportPrintId.Id },
                        })
                    };
                } else {
                    this.CommunicationPlanIds.ForEach(x => {
                        x.Trigger = "create";
                        x.PosReportPrintId = this.PosReportPrintId;
                    });
                }
            }
        }
    }

    public void LoadPosDataDomain(Dictionary<string, object> data) {
        var configId = Env.Get<Pos.PosConfig>().Browse(data["Pos.PosConfig"]["data"][0]["id"]);
        return new List<object> {
            new Dictionary<string, object> {
                { "Id", new List<object> { configId.GetProgramIds() } }
            }
        };
    }

    public List<string> LoadPosDataFields(Pos.PosConfig configId) {
        return new List<string>() {
            "Name", "Trigger", "AppliesOn", "ProgramType", "PricelistIds", "DateFrom",
            "DateTo", "LimitUsage", "MaxUsage", "IsNominative", "PortalVisible",
            "PortalPointName", "TriggerProductIds", "RuleIds", "RewardIds"
        };
    }
}
