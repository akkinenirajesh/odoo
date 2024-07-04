csharp
public partial class BaseCountry
{
    public virtual void ComputeImageUrl()
    {
        if (this.Code == null || new List<string> { "AQ", "SJ" }.Contains(this.Code))
        {
            this.ImageUrl = null;
        }
        else
        {
            string code = new Dictionary<string, string> { { "GF", "fr" }, { "BV", "no" }, { "BQ", "nl" }, { "GP", "fr" }, { "HM", "au" }, { "YT", "fr" }, { "RE", "fr" }, { "MF", "fr" }, { "UM", "us" } }.GetValueOrDefault(this.Code, this.Code.ToLower());
            this.ImageUrl = $"/base/static/img/country_flags/{code}.png";
        }
    }

    public virtual void CheckAddressFormat()
    {
        if (this.AddressFormat != null)
        {
            List<string> addressFields = Env.GetModel("Res.Partner").GetAddressFields().ToList();
            addressFields.AddRange(new List<string> { "StateCode", "StateName", "CountryCode", "CountryName", "CompanyName" });
            try
            {
                string.Format(this.AddressFormat, addressFields.Select(f => 1).ToDictionary(i => i.Key, i => i.Value));
            }
            catch (Exception ex)
            {
                throw new UserError("The layout contains an invalid format key");
            }
        }
    }

    public virtual List<string> GetAddressFields()
    {
        return new List<string>();
    }

    public virtual void NameSearch(string name, List<object> domain = null, string operator = "ilike", int limit = 0, string order = null)
    {
        List<object> ids = new List<object>();
        if (name.Length == 2)
        {
            ids = Env.GetModel("Base.Country").Search(new List<object> { ["Code", operator, name] }.Concat(domain ?? new List<object>()).ToList(), limit, order);
        }

        List<object> searchDomain = new List<object> { ["Name", operator, name] };
        if (ids.Count > 0)
        {
            searchDomain.Add(["Id", "not in", ids]);
        }
        ids.AddRange(Env.GetModel("Base.Country").Search(searchDomain.Concat(domain ?? new List<object>()).ToList(), limit, order));
    }

    public virtual int PhoneCodeFor(string code)
    {
        return Env.GetModel("Base.Country").Search(new List<object> { ["Code", "=", code] }).PhoneCode;
    }

    public virtual void Create(List<Dictionary<string, object>> valsList)
    {
        foreach (var vals in valsList)
        {
            if (vals.ContainsKey("Code"))
            {
                vals["Code"] = vals["Code"].ToString().ToUpper();
            }
        }
        Env.GetModel("Base.Country").Create(valsList);
    }

    public virtual void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("Code"))
        {
            vals["Code"] = vals["Code"].ToString().ToUpper();
        }
        Env.GetModel("Base.Country").Write(vals);
        if (vals.ContainsKey("Code") || vals.ContainsKey("PhoneCode"))
        {
            // Intentionally simplified by not clearing the cache in create and unlink.
            Env.Registry.ClearCache();
        }
        if (vals.ContainsKey("AddressViewId"))
        {
            // Changing the address view of the company must invalidate the view cached for res.partner
            // because of _view_get_address
            Env.Registry.ClearCache("templates");
        }
    }
}
