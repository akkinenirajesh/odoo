csharp
public partial class Company {
    public void CreateMissingUnbuildSequences() {
        var companyIds = Env.GetModel("Mrp.Company").Search(new List<object>());
        var companyHasUnbuildSeq = Env.GetModel("Ir.Sequence").Search(new List<object>() { new[] { "Code", "=", "mrp.unbuild" } }).Select(x => x.CompanyId);
        var companyTodoSequence = companyIds.Except(companyHasUnbuildSeq);
        companyTodoSequence.ForEach(x => x.CreateUnbuildSequence());
    }

    public void CreateUnbuildSequence() {
        var unbuildVals = new List<object> {
            new Dictionary<string, object> {
                { "Name", "Unbuild" },
                { "Code", "mrp.unbuild" },
                { "CompanyId", this.Id },
                { "Prefix", "UB/" },
                { "Padding", 5 },
                { "NumberNext", 1 },
                { "NumberIncrement", 1 }
            }
        };
        Env.GetModel("Ir.Sequence").Create(unbuildVals);
    }

    public void CreatePerCompanySequences() {
        Env.GetModel("Mrp.Company").CreatePerCompanySequences();
        CreateUnbuildSequence();
    }
}
