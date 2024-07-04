csharp
public partial class WebsiteMembership.MembershipLine {
    public List<Core.Partner> GetPublishedCompanies(int? limit = null) {
        if (this.Ids.Count == 0) {
            return new List<Core.Partner>();
        }
        string limitClause = limit == null ? "" : $" LIMIT {limit}";
        Env.Cr.Execute($@"
            SELECT DISTINCT p.id
            FROM res_partner p INNER JOIN membership_membership_line m
            ON  p.id = m.partner
            WHERE is_published AND is_company AND m.id IN {this.Ids} " + limitClause);
        return Env.Cr.FetchAll().Select(partner_id => new Core.Partner(partner_id[0])).ToList();
    }
}
