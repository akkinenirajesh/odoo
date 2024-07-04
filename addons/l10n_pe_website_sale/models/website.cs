C#
public partial class Website {
    public bool DisplayPartnerB2BFields() {
        return this.Company.Code == "PE" || Env.Call("website", "_display_partner_b2b_fields");
    }
}
