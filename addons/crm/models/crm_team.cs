csharp
public partial class Team
{
    public void OnchangeUseLeadsOpportunities()
    {
        if (!UseLeads && !UseOpportunities)
        {
            AliasName = false;
        }
    }

    public void ConstrainsAssignmentDomain()
    {
        try
        {
            var domain = Env.SafeEval(AssignmentDomain ?? "[]");
            if (domain != null && domain.Any())
            {
                Env["Crm.Lead"].Search(domain, limit: 1);
            }
        }
        catch (Exception)
        {
            throw new ValidationException($"Assignment domain for team {Name} is incorrectly formatted");
        }
    }

    public override async Task<bool> WriteAsync(IDictionary<string, object> vals)
    {
        var result = await base.WriteAsync(vals);
        if (vals.ContainsKey(nameof(UseLeads)) || vals.ContainsKey(nameof(UseOpportunities)))
        {
            var aliasVals = await AliasGetCreationValuesAsync();
            await WriteAsync(new Dictionary<string, object>
            {
                { nameof(AliasName), aliasVals.GetValueOrDefault("alias_name", AliasName) },
                { nameof(AliasDefaults), aliasVals.GetValueOrDefault("alias_defaults") }
            });
        }
        return result;
    }

    public override async Task UnlinkAsync()
    {
        // When unlinking, concatenate `crm.lead.scoring.frequency` linked to
        // the team into "no team" statistics.
        var frequencies = await Env["Crm.LeadScoringFrequency"].SearchAsync(new List<object> { new List<object> { nameof(TeamId), "in", new List<int> { Id } } });
        if (frequencies.Any())
        {
            var existingNoteam = await Env["Crm.LeadScoringFrequency"].SearchAsync(new List<object>
            {
                new List<object> { nameof(TeamId), "=", false },
                new List<object> { nameof(Variable), "in", frequencies.Select(f => f.Variable).ToList() }
            });

            foreach (var frequency in frequencies)
            {
                // Skip void-like values
                if (Env.FloatCompare(frequency.WonCount, 0.1m, 2) != 1 && Env.FloatCompare(frequency.LostCount, 0.1m, 2) != 1)
                {
                    continue;
                }

                var match = existingNoteam.FirstOrDefault(f => f.Variable == frequency.Variable && f.Value == frequency.Value);
                if (match != null)
                {
                    // Remove extra .1 that may exist in db as those are artifacts of initializing
                    // frequency table. Final value of 0 will be set to 0.1.
                    var existWonCount = Env.FloatRound(match.WonCount, precisionDigits: 0, roundingMethod: RoundingMethod.HalfUp);
                    var existLostCount = Env.FloatRound(match.LostCount, precisionDigits: 0, roundingMethod: RoundingMethod.HalfUp);
                    var addWonCount = Env.FloatRound(frequency.WonCount, precisionDigits: 0, roundingMethod: RoundingMethod.HalfUp);
                    var addLostCount = Env.FloatRound(frequency.LostCount, precisionDigits: 0, roundingMethod: RoundingMethod.HalfUp);
                    var newWonCount = existWonCount + addWonCount;
                    var newLostCount = existLostCount + addLostCount;
                    match.WonCount = Env.FloatCompare(newWonCount, 0.1m, 2) == 1 ? newWonCount : 0.1m;
                    match.LostCount = Env.FloatCompare(newLostCount, 0.1m, 2) == 1 ? newLostCount : 0.1m;
                }
                else
                {
                    existingNoteam.Add(await Env["Crm.LeadScoringFrequency"].CreateAsync(new Dictionary<string, object>
                    {
                        { nameof(LostCount), Env.FloatCompare(frequency.LostCount, 0.1m, 2) == 1 ? frequency.LostCount : 0.1m },
                        { nameof(TeamId), false },
                        { nameof(Value), frequency.Value },
                        { nameof(Variable), frequency.Variable },
                        { nameof(WonCount), Env.FloatCompare(frequency.WonCount, 0.1m, 2) == 1 ? frequency.WonCount : 0.1m }
                    }));
                }
            }
        }
        await base.UnlinkAsync();
    }

    public Dictionary<string, object> AliasGetCreationValues()
    {
        var values = base.AliasGetCreationValues();
        values["alias_model_id"] = Env["Ir.Model"].Search(new List<object> { new List<object> { nameof(Model), "=", "Crm.Lead" } }, limit: 1).Id;
        if (Id != 0)
        {
            if (!UseLeads && !UseOpportunities)
            {
                values["alias_name"] = false;
            }
            values["alias_defaults"] = Env.SafeEval(AliasDefaults ?? "{}");
            var hasGroupUseLead = Env.User.HasGroup("Crm.GroupUseLead");
            ((Dictionary<string, object>)values["alias_defaults"])["type"] = hasGroupUseLead && UseLeads ? "lead" : "opportunity";
            ((Dictionary<string, object>)values["alias_defaults"])["team_id"] = Id;
        }
        return values;
    }

    // Additional methods would be implemented here...
}
