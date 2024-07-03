csharp
using System;
using System.Linq;
using System.Collections.Generic;

namespace Account
{
    public partial class AccountTag
    {
        public override string ToString()
        {
            if (!Env.Company.MultiVatForeignCountryIds.Any())
            {
                return Name;
            }

            if (Applicability == AccountTagApplicability.Taxes && Country != null && Country != Env.Company.AccountFiscalCountry)
            {
                return $"{Name} ({Country.Code})";
            }

            return Name;
        }

        public IEnumerable<AccountTag> GetTaxTags(string tagName, Core.Country country)
        {
            var domain = GetTaxTagsDomain(tagName, country);
            return Env.Set<AccountTag>().WithContext(new { active_test = false }).Search(domain);
        }

        public IEnumerable<object> GetTaxTagsDomain(string tagName, Core.Country country, string sign = null)
        {
            var escapedTagName = tagName.Replace("\\", "\\\\").Replace("%", @"\%").Replace("_", @"\_");
            return new List<object>
            {
                new[] { "Name", "=like", (sign ?? "_") + escapedTagName },
                new[] { "Country", "=", country },
                new[] { "Applicability", "=", AccountTagApplicability.Taxes }
            };
        }

        public IEnumerable<AccountReportExpression> GetRelatedTaxReportExpressions()
        {
            if (Env.IsEmpty(this))
            {
                return Enumerable.Empty<AccountReportExpression>();
            }

            var orDomains = new List<IEnumerable<object>>();
            foreach (var record in Env.Set<AccountTag>().Browse(this))
            {
                var exprDomain = new List<object>
                {
                    new[] { "ReportLine.Report.Country", "=", record.Country },
                    new[] { "Formula", "=", record.Name.Substring(1) }
                };
                orDomains.Add(exprDomain);
            }

            var domain = new List<object>
            {
                new[] { "Engine", "=", "tax_tags" },
                orDomains.Count > 1 ? new object[] { "|", orDomains } : orDomains.FirstOrDefault()
            };

            return Env.Set<AccountReportExpression>().Search(domain);
        }

        public void OnDeleteCheckMasterTags()
        {
            var masterXmlids = new[] 
            {
                "account_tag_operating",
                "account_tag_financing",
                "account_tag_investing"
            };

            foreach (var masterXmlid in masterXmlids)
            {
                var masterTag = Env.Ref<AccountTag>($"account.{masterXmlid}", false);
                if (masterTag != null && Equals(this, masterTag))
                {
                    throw new UserException($"You cannot delete this account tag ({Name}), it is used on the chart of account definition.");
                }
            }
        }
    }
}
