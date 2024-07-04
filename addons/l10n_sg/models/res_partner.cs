C#
public partial class ResPartner {
    public string L10nSgUniqueEntityNumber { get; set; }

    public string DeduceCountryCode() {
        if (!string.IsNullOrEmpty(this.L10nSgUniqueEntityNumber)) {
            return "SG";
        }
        return Env.Model("Res.Partner").Call<string>("_deduce_country_code");
    }

    public List<string> PeppolEasEndpointDepends() {
        List<string> depends = Env.Model("Res.Partner").Call<List<string>>("_peppol_eas_endpoint_depends");
        depends.Add("L10nSgUniqueEntityNumber");
        return depends;
    }
}
