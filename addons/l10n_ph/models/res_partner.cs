csharp
public partial class ResPartner {
    public string BranchCode { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }

    public void ComputeBranchCode() {
        string branchCode = "000";
        if (Env.Get("CountryId").Code == "PH" && this.Vat != null) {
            var match = Regex.Match(this.Vat, @"^\d{3}-\d{3}-\d{4}-\d{3}$");
            if (match.Success && match.Groups[1].Success) {
                branchCode = match.Groups[1].Value[1..];
            }
        }
        this.BranchCode = branchCode;
    }

    public List<string> CommercialFields() {
        var fields = base.CommercialFields();
        fields.Add("BranchCode");
        return fields;
    }
}
