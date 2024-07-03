csharp
public partial class UoM
{
    public string ComputeFiscalCountryCodes()
    {
        return string.Join(",", Env.Companies.Select(c => c.AccountFiscalCountryId?.Code).Where(c => c != null));
    }

    public string GetUneceCode()
    {
        var mapping = new Dictionary<string, string>
        {
            {"uom.product_uom_unit", "C62"},
            {"uom.product_uom_dozen", "DZN"},
            {"uom.product_uom_kgm", "KGM"},
            {"uom.product_uom_gram", "GRM"},
            {"uom.product_uom_day", "DAY"},
            {"uom.product_uom_hour", "HUR"},
            {"uom.product_uom_ton", "TNE"},
            {"uom.product_uom_meter", "MTR"},
            {"uom.product_uom_km", "KMT"},
            {"uom.product_uom_cm", "CMT"},
            {"uom.product_uom_litre", "LTR"},
            {"uom.product_uom_lb", "LBR"},
            {"uom.product_uom_oz", "ONZ"},
            {"uom.product_uom_inch", "INH"},
            {"uom.product_uom_foot", "FOT"},
            {"uom.product_uom_mile", "SMI"},
            {"uom.product_uom_floz", "OZA"},
            {"uom.product_uom_qt", "QT"},
            {"uom.product_uom_gal", "GLL"},
            {"uom.product_uom_cubic_meter", "MTQ"},
            {"uom.product_uom_cubic_inch", "INQ"},
            {"uom.product_uom_cubic_foot", "FTQ"},
        };

        var xmlIds = Env.GetExternalIds(this.Id);
        var matches = xmlIds.Intersect(mapping.Keys).ToList();

        return matches.Any() ? mapping[matches[0]] : "C62";
    }
}
