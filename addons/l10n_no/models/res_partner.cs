C#
public partial class ResPartner {
    public string _deduce_country_code() {
        if (this.L10nNoBronnoysundNumber != null) {
            return "NO";
        }
        return Env.Model("Res.Partner").Call("_deduce_country_code", this);
    }

    public List<string> _peppol_eas_endpoint_depends() {
        List<string> result = Env.Model("Res.Partner").Call("_peppol_eas_endpoint_depends", this);
        result.Add("L10nNoBronnoysundNumber");
        return result;
    }
}
