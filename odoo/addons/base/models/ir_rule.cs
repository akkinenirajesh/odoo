csharp
public partial class BaseIrRule {

    public BaseIrRule() {
        
    }

    public void ComputeGlobal() {
        this.Global = !this.Groups.Any();
    }

    public void CheckModelName() {
        // Don't allow rules on rules records (this model).
        if (Env.Model(this.Model.Model).Name == "Base.IrRule") {
            throw new Exception("Rules can not be applied on the Record Rules model.");
        }
    }

    public void CheckDomain() {
        if (this.Active && !string.IsNullOrWhiteSpace(this.DomainForce)) {
            try {
                // Evaluate domain
                var domain = Env.Eval<object[]>(this.DomainForce, this.EvalContext);
                // Check if domain is valid
                Env.Model(this.Model.Model).Search(domain);
            } catch (Exception ex) {
                throw new Exception("Invalid domain: " + ex.Message);
            }
        }
    }

    public IEnumerable<object> ComputeDomainContextValues() {
        yield return this.Env.Context.Get<List<int>>("allowed_company_ids").Select(x => (object)x);
    }

    public object[] EvalContext() {
        // Returns a dictionary to use as evaluation context for ir.rule domains.
        // Note: company_ids contains the ids of the activated companies by the user with the switch company menu. These companies are filtered and trusted.
        return new object[] {
            Env.User.WithContext(new Dictionary<string, object>()).Id,
            Env.Time,
            Env.Companies.Ids,
            Env.Company.Id
        };
    }

    public IEnumerable<BaseIrRule> GetFailing(IEnumerable<int> recordIds, string mode = "read") {
        // Returns the rules for the mode for the current user which fail on the specified records.
        // Can return any global rule and/or all local rules (since local rules are OR-ed together, the entire group succeeds or fails, while global rules get AND-ed and can each fail)
        if (mode != "read" && mode != "write" && mode != "create" && mode != "unlink") {
            throw new Exception("Invalid mode: " + mode);
        }

        if (Env.User.IsSuperUser) {
            return new List<BaseIrRule>();
        }

        // Get all rules for the model
        var allRules = GetRules(Env.Model(this.Model.Model).Name, mode).WithUser(Env.User);

        // Check group rules
        var groupRules = allRules.Where(r => r.Groups.Any() && r.Groups.Any(g => Env.User.Groups.Contains(g)));
        var groupDomains = groupRules.Select(r => Env.Eval<object[]>(r.DomainForce, EvalContext())).ToList();
        // If all records get returned, the group rules are not failing
        if (Env.Model(this.Model.Model).SearchCount(groupDomains.Concat(new List<object[]> { new object[] { "id", "in", recordIds } })) == recordIds.Count()) {
            groupRules = new List<BaseIrRule>();
        }

        // Failing rules are previously selected group rules or any failing global rule
        return allRules.Where(r => groupRules.Contains(r) || (!r.Groups.Any() && !IsFailing(r, recordIds)));
    }

    public bool IsFailing(BaseIrRule rule, IEnumerable<int> recordIds) {
        // Check if the rule fails for any of the given recordIds
        var domain = Env.Eval<object[]>(rule.DomainForce, EvalContext()).Concat(new List<object[]> { new object[] { "id", "in", recordIds } });
        return Env.Model(this.Model.Model).SearchCount(domain) < recordIds.Count();
    }

    public IEnumerable<BaseIrRule> GetRules(string modelName, string mode = "read") {
        // Returns all the rules matching the model for the mode for the current user.
        if (mode != "read" && mode != "write" && mode != "create" && mode != "unlink") {
            throw new Exception("Invalid mode: " + mode);
        }

        if (Env.User.IsSuperUser) {
            return new List<BaseIrRule>();
        }

        // Execute the query
        var query = string.Format("SELECT r.id FROM BaseIrRule r JOIN BaseIrModel m ON (r.Model=m.id) WHERE m.Model='{0}' AND r.Active AND r.Perm{1} AND (r.id IN (SELECT rule_group_id FROM rule_group_rel rg JOIN res_groups_users_rel gu ON (rg.group_id=gu.gid) WHERE gu.uid={2}) OR r.Global) ORDER BY r.id", modelName, mode, Env.User.Id);
        var result = Env.Execute(query);
        return result.Select(x => (BaseIrRule)Env.Ref(x));
    }

    public object[] ComputeDomain(string modelName, string mode = "read") {
        // Add rules for parent models
        foreach (var parent in Env.Model(modelName).Inherits) {
            var domain = ComputeDomain(parent.Key, mode);
            if (domain.Any()) {
                return new object[] { new object[] { parent.Value, "any", domain } };
            }
        }

        // Get all rules for the model
        var rules = GetRules(modelName, mode);
        if (!rules.Any()) {
            return new object[] { };
        }

        var evalContext = EvalContext();
        var userGroups = Env.User.Groups;
        var groupDomains = new List<object[]>();
        foreach (var rule in rules.WithUser(Env.User)) {
            var dom = Env.Eval<object[]>(rule.DomainForce, evalContext);
            if (!rule.Groups.Any()) {
                return new object[] { dom };
            } else if (rule.Groups.Any(g => userGroups.Contains(g))) {
                groupDomains.Add(dom);
            }
        }

        // Combine global domains and group domains
        if (!groupDomains.Any()) {
            return new object[] { };
        }
        return new object[] { new object[] { "AND", groupDomains.Select(d => new object[] { "OR", d }).ToList() } };
    }

    public void Unlink() {
        // Clear cache before deleting rules.
        Env.ClearCache();
        base.Unlink();
    }

    public void Create() {
        // Flush all data and clear cache after creating a rule.
        Env.Flush();
        Env.ClearCache();
    }

    public void Write() {
        // Flush all data and clear cache after writing to a rule.
        Env.Flush();
        Env.ClearCache();
    }

    public void MakeAccessError(string operation, IEnumerable<int> recordIds) {
        // Check if the user has access to the records based on the rules and current user context
        if (operation != "read" && operation != "write" && operation != "create" && operation != "unlink") {
            throw new Exception("Invalid operation: " + operation);
        }

        var records = Env.Model(this.Model.Model).Browse(recordIds);

        // Log access denied information
        Env.Logger.Info("Access Denied by record rules for operation: {0} on record ids: {1}, uid: {2}, model: {3}", operation, recordIds.Take(6).ToList(), Env.User.Id, records.Name);

        // Get the model description
        var modelDescription = Env.Model(records.Name).Description;

        // Get the user description
        var userDescription = $"{Env.User.Name} (id={Env.User.Id})";

        // Get the operation description
        var operationDescription = operation == "read" ? "read" : operation == "write" ? "write" : operation == "create" ? "create" : operation == "unlink" ? "delete" : "";

        // Generate error message
        var errorMessage = string.Format("Uh-oh! Looks like you have stumbled upon some top-secret records.\n\nSorry, {0} doesn't have '{1}' access to: \n- {2} ({3}: {4})", userDescription, operationDescription, modelDescription, records.Name, string.Join(", ", recordIds.Take(6).ToList()));

        // Get the failing rules
        var failingRules = GetFailing(recordIds, operation).WithUser(Env.User);

        // Get the record names
        var recordNames = records.Take(6).Select(r => r.DisplayName).ToList();

        // Check if the company ID is relevant for any rule
        var companyRelated = failingRules.Any(r => r.DomainForce.Contains("company_id"));

        // Generate additional error message based on company ID
        if (companyRelated) {
            var suggestedCompanies = records.GetRedirectSuggestedCompany();
            if (suggestedCompanies.Count != 1) {
                errorMessage += "\n\nNote: this might be a multi-company issue. Switching company may help - in Odoo, not in real life!";
            } else if (Env.User.Companies.Contains(suggestedCompanies.FirstOrDefault())) {
                errorMessage += "\n\nThis seems to be a multi-company issue, you might be able to access the record by switching to the company: " + suggestedCompanies.FirstOrDefault().DisplayName;
            } else {
                errorMessage += "\n\nThis seems to be a multi-company issue, but you do not have access to the proper company to access the record anyhow.";
            }
        }

        // Add information about failing rules if debug mode is enabled
        if (!Env.User.HasGroup("base.group_no_one") || !Env.User.IsInternal()) {
            errorMessage += "\n\nIf you really, really need access, perhaps you can win over your friendly administrator with a batch of freshly baked cookies.";
        } else {
            var failingRulesDescription = string.Join("\n- ", failingRules.Select(r => r.Name));
            errorMessage += string.Format("\n\nBlame the following rules: \n{0} \n\nIf you really, really need access, perhaps you can win over your friendly administrator with a batch of freshly baked cookies.", failingRulesDescription);
        }

        // Clean up the cache of records
        records.InvalidateRecordset();

        // Throw access error
        throw new Exception(errorMessage);
    }
}
