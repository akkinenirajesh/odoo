C#
public partial class WebsiteSaleLoyalty.LoyaltyRule {
    public void ConstrainsCode() {
        var withCode = this.Env.Search<WebsiteSaleLoyalty.LoyaltyRule>(r => r.Mode == "with_code");
        var mappedCodes = withCode.Map(r => r.Code);
        var readResult = this.Env.SearchRead<WebsiteSaleLoyalty.LoyaltyRule>(
            r => (r.WebsiteId.In(new List<long>() { 0 }.Concat(this.WebsiteId.Ids)) && r.Mode == "with_code" && r.Code.In(mappedCodes) && r.Id.NotIn(withCode.Ids)),
            new List<string>() { "Code", "WebsiteId" }
        ).Concat(withCode.Map(p => new { Code = p.Code, WebsiteId = p.WebsiteId }));
        var existingCodes = new HashSet<(string, long)>();
        foreach (var res in readResult) {
            var websiteChecks = res.WebsiteId != 0 ? (res.WebsiteId, 0L) : (0L,);
            foreach (var website in websiteChecks) {
                var val = (res.Code, website);
                if (existingCodes.Contains(val)) {
                    throw new ValidationError("The promo code must be unique.");
                }
                existingCodes.Add(val);
            }
        }
        if (this.Env.SearchCount<WebsiteSaleLoyalty.LoyaltyCard>(r => r.Code.In(mappedCodes)) > 0) {
            throw new ValidationError("A coupon with the same code was found.");
        }
    }
}
