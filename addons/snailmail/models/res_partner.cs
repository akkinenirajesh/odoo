C#
public partial class Snailmail.ResPartner {
    public void Write(Dictionary<string, object> vals) {
        Dictionary<string, object> letterAddressVals = new Dictionary<string, object>();
        string[] addressFields = new string[] { "Street", "Street2", "City", "Zip", "StateId", "CountryId" };
        foreach (string field in addressFields) {
            if (vals.ContainsKey(field)) {
                letterAddressVals.Add(field, vals[field]);
            }
        }

        if (letterAddressVals.Count > 0) {
            var letters = Env.GetModel("Snailmail.Letter").Search(new List<object[]> {
                new object[] { "State", "not in", new List<object> { "Sent", "Canceled" } },
                new object[] { "PartnerId", "in", this.Id } 
            });
            letters.Write(letterAddressVals);
        }

        base.Write(vals);
    }

    public string GetCountryName() {
        string countryCode = this.CountryId.Code;
        if (Env.Context.ContainsKey("SnailmailLayout") && SNAILMAIL_COUNTRIES.ContainsKey(countryCode)) {
            return SNAILMAIL_COUNTRIES[countryCode];
        }

        return base.GetCountryName();
    }

    public string GetAddressFormat() {
        if (Env.Context.ContainsKey("SnailmailLayout") && !string.IsNullOrEmpty(this.Street2)) {
            return "%(Street)s, %(Street2)s\n%(City)s %(StateCode)s %(Zip)s\n%(CountryName)s";
        }

        return base.GetAddressFormat();
    }
}
