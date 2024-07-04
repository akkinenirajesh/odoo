csharp
public partial class WebsiteProductPricelist {
  public WebsiteProductPricelist() { }
  public virtual int? WebsiteId { get; set; }
  public virtual string Code { get; set; }
  public virtual bool Selectable { get; set; }
  public virtual int? CompanyId { get; set; }
  public virtual Website Website { get; set; }
  public virtual Company Company { get; set; }

  public virtual Website DefaultWebsite { get { return Env.Get("Website").FirstOrDefault<Website>(x => x.CompanyId == this.CompanyId); } }
  public virtual void Create(Dictionary<string, object> vals) {
    if (vals.ContainsKey("CompanyId") && !vals.ContainsKey("WebsiteId")) {
      this = (WebsiteProductPricelist)Env.WithContext(new Dictionary<string, object>() { { "default_company_id", vals["CompanyId"] } }).Get("Website.ProductPricelist").Create(vals);
    }
    else {
      this = (WebsiteProductPricelist)Env.Get("Website.ProductPricelist").Create(vals);
    }
    Env.Registry.ClearCache();
  }
  public virtual void Write(Dictionary<string, object> data) {
    this = (WebsiteProductPricelist)this.Write(data);
    Env.Registry.ClearCache();
  }
  public virtual void Unlink() {
    this = (WebsiteProductPricelist)this.Unlink();
    Env.Registry.ClearCache();
  }
  public virtual List<WebsiteProductPricelist> GetPartnerPricelistMultiSearchDomainHook(int company_id) {
    var website = Env.Get("ir_http").GetWebRequestWebsite();
    var domain = base.GetPartnerPricelistMultiSearchDomainHook(company_id);
    if (website != null) {
      domain.AddRange(GetWebsitePricelistsDomain(website));
    }
    return domain.Select(x => x as WebsiteProductPricelist).ToList();
  }
  public virtual List<WebsiteProductPricelist> GetPartnerPricelistMultiFilterHook() {
    var website = Env.Get("ir_http").GetWebRequestWebsite();
    var res = base.GetPartnerPricelistMultiFilterHook();
    if (website != null) {
      res = res.Where(x => x.IsAvailableOnWebsite(website)).ToList();
    }
    return res;
  }
  public virtual bool IsAvailableOnWebsite(Website website) {
    if (this.CompanyId != null && this.CompanyId != website.CompanyId) {
      return false;
    }
    return this.Active && this.WebsiteId == website.Id || (this.WebsiteId == null && (this.Selectable || this.Code != null)) || this.CompanyId == null;
  }
  public virtual bool IsAvailableInCountry(string country_code) {
    if (country_code == null || this.CountryGroupIds == null) {
      return true;
    }
    return this.CountryGroupIds.CountryIds.Any(x => x.Code == country_code);
  }
  public virtual List<object> GetWebsitePricelistsDomain(Website website) {
    return new List<object>() {
      new { Active = true },
      new { CompanyId = new List<object>() { null, website.CompanyId } },
      new { Or = new List<object>() { new { WebsiteId = website.Id }, new { And = new List<object>() { new { WebsiteId = null }, new { Or = new List<object>() { new { Selectable = true }, new { Code = (object)new { Not = null } } } } } } }
    };
  }
}
