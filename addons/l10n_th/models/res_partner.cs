csharp
public partial class ResPartner 
{
    public void ComputeL10nThBranchName()
    {
        if (!this.IsCompany || this.CountryCode != "TH")
        {
            this.L10nThBranchName = "";
        }
        else
        {
            string code = this.CompanyRegistry;
            this.L10nThBranchName = code != null ? $"Branch {code}" : "Headquarter";
        }
    }
}
