csharp
public partial class EventRegistration
{
    public IEnumerable<(CrmLead Lead, SaleOrder SaleOrder, IEnumerable<EventRegistration> Registrations)> GetLeadGrouping(
        IEnumerable<EventLeadRule> rules,
        Dictionary<EventLeadRule, IEnumerable<EventRegistration>> ruleToNewRegs)
    {
        var soRegistrations = this.Where(reg => reg.SaleOrder != null);
        var groupingRes = base.GetLeadGrouping(rules, ruleToNewRegs);

        if (soRegistrations.Any())
        {
            var relatedRegistrations = Env.EventRegistration.Search(new[]
            {
                ("SaleOrder", "in", soRegistrations.Select(r => r.SaleOrder.Id).ToArray())
            });

            var relatedLeads = Env.CrmLead.Search(new[]
            {
                ("EventLeadRule", "in", rules.Select(r => r.Id).ToArray()),
                ("Registrations", "in", relatedRegistrations.Select(r => r.Id).ToArray())
            });

            foreach (var rule in rules)
            {
                var ruleNewRegs = ruleToNewRegs[rule];

                var soToRegs = new Dictionary<SaleOrder, List<EventRegistration>>();
                foreach (var registration in ruleNewRegs.Intersect(soRegistrations))
                {
                    if (!soToRegs.ContainsKey(registration.SaleOrder))
                    {
                        soToRegs[registration.SaleOrder] = new List<EventRegistration>();
                    }
                    soToRegs[registration.SaleOrder].Add(registration);
                }

                var soRes = new List<(CrmLead, SaleOrder, IEnumerable<EventRegistration>)>();
                foreach (var kvp in soToRegs)
                {
                    var saleOrder = kvp.Key;
                    var registrations = kvp.Value.OrderBy(r => r.Id);
                    var leads = relatedLeads.Where(lead => lead.EventLeadRule == rule && lead.Registrations.Any(r => r.SaleOrder == saleOrder));
                    soRes.Add((leads.FirstOrDefault(), saleOrder, registrations));
                }

                if (soRes.Any())
                {
                    if (!groupingRes.ContainsKey(rule))
                    {
                        groupingRes[rule] = new List<(CrmLead, SaleOrder, IEnumerable<EventRegistration>)>();
                    }
                    groupingRes[rule].AddRange(soRes);
                }
            }
        }

        return groupingRes.SelectMany(kvp => kvp.Value);
    }
}
