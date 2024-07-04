csharp
public partial class BaseCompany {
    public BaseCompany() { }

    public virtual void Copy(Dictionary<string, object> defaultValues = null) {
        throw new Exception("Duplicating a company is not allowed. Please create a new company instead.");
    }

    public virtual byte[] GetLogo() {
        byte[] logoBytes;
        using (var stream = System.IO.File.OpenRead("base/static/img/res_company_logo.png")) {
            logoBytes = new byte[stream.Length];
            stream.Read(logoBytes, 0, (int)stream.Length);
        }
        return Convert.ToBase64String(logoBytes);
    }

    public virtual ResCurrency GetDefaultCurrency() {
        return Env.User.Company.Currency;
    }

    public virtual void Init() {
        var companies = Env.Model<BaseCompany>().Search(c => c.Paperformat == null);
        foreach (var company in companies) {
            var paperformatEuro = Env.Ref<ReportPaperformat>("base.paperformat_euro");
            if (paperformatEuro != null) {
                company.Paperformat = paperformatEuro;
            }
        }
    }

    public virtual List<string> GetCompanyRootDelegatedFieldNames() {
        return new List<string> { "Currency" };
    }

    public virtual List<string> GetCompanyAddressFieldNames() {
        return new List<string> { "Street", "Street2", "City", "Zip", "State", "Country" };
    }

    public virtual Dictionary<string, object> GetCompanyAddressUpdate(ResPartner partner) {
        var companyAddressFields = GetCompanyAddressFieldNames();
        return companyAddressFields.ToDictionary(fname => fname, fname => partner.GetValueOrDefault<object>(fname));
    }

    public virtual void ComputeParentIds() {
        foreach (var company in this.WithContext("active_test", false)) {
            if (company.ParentPath != null) {
                company.ParentIds = company.ParentPath.Split('/').Where(id => !string.IsNullOrEmpty(id)).Select(id => Convert.ToInt32(id)).ToList();
                company.Root = company.ParentIds.FirstOrDefault();
            } else {
                company.ParentIds = new List<int> { company.Id };
                company.Root = company;
            }
        }
    }

    public virtual void ComputeAddress() {
        foreach (var company in this.Where(c => c.Partner != null)) {
            var addressData = company.Partner.GetAddress("contact");
            if (addressData.ContainsKey("contact")) {
                var partner = Env.Model<ResPartner>().Browse(addressData["contact"]);
                company.Update(GetCompanyAddressUpdate(partner));
            }
        }
    }

    public virtual void InverseStreet() {
        foreach (var company in this) {
            company.Partner.Street = company.Street;
        }
    }

    public virtual void InverseStreet2() {
        foreach (var company in this) {
            company.Partner.Street2 = company.Street2;
        }
    }

    public virtual void InverseZip() {
        foreach (var company in this) {
            company.Partner.Zip = company.Zip;
        }
    }

    public virtual void InverseCity() {
        foreach (var company in this) {
            company.Partner.City = company.City;
        }
    }

    public virtual void InverseState() {
        foreach (var company in this) {
            company.Partner.State = company.State;
        }
    }

    public virtual void InverseCountry() {
        foreach (var company in this) {
            company.Partner.Country = company.Country;
        }
    }

    public virtual void ComputeLogoWeb() {
        foreach (var company in this) {
            if (company.Partner.Image1920 != null) {
                var img = Convert.FromBase64String(company.Partner.Image1920);
                company.LogoWeb = Convert.ToBase64String(Tools.ImageProcess(img, 180, 0));
            } else {
                company.LogoWeb = null;
            }
        }
    }

    public virtual void ComputeUsesDefaultLogo() {
        var defaultLogo = GetLogo();
        foreach (var company in this) {
            company.UsesDefaultLogo = company.Logo == null || company.Logo == defaultLogo;
        }
    }

    public virtual void ComputeColor() {
        foreach (var company in this) {
            company.Color = company.Root.Partner.Color ?? (company.Root.Id % 12);
        }
    }

    public virtual void InverseColor() {
        foreach (var company in this) {
            company.Root.Partner.Color = company.Color;
        }
    }

    public virtual void OnChangeState() {
        if (this.State != null) {
            this.Country = this.State.Country;
        }
    }

    public virtual void OnChangeCountry() {
        if (this.Country != null) {
            this.Currency = this.Country.Currency;
        }
    }

    public virtual void OnChangeParent() {
        if (this.Parent != null) {
            foreach (var fname in GetCompanyRootDelegatedFieldNames()) {
                if (this.GetValueOrDefault<object>(fname) != this.Parent.GetValueOrDefault<object>(fname)) {
                    this[fname] = this.Parent[fname];
                }
            }
        }
    }

    public virtual void ComputeUninstalledL10nModuleIds() {
        Env.Model<IrModuleModule>().FlushModel("auto_install", "country_ids", "dependencies_id");
        Env.Model<IrModuleModuleDependency>().FlushModel();
        Env.Cr.Execute(
            @"
            SELECT country.Id,
                   ARRAY_AGG(module.Id)
              FROM ir_module_module module,
                   res_country country
             WHERE module.auto_install
               AND state NOT IN ('installed', 'to install', 'to upgrade')
               AND NOT EXISTS (
                       SELECT 1
                         FROM ir_module_module_dependency d
                         JOIN ir_module_module mdep ON (d.name = mdep.name)
                        WHERE d.module_id = module.id
                          AND d.auto_install_required
                          AND mdep.state NOT IN ('installed', 'to install', 'to upgrade')
                   )
               AND EXISTS (
                       SELECT 1
                         FROM module_country mc
                        WHERE mc.module_id = module.id
                          AND mc.country_id = country.id
                   )
               AND country.Id = ANY(@country_ids)
          GROUP BY country.Id
        ",
            new Dictionary<string, object> {
                {"country_ids", this.Country.Id}
            }
        );
        var mapping = Env.Cr.FetchAll().ToDictionary(row => (int)row[0], row => (List<int>)row[1]);
        foreach (var company in this) {
            company.UninstalledL10nModuleIds = Env.Model<IrModuleModule>().Browse(mapping.GetValueOrDefault(company.Country.Id));
        }
    }

    public virtual IrModuleModule InstallL10nModules() {
        var uninstalledModules = this.UninstalledL10nModuleIds;
        var isReadyAndNotTest = !Tools.Config.TestEnable && (Env.Registry.Ready || !Env.Registry.IsInInit) && !Thread.CurrentThread.IsRunningTest;
        if (uninstalledModules.Count > 0 && isReadyAndNotTest) {
            return uninstalledModules.ButtonImmediateInstall();
        }
        return null;
    }

    public virtual Tuple<string, IrUiView> GetView(int? viewId = null, string viewType = "form", Dictionary<string, object> options = null) {
        var delegatedFnames = GetCompanyRootDelegatedFieldNames();
        var arch = Env.Model<BaseCompany>()._GetView(viewId, viewType, options).Item1;
        foreach (var f in arch.Elements("field")) {
            if (delegatedFnames.Contains(f.Attribute("name"))) {
                f.SetAttribute("readonly", "parent_id != False");
            }
        }
        return Tuple.Create(arch.ToString(), Env.Model<BaseCompany>()._GetView(viewId, viewType, options).Item2);
    }

    public virtual List<BaseCompany> NameSearch(string name, List<object> domain = null, string operator_ = "ilike", int? limit = null, string order = null) {
        var context = new Dictionary<string, object>(Env.Context);
        if (context.ContainsKey("user_preference")) {
            context.Remove("user_preference");
            // Browse as superuser to allow selecting any company
            var companies = Env.User.CompanyIds;
            if (domain == null) {
                domain = new List<object>();
            }
            domain.Add(new List<object> { "Id", "in", companies.Select(c => c.Id).ToList() });
            return this.WithUser("admin").NameSearch(name, domain, operator_, limit, order);
        } else {
            return this.WithContext(context).NameSearch(name, domain, operator_, limit, order);
        }
    }

    public virtual int CompanyDefaultGet(object obj = null, string field = null) {
        // Deprecated
        Env.Log.Warning("The method '_company_default_get' on res.company is deprecated and shouldn't be used anymore");
        return Env.Company.Id;
    }

    public virtual void ComputeEmptyCompanyDetails() {
        foreach (var record in this) {
            record.IsCompanyDetailsEmpty = string.IsNullOrEmpty(Tools.Html2PlainText(record.CompanyDetails));
        }
    }

    public virtual List<BaseCompany> Create(List<Dictionary<string, object>> valsList) {
        var noPartnerValsList = valsList.Where(vals => vals.ContainsKey("Name") && !vals.ContainsKey("Partner")).ToList();
        if (noPartnerValsList.Count > 0) {
            var partners = Env.Model<ResPartner>().Create(noPartnerValsList.Select(vals => new Dictionary<string, object> {
                { "Name", vals["Name"] },
                { "IsCompany", true },
                { "Image1920", vals.GetValueOrDefault("Logo") },
                { "Email", vals.GetValueOrDefault("Email") },
                { "Phone", vals.GetValueOrDefault("Phone") },
                { "Website", vals.GetValueOrDefault("Website") },
                { "Vat", vals.GetValueOrDefault("Vat") },
                { "Country", vals.GetValueOrDefault("Country") }
            }).ToList());
            partners.FlushModel();
            for (var i = 0; i < noPartnerValsList.Count; i++) {
                noPartnerValsList[i]["Partner"] = partners[i].Id;
            }
        }
        foreach (var vals in valsList) {
            if (vals.ContainsKey("Parent")) {
                var parent = Env.Model<BaseCompany>().Browse(vals["Parent"]);
                foreach (var fname in GetCompanyRootDelegatedFieldNames()) {
                    if (!vals.ContainsKey(fname)) {
                        vals[fname] = parent.GetValueOrDefault<object>(fname);
                    }
                }
            }
        }
        Env.Registry.ClearCache();
        var companies = Env.Model<BaseCompany>().Create(valsList);
        if (companies.Count > 0) {
            (Env.User | Env.Model<ResUsers>().Browse(Env.SuperuserId)).Update(new Dictionary<string, object> {
                { "CompanyIds", companies.Select(company => new Command.Link(company.Id)).ToList() }
            });
        }
        companies.Where(c => c.Currency != null).ForEach(c => c.Currency.Active = true);
        var companiesNeedsL10n = companies.Where(c => c.Country != null).ToList();
        if (companiesNeedsL10n.Count > 0) {
            companiesNeedsL10n.ForEach(c => c.InstallL10nModules());
        }
        return companies;
    }

    public virtual List<string> CacheInvalidationFields() {
        return new List<string> { "Active", "Sequence" };
    }

    public virtual void Write(Dictionary<string, object> values) {
        var invalidationFields = CacheInvalidationFields();
        var assetInvalidationFields = new List<string> { "Font", "PrimaryColor", "SecondaryColor", "ExternalReportLayout" };
        var companiesNeedsL10n = (values.ContainsKey("Country") && this.Where(c => c.Country == null).Any() || this).ToList();
        if (invalidationFields.Intersect(values.Keys).Any()) {
            Env.Registry.ClearCache();
        }
        if (assetInvalidationFields.Intersect(values.Keys).Any()) {
            Env.Registry.ClearCache("assets");
        }
        if (values.ContainsKey("Parent")) {
            throw new Exception("The company hierarchy cannot be changed.");
        }
        if (values.ContainsKey("Currency")) {
            var currency = Env.Model<ResCurrency>().Browse(values["Currency"]);
            if (!currency.Active) {
                currency.Active = true;
            }
        }
        var res = Env.Model<BaseCompany>().Write(values);
        if (values.ContainsKey("Active") && !Convert.ToBoolean(values["Active"])) {
            this.Where(c => c.Children != null).ForEach(c => c.Active = false);
        }
        foreach (var company in this) {
            if (!company.Parent.HasValue && (values.Keys.Intersect(GetCompanyRootDelegatedFieldNames()).Any())) {
                var branches = Env.Model<BaseCompany>().Search(new List<object> {
                    new List<object> { "Id", "child_of", company.Id },
                    new List<object> { "Id", "!=", company.Id }
                });
                foreach (var fname in values.Keys.Intersect(GetCompanyRootDelegatedFieldNames()).OrderBy(k => k)) {
                    branches[fname] = company[fname];
                }
            }
        }
        if (companiesNeedsL10n.Count > 0) {
            companiesNeedsL10n.ForEach(c => c.InstallL10nModules());
        }
        var companyAddressFields = GetCompanyAddressFieldNames();
        var companyAddressFieldsUpd = companyAddressFields.Intersect(values.Keys).ToList();
        if (companyAddressFieldsUpd.Count > 0) {
            this.InvalidateModel(companyAddressFields);
        }
    }

    public virtual void CheckActive() {
        foreach (var company in this) {
            if (!company.Active) {
                var companyActiveUsers = Env.Model<ResUsers>().SearchCount(new List<object> {
                    new List<object> { "CompanyId", "=", company.Id },
                    new List<object> { "Active", "=", true }
                });
                if (companyActiveUsers > 0) {
                    throw new ValidationException($"The company {company.Name} cannot be archived because it is still used as the default company of {companyActiveUsers} users.");
                }
            }
        }
    }

    public virtual void CheckRootDelegatedFields() {
        foreach (var company in this) {
            if (company.Parent != null) {
                foreach (var fname in company.GetCompanyRootDelegatedFieldNames()) {
                    if (company.GetValueOrDefault<object>(fname) != company.Parent.GetValueOrDefault<object>(fname)) {
                        var description = Env.Model<IrModelFields>()._Get("res.company", fname).FieldDescription;
                        throw new ValidationException($"The {description} of a subsidiary must be the same as it's root company.");
                    }
                }
            }
        }
    }

    public virtual BaseCompany GetMainCompany() {
        try {
            return Env.Ref<BaseCompany>("base.main_company");
        } catch (Exception) {
            return Env.Model<BaseCompany>().Search([], 1, "Id").FirstOrDefault();
        }
    }

    public virtual List<int> GetAccessibleBranches() {
        // Get branches of this company that the current user can use
        if (this.Count != 1) {
            throw new Exception("This method should be called on a single company object.");
        }
        var accessibleBranchIds = new List<int>();
        var accessible = Env.Companies;
        var current = this.WithUser("admin");
        while (current.Count > 0) {
            accessibleBranchIds.AddRange((current.Intersect(accessible)).Select(c => c.Id).ToList());
            current = current.Children;
        }
        if (accessibleBranchIds.Count == 0 && Env.Uid == Env.SuperuserId) {
            // Accessible companies will always be the same for super user when called in a cron.
            // Because of that, the intersection between them and self might be empty. The super user anyway always has
            // access to all companies (as it bypasses the record rules), so we return the current company in this case.
            return new List<int> { this.Id };
        }
        return accessibleBranchIds;
    }

    public virtual List<BaseCompany> AccessibleBranches() {
        return this.Browse(GetAccessibleBranches());
    }

    public virtual bool AllBranchesSelected() {
        // Return whether or all the branches of the companies in self are selected.
        // Is True if all the branches, and only those, are selected.
        // Can be used when some actions only make sense for whole companies regardless of the branches.
        return this == this.WithUser("admin").Search(new List<object> { new List<object> { "Id", "child_of", this.Root.Id } });
    }

    public virtual Dictionary<string, object> ActionAllCompanyBranches() {
        if (this.Count != 1) {
            throw new Exception("This method should be called on a single company object.");
        }
        return new Dictionary<string, object> {
            { "type", "ir.actions.act_window" },
            { "name", "Branches" },
            { "res_model", "Base.Company" },
            { "domain", new List<object> { new List<object> { "Parent", "=", this.Id } } },
            { "context", new Dictionary<string, object> { { "active_test", false }, { "default_parent_id", this.Id } } },
            { "views", new List<List<object>> { new List<object> { false, "tree" }, new List<object> { false, "kanban" }, new List<object> { false, "form" } } }
        };
    }
}
