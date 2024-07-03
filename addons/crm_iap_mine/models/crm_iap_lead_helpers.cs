csharp
public partial class IapLeadHelpers
{
    public void NotifyNoMoreCredit(string serviceName, string modelName, string notificationParameter)
    {
        var alreadyNotified = Env.IrConfigParameter.GetParam(notificationParameter, false);
        if (alreadyNotified)
        {
            return;
        }

        var mailTemplate = Env.Ref("crm_iap_mine.lead_generation_no_credits");
        var iapAccount = Env.IapAccount.Search(new[] { ("ServiceName", "=", serviceName) }, limit: 1).FirstOrDefault();

        var res = Env[modelName].SearchRead(new object[] { }, new[] { "CreateUid" });
        var uids = new HashSet<int>(res.Where(r => r.ContainsKey("CreateUid")).Select(r => (int)r["CreateUid"]));

        res = Env.ResUsers.SearchRead(new[] { ("Id", "in", uids.ToList()) }, new[] { "Email" });
        var emails = new HashSet<string>(res.Where(r => r.ContainsKey("Email")).Select(r => (string)r["Email"]));

        var emailValues = new Dictionary<string, object>
        {
            { "EmailTo", string.Join(",", emails) }
        };

        mailTemplate.SendMail(iapAccount.Id, forceSet: true, emailValues: emailValues);
        Env.IrConfigParameter.SetParam(notificationParameter, true);
    }

    public Dictionary<string, object> LeadValsFromResponse(string leadType, int teamId, List<int> tagIds, int userId, Dictionary<string, object> companyData, List<Dictionary<string, object>> peopleData)
    {
        var countryId = Env.ResCountry.Search(new[] { ("Code", "=", companyData["country_code"]) }).FirstOrDefault()?.Id ?? 0;
        var websiteUrl = companyData.ContainsKey("domain") && !string.IsNullOrEmpty((string)companyData["domain"]) ? $"https://www.{companyData["domain"]}" : false;

        var leadVals = new Dictionary<string, object>
        {
            { "Type", leadType },
            { "TeamId", teamId },
            { "TagIds", new[] { 6, 0, tagIds } },
            { "UserId", userId },
            { "RevealId", companyData["clearbit_id"] },
            { "Name", companyData.ContainsKey("name") ? companyData["name"] : companyData["domain"] },
            { "PartnerName", companyData.ContainsKey("legal_name") ? companyData["legal_name"] : companyData["name"] },
            { "EmailFrom", ((List<string>)companyData["email"]).FirstOrDefault() ?? "" },
            { "Phone", companyData.ContainsKey("phone") ? companyData["phone"] : (((List<string>)companyData["phone_numbers"]).Any() ? ((List<string>)companyData["phone_numbers"])[0] : "") },
            { "Website", websiteUrl },
            { "Street", companyData["location"] },
            { "City", companyData["city"] },
            { "Zip", companyData["postal_code"] },
            { "CountryId", countryId },
            { "StateId", FindStateId((string)companyData["state_code"], countryId) }
        };

        if (peopleData != null && peopleData.Any())
        {
            leadVals["ContactName"] = peopleData[0]["full_name"];
            leadVals["EmailFrom"] = peopleData[0]["email"];
            leadVals["Function"] = peopleData[0]["title"];
        }

        return leadVals;
    }

    private int? FindStateId(string stateCode, int countryId)
    {
        var stateId = Env.ResCountryState.Search(new[] { ("Code", "=", stateCode), ("CountryId", "=", countryId) }).FirstOrDefault();
        return stateId?.Id;
    }
}
