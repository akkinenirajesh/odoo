csharp
public partial class Alias {
    public string AliasFullName { get; set; }
    public string DisplayName { get; set; }
    public Mail.AliasDomain AliasDomain { get; set; }
    public Ir.Model AliasedModel { get; set; }
    public string AliasDefaults { get; set; }
    public int AliasForceThreadId { get; set; }
    public Ir.Model AliasParentModel { get; set; }
    public int AliasParentThreadId { get; set; }
    public string AliasContact { get; set; }
    public bool AliasIncomingLocal { get; set; }
    public string AliasBouncedContent { get; set; }
    public string AliasStatus { get; set; }

    public void ComputeAliasFullName() {
        this.AliasFullName = $"{this.AliasName}@{this.AliasDomain.Name}";
    }

    public void ComputeDisplayName() {
        this.DisplayName = $"{this.AliasName}@{this.AliasDomain.Name}";
    }

    public Mail.AliasDomain GetCompanyAliasDomain() {
        return Env.Company.AliasDomain;
    }

    public void ComputeAliasStatus() {
        this.AliasStatus = "NotTested";
    }
}
