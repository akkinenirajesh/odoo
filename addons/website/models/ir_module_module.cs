csharp
public partial class IrModuleModule {

    public bool ComputeIsInstalledOnCurrentWebsite() {
        return this == Env.Ref<Website.Website>("website").GetCurrentWebsite().ThemeId;
    }

    public void Write(Dictionary<string, object> vals) {
        if (Env.Request.Context.ContainsKey("apply_new_theme")) {
            this = this.WithContext("apply_new_theme", true);
        }

        foreach (var module in this) {
            if (module.Name.StartsWith("theme_") && vals.ContainsKey("State") && vals["State"].ToString() == "installed") {
                Env.Logger.Info($"Module {module.Name} has been loaded as theme template ({module.State})");

                if (module.State == "to install" || module.State == "to upgrade") {
                    var websitesToUpdate = module.GetThemeStreamWebsiteIds();

                    if (module.State == "to upgrade" && Env.Request != null) {
                        var website = Env.Ref<Website.Website>("website").GetCurrentWebsite();
                        websitesToUpdate = website.In(websitesToUpdate) ? website : Env.Ref<Website.Website>("website");
                    }

                    foreach (var website in websitesToUpdate) {
                        module.LoadTheme(website);
                    }
                }
            }
        }

        base.Write(vals);
    }

    public List<Website.Website> GetThemeStreamWebsiteIds() {
        var websites = Env.Ref<Website.Website>("website").Search(new[] {
            new[] { "ThemeId", "!=", null }
        });

        var result = new List<Website.Website>();
        foreach (var website in websites) {
            if (this.In(website.ThemeId.GetThemeStreamThemes())) {
                result.Add(website);
            }
        }

        return result;
    }

    public void LoadTheme(Website.Website website) {
        Env.Logger.Info($"Load theme {this.Name} for website {website.Id} from template.");

        foreach (var modelName in new[] {
            "IrUiView",
            "IrAsset",
            "WebsitePage",
            "WebsiteMenu",
            "IrAttachment"
        }) {
            this.UpdateRecords(modelName, website);
        }

        if (Env.Request.Context.ContainsKey("apply_new_theme")) {
            // Both the theme install and upgrade flow ends up here.
            // The _post_copy() is supposed to be called only when the theme
            // is installed for the first time on a website.
            // It will basically select some header and footer template.
            // We don't want the system to select again the theme footer or
            // header template when that theme is updated later. It could
            // erase the change the user made after the theme install.
            Env.Ref<ThemeUtils.ThemeUtils>("theme.utils").WithContext("website_id", website.Id).PostCopy(this);
        }
    }

    public List<IrModuleModule> GetThemeUpstream() {
        return this.UpstreamDependencies(new[] { "" }).Where(x => x.Name.StartsWith("theme_")).ToList();
    }

    public List<IrModuleModule> GetThemeDownstream() {
        return this.DownstreamDependencies().Where(x => x.Name.StartsWith(this.Name)).ToList();
    }

    public List<IrModuleModule> GetThemeStreamThemes() {
        var allMods = this.Concat(this.GetThemeDownstream()).ToList();

        foreach (var downMod in this.GetThemeDownstream().Concat(new[] { this }).ToList()) {
            foreach (var upMod in downMod.GetThemeUpstream()) {
                allMods = upMod.Union(allMods).ToList();
            }
        }

        return allMods;
    }

    public void UnloadTheme(Website.Website website) {
        Env.Logger.Info($"Unload theme {this.Name} for website {website.Id} from template.");

        foreach (var modelName in new[] {
            "IrUiView",
            "IrAsset",
            "WebsitePage",
            "WebsiteMenu",
            "IrAttachment"
        }) {
            var template = this.GetModuleData(modelName);
            var models = template.WithContext(new[] {
                new[] { "active_test", false },
                new[] { "MODULE_UNINSTALL_FLAG", true }
            }).SelectMany(m => m.CopyIds).Where(m => m.WebsiteId == website).ToList();
            models.ForEach(m => m.Unlink());
            this.CleanupTheme(modelName, website);
        }
    }

    public void CleanupTheme(string modelName, Website.Website website) {
        var model = Env.Ref<object>(modelName);

        if (modelName == "WebsitePage" || modelName == "WebsiteMenu") {
            return;
        }

        // use active_test to also unlink archived models
        // and use MODULE_UNINSTALL_FLAG to also unlink inherited models
        var orphans = model.WithContext(new[] {
            new[] { "active_test", false },
            new[] { "MODULE_UNINSTALL_FLAG", true }
        }).Search(new[] {
            new[] { "Key", "=like", $"{this.Name}.%" },
            new[] { "WebsiteId", "=", website.Id },
            new[] { "ThemeTemplateId", "=", null }
        }).ToList();
        orphans.ForEach(m => m.Unlink());
    }

    public void UpgradeUpstreamThemes() {
        void InstallOrUpgrade(IrModuleModule theme) {
            if (theme.State != "installed") {
                theme.ButtonInstall();
            }

            var themes = theme.Concat(theme.GetThemeUpstream()).ToList();
            themes.Where(m => m.State == "installed").ToList().ForEach(m => m.ButtonUpgrade());
        }

        this.ButtonImmediateFunction(InstallOrUpgrade);
    }

    public void RemoveTheme(Website.Website website) {
        // _theme_remove is the entry point of any change of theme for a website
        // (either removal or installation of a theme and its dependencies). In
        // either case, we need to reset some default configuration before.
        Env.Ref<ThemeUtils.ThemeUtils>("theme.utils").WithContext("website_id", website.Id).ResetDefaultConfig();

        if (website.ThemeId == null) {
            return;
        }

        foreach (var theme in website.ThemeId.GetThemeStreamThemes().Reverse()) {
            theme.UnloadTheme(website);
        }

        website.ThemeId = null;
    }

    public Dictionary<string, object> ButtonChooseTheme() {
        var website = Env.Ref<Website.Website>("website").GetCurrentWebsite();

        this.RemoveTheme(website);

        // website.theme_id must be set before upgrade/install to trigger the load in ``write``
        website.ThemeId = this;

        // this will install 'self' if it is not installed yet
        if (Env.Request != null) {
            Env.Request.UpdateContext("apply_new_theme", true);
        }

        this.UpgradeUpstreamThemes();

        var result = website.ButtonGoWebsite();
        result.Add("context", new Dictionary<string, object>() {
            { "params", new Dictionary<string, object>() { { "with_loader", true } } }
        });

        return result;
    }

    public void ButtonRemoveTheme() {
        var website = Env.Ref<Website.Website>("website").GetCurrentWebsite();
        this.RemoveTheme(website);
    }

    public void ButtonRefreshTheme() {
        var website = Env.Ref<Website.Website>("website").GetCurrentWebsite();
        website.ThemeId.UpgradeUpstreamThemes();
    }

    public List<IrUiView> GetModuleData(string modelName) {
        var themeModelName = new Dictionary<string, string>() {
            { "IrUiView", "ThemeIrUiView" },
            { "IrAsset", "ThemeIrAsset" },
            { "WebsitePage", "ThemeWebsitePage" },
            { "WebsiteMenu", "ThemeWebsiteMenu" },
            { "IrAttachment", "ThemeIrAttachment" },
        }[modelName];

        var irModelData = Env.Ref<IrModelData.IrModelData>("ir.model.data");
        var records = Env.Ref<object>(themeModelName);

        foreach (var module in this) {
            var imdIds = irModelData.Search(new[] {
                new[] { "Module", "=", module.Name },
                new[] { "Model", "=", themeModelName }
            }).Select(x => x.ResId).ToList();
            records = records.WithContext(new[] {
                new[] { "active_test", false }
            }).Where(x => imdIds.Contains(x.Id)).ToList();
        }

        return records.Cast<IrUiView>().ToList();
    }

    public void UpdateRecords(string modelName, Website.Website website) {
        var remaining = this.GetModuleData(modelName);
        int lastLen = -1;
        while (remaining.Count != lastLen) {
            lastLen = remaining.Count;

            foreach (var rec in remaining) {
                var recData = rec.ConvertToBaseModel(website);

                if (recData == null) {
                    Env.Logger.Info($"Record queued: {rec.DisplayName}");
                    continue;
                }

                var find = rec.WithContext(new[] {
                    new[] { "active_test", false }
                }).SelectMany(x => x.CopyIds).Where(m => m.WebsiteId == website).ToList();

                // special case for attachment
                // if module B override attachment from dependence A, we update it
                if (modelName == "IrAttachment" && find.Count == 0) {
                    // In master, a unique constraint over (theme_template_id, website_id)
                    // will be introduced, thus ensuring unicity of 'find'
                    find = rec.CopyIds.Search(new[] {
                        new[] { "Key", "=", rec.Key },
                        new[] { "WebsiteId", "=", website.Id },
                        new[] { "OriginalId", "=", false }
                    }).ToList();
                }

                if (find.Count > 0) {
                    var imd = Env.Ref<IrModelData.IrModelData>("ir.model.data").Search(new[] {
                        new[] { "Model", "=", find[0].Name },
                        new[] { "ResId", "=", find[0].Id }
                    });

                    if (imd != null && imd.Noupdate) {
                        Env.Logger.Info($"Noupdate set for {find[0]} ({imd})");
                    } else {
                        // at update, ignore active field
                        if (recData.ContainsKey("Active")) {
                            recData.Remove("Active");
                        }

                        if (modelName == "IrUiView" && (find[0].ArchUpdated || find[0].Arch == recData["Arch"].ToString())) {
                            recData.Remove("Arch");
                        }

                        find[0].Update(recData);
                        this.PostCopy(rec, find[0]);
                    }
                } else {
                    var newRec = Env.Ref<object>(modelName).Create(recData);
                    this.PostCopy(rec, newRec);
                }

                remaining.Remove(rec);
            }
        }

        if (remaining.Count > 0) {
            var error = $"Error - Remaining: {string.Join(", ", remaining.Select(x => x.DisplayName))}";
            Env.Logger.Error(error);
            throw new Exception(error);
        }

        this.CleanupTheme(modelName, website);
    }

    public void PostCopy(object oldRec, object newRec) {
        var translatedFields = new Dictionary<string, List<string>>() {
            { "ThemeIrUiView", new List<string>() { "ThemeIrUiView,Arch", "IrUiView,ArchDb" } },
            { "ThemeWebsiteMenu", new List<string>() { "ThemeWebsiteMenu,Name", "WebsiteMenu,Name" } },
        }[oldRec.GetType().Name];

        var curLang = Env.Lang ?? "en_US";
        var validLangs = Env.Ref<ResLang.ResLang>("res.lang").GetInstalled().Select(x => x.Code).ToList();
        validLangs.Add("en_US");

        oldRec.FlushRecordset();
        foreach (var (srcField, dstField) in translatedFields) {
            var srcFname = srcField.Split(',')[1];
            var dstMname = dstField.Split(',')[0];
            var dstFname = dstField.Split(',')[1];

            if (dstMname != newRec.GetType().Name) {
                continue;
            }

            var oldField = oldRec.GetType().GetField(srcFname);
            var oldStoredTranslations = oldField.GetValue(oldRec) as Dictionary<string, object>;

            if (oldStoredTranslations == null) {
                continue;
            }

            var newField = newRec.GetType().GetField(dstFname);
            if (newField.GetCustomAttribute<TranslateAttribute>() != null) {
                if (oldRec.GetType().GetField(srcFname).GetValue(oldRec) != newRec.GetType().GetField(dstFname).GetValue(newRec)) {
                    continue;
                }

                var translations = oldStoredTranslations.Where(x => validLangs.Contains(x.Key) && x.Key != curLang).ToDictionary(x => x.Key, x => x.Value);
                newField.SetValue(newRec, translations);
            } else {
                var oldTranslations = oldStoredTranslations.Where(x => validLangs.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                var translationDictionary = newField.GetCustomAttribute<TranslationDictionaryAttribute>().GetTranslationDictionary(
                    oldTranslations.TryGetValue(curLang, out var enTranslation) ? enTranslation : oldTranslations["en_US"],
                    oldTranslations.Where(x => x.Key != curLang).ToDictionary(x => x.Key, x => x.Value));
                var translations = new Dictionary<string, Dictionary<string, object>>();
                foreach (var (fromLangTerm, toLangTerms) in translationDictionary) {
                    foreach (var (lang, toLangTerm) in toLangTerms) {
                        if (translations.ContainsKey(lang)) {
                            translations[lang].Add(fromLangTerm, toLangTerm);
                        } else {
                            translations.Add(lang, new Dictionary<string, object>() { { fromLangTerm, toLangTerm } });
                        }
                    }
                }
                newField.SetValue(newRec, translations);
            }
        }
    }

    public void UpdateList() {
        base.UpdateList();
        this.UpdateThemeImages();
    }

    public void UpdateThemeImages() {
        var irAttachment = Env.Ref<IrAttachment.IrAttachment>("ir.attachment");
        var existingUrls = irAttachment.SearchRead(new[] {
            new[] { "ResModel", "=", "Website.IrModuleModule" },
            new[] { "Type", "=", "url" }
        }, new[] { "Url" }).Select(x => x.Url.ToString()).ToList();

        var themes = Env.Ref<IrModuleModule.IrModuleModule>("ir.module.module").WithContext(new[] {
            new[] { "active_test", false }
        }).Search(new[] {
            new[] { "CategoryId", "child_of", Env.Ref<IrModelData.IrModelData>("base.module_category_theme").Id }
        }, "Name").ToList();

        foreach (var theme in themes) {
            var terp = Env.GetModuleInfo(theme.Name);
            var images = terp.TryGetValue("images", out var imagesValue) ? imagesValue as List<object> : new List<object>();
            var imagePaths = images.Select(x => $"/{theme.Name}/{x}").ToList();

            if (imagePaths.All(imagePath => existingUrls.Contains(imagePath))) {
                continue;
            }

            // Images creation order must be the order specified in the manifest
            foreach (var imagePath in imagePaths) {
                var imageName = imagePath.Split('/').Last();
                irAttachment.Create(new Dictionary<string, object>() {
                    { "Type", "url" },
                    { "Name", imageName },
                    { "Url", imagePath },
                    { "ResModel", "Website.IrModuleModule" },
                    { "ResId", theme.Id }
                });
            }
        }
    }

    public List<object> GetThemesDomain() {
        return new List<object>() {
            new[] { "State", "!=", "uninstallable" },
            new[] { "CategoryId", "not in", new[] {
                Env.Ref<IrModelData.IrModelData>("base.module_category_hidden").Id,
                Env.Ref<IrModelData.IrModelData>("base.module_category_theme_hidden").Id
            } },
            new[] { "|",
                new[] { "CategoryId", "=", Env.Ref<IrModelData.IrModelData>("base.module_category_theme").Id },
                new[] { "CategoryId.ParentId", "=", Env.Ref<IrModelData.IrModelData>("base.module_category_theme").Id }
            }
        };
    }

    public void Check() {
        base.Check();

        var view = Env.Ref<IrUiView.IrUiView>("ir.ui.view");
        var websiteViewsToAdapt = Env.Pool.Get<object>("WebsiteViewsToAdapt");

        if (websiteViewsToAdapt != null) {
            foreach (var viewReplay in websiteViewsToAdapt.GetValue(Env.Pool)) {
                var cowView = view.Browse(viewReplay[0]).First();
                view.LoadRecordsWriteOnCow(cowView, viewReplay[1], viewReplay[2]);
            }

            websiteViewsToAdapt.SetValue(Env.Pool, null);
        }
    }

    public List<object> LoadModuleTerms(List<string> modules, List<string> langs, bool overwrite = false) {
        var res = base.LoadModuleTerms(modules, langs, overwrite);

        if (langs == null || langs.Count == 0 || langs.Count == 1 && langs[0] == "en_US" || modules == null || modules.Count == 0) {
            return res;
        }

        // Add specific view translations

        // use the translation dic of the generic to translate the specific
        Env.Cr.Flush();
        var view = Env.Ref<IrUiView.IrUiView>("ir.ui.view");
        var field = view.GetType().GetField("ArchDb");

        // assume there are not too many records
        Env.Cr.Execute("SELECT generic.arch_db, specific.arch_db, specific.id FROM ir_ui_view generic INNER JOIN ir_ui_view specific ON generic.key = specific.key WHERE generic.website_id IS NULL AND generic.type = 'qweb' AND specific.website_id IS NOT NULL");
        var data = Env.Cr.FetchAll();
        foreach (var row in data) {
            if (row[0] == null) {
                continue;
            }

            var langsUpdate = langs.Intersect(row[0] as Dictionary<string, object>).Except(new[] { "en_US" }).ToList();

            if (langsUpdate.Count == 0) {
                continue;
            }

            // get dictionaries limited to the requested languages
            var genericArchDbEn = (row[0] as Dictionary<string, object>).TryGetValue("en_US", out var enTranslation) ? enTranslation : null;
            var specificArchDbEn = (row[1] as Dictionary<string, object>).TryGetValue("en_US", out var enTranslation) ? enTranslation : null;
            var genericArchDbUpdate = (row[0] as Dictionary<string, object>).Where(x => langsUpdate.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
            var specificArchDbUpdate = (row[1] as Dictionary<string, object>).Where(x => langsUpdate.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
            var genericTranslationDictionary = field.GetCustomAttribute<TranslationDictionaryAttribute>().GetTranslationDictionary(genericArchDbEn, genericArchDbUpdate);
            var specificTranslationDictionary = field.GetCustomAttribute<TranslationDictionaryAttribute>().GetTranslationDictionary(specificArchDbEn, specificArchDbUpdate);

            // update specific_translation_dictionary
            foreach (var (termEn, specificTermLangs) in specificTranslationDictionary) {
                if (!genericTranslationDictionary.ContainsKey(termEn)) {
                    continue;
                }

                foreach (var (lang, genericTermLang) in genericTranslationDictionary[termEn]) {
                    if (overwrite || termEn == specificTermLangs[lang].ToString()) {
                        specificTermLangs[lang] = genericTermLang;
                    }
                }
            }

            foreach (var lang in langsUpdate) {
                specificArchDbUpdate[lang] = field.GetCustomAttribute<TranslationDictionaryAttribute>().Translate(
                    term => specificTranslationDictionary.TryGetValue(term, out var dict) ? dict.TryGetValue(lang, out var translation) ? translation : null : null,
                    specificArchDbUpdate["en_US"]);
            }

            Env.Cache.UpdateRaw(view.Browse(row[2].ToInt32()), field, new[] { specificArchDbUpdate }, true);
        }

        var defaultMenu = Env.Ref<WebsiteMenu.WebsiteMenu>("website.main_menu");

        if (defaultMenu == null) {
            return res;
        }

        var langValueList = langs.Where(lang => lang != "en_US").Select(lang => $"{(lang)}, o_menu.name->>{(lang)}").ToList();
        var updateJsonbList = langValueList.Select((_, index) => $"jsonb_build_object({string.Join(", ", langValueList.Skip(index * 50).Take(50))})").ToList();
        var updateJsonb = string.Join(" || ", updateJsonbList);
        var oMenuName = overwrite ? $"menu.name || {updateJsonb}" : $"{updateJsonb} || menu.name";

        Env.Cr.Execute($"UPDATE website_menu menu SET name = {oMenuName} FROM website_menu o_menu INNER JOIN website_menu s_menu ON o_menu.name->>'en_US' = s_menu.name->>'en_US' AND o_menu.url = s_menu.url INNER JOIN website_menu root_menu ON s_menu.parent_id = root_menu.id AND root_menu.parent_id IS NULL WHERE o_menu.website_id IS NULL AND o_menu.parent_id = {defaultMenu.Id} AND s_menu.website_id IS NOT NULL AND menu.id = s_menu.id");

        return res;
    }

    public void GeneratePrimarySnippetTemplates() {
        // ------------------------------------------------------------
        // Configurator
        // ------------------------------------------------------------

        var manifest = Env.GetModuleInfo(this.Name);
        var configuratorSnippets = manifest.TryGetValue("configurator_snippets", out var configuratorSnippetsValue) ? configuratorSnippetsValue as Dictionary<string, object> : new Dictionary<string, object>();

        // Generate general configurator snippet templates
        var createValues = new List<Dictionary<string, object>>();
        foreach (var snippetName in GetDistinctSnippetNames(configuratorSnippets)) {
            createValues.Add(GetCreateVals(
                $"Snippet {snippetName!r} for pages generated by the configurator",
                snippetName, "%s", "configurator_%s"));
        }

        CreateMissingViews(createValues);

        // Generate configurator snippet templates for specific pages
        createValues.Clear();
        foreach (var pageName in configuratorSnippets.Keys) {
            foreach (var snippetName in (configuratorSnippets[pageName] as List<object>).Select(x => x.ToString()).ToHashSet()) {
                createValues.Add(GetCreateVals(
                    $"Snippet {snippetName!r} for {pageName!r} pages generated by the configurator",
                    snippetName, "configurator_%s", $"configurator_{pageName}_%s"));
            }
        }

        CreateMissingViews(createValues);

        // ------------------------------------------------------------
        // New page templates
        // ------------------------------------------------------------

        var templates = manifest.TryGetValue("new_page_templates", out var templatesValue) ? templatesValue as Dictionary<string, object> : new Dictionary<string, object>();

        // Generate general new page snippet templates
        createValues.Clear();
        foreach (var snippetName in GetDistinctSnippetNames(templates)) {
            createValues.Add(GetCreateVals(
                $"Snippet {snippetName!r} for new page templates",
                snippetName, "%s", "new_page_template_%s"));
        }

        CreateMissingViews(createValues);

        // Generate new page snippet templates for new page template groups
        createValues.Clear();
        foreach (var group in templates.Keys) {
            foreach (var snippetName in (GetDistinctSnippetNames(templates[group]) as List<object>).Select(x => x.ToString()).ToHashSet()) {
                createValues.Add(GetCreateVals(
                    $"Snippet {snippetName!r} for new page {group!r} templates",
                    snippetName, "new_page_template_%s", $"new_page_template_{group}_%s"));
            }
        }

        CreateMissingViews(createValues);

        // Generate new page snippet templates for specific new page templates within groups
        createValues.Clear();
        foreach (var group in templates.Keys) {
            foreach (var templateName in (templates[group] as Dictionary<string, object>).Keys) {
                foreach (var snippetName in (templates[group] as Dictionary<string, object>)[templateName] as List<object>) {
                    createValues.Add(GetCreateVals(
                        $"Snippet {snippetName!r} for new page {group!r} template {templateName!r}",
                        snippetName, $"new_page_template_{group}_%s", $"new_page_template_{group}_{templateName}_%s"));
                }
            }
        }

        CreateMissingViews(createValues);

        if (createValues.Count > 0) {
            Env.Logger.Info($"Generated {createValues.Count} primary snippet templates for {this.Name}");
        }
    }

    public void GeneratePrimaryPageTemplates() {
        var view = Env.Ref<IrUiView.IrUiView>("ir.ui.view");
        var manifest = Env.GetModuleInfo(this.Name);
        var templates = manifest.TryGetValue("new_page_templates", out var templatesValue) ? templatesValue as Dictionary<string, object> : new Dictionary<string, object>();

        // TODO Find a way to create theme and other module's template patches
        // Create or update template views per group x key
        var createValues = new List<Dictionary<string, object>>();
        foreach (var group in templates.Keys) {
            foreach (var templateName in (templates[group] as Dictionary<string, object>).Keys) {
                var xmlid = $"{this.Name}.new_page_template_sections_{group}_{templateName}";
                var wrapper = $"{this.Name}.new_page_template_{group}_{templateName}_%s";
                var calls = string.Join("\n    ", (templates[group] as Dictionary<string, object>)[templateName] as List<object>
                    .Select(x => x.ToString())
                    .Select(snippetKey => {
                        var splitSnippetKey = snippetKey.Split('.');
                        return splitSnippetKey.Length == 1 ? $"website.{splitSnippetKey[0]}" : string.Join(".", splitSnippetKey);
                    })
                    .Select(snippetKey => $"<t t-snippet-call=\"{wrapper % snippetKey}\"/>"));
                createValues.Add(new Dictionary<string, object>() {
                    { "Name", $"New page template: {templateName!r} in {group!r}" },
                    { "Type", "qweb" },
                    { "Key", xmlid },
                    { "Arch", $"<div id=\"wrap\">\n    {calls}\n</div>" },
                });
            }
        }

        var existingPrimaryTemplates = view.SearchRead(new[] {
            new[] { "Mode", "=", "primary" },
            new[] { "Key", "in", createValues.Select(x => x["Key"].ToString()).ToList() }
        }, new[] { "Key" });

        var existingPrimaryTemplateKeys = existingPrimaryTemplates.ToDictionary(x => x["Key"].ToString(), x => x["id"]);
        var missingCreateValues = new List<Dictionary<string, object>>();
        int updateCount = 0;
        foreach (var createValue in createValues) {
            if (existingPrimaryTemplateKeys.ContainsKey(createValue["Key"].ToString())) {
                view.Browse(existingPrimaryTemplateKeys[createValue["Key"].ToString()]).WithContext(new[] {
                    new[] { "no_cow", true }
                }).Write(new Dictionary<string, object>() {
                    { "Arch", createValue["Arch"].ToString() }
                });
                updateCount++;
            } else {
                missingCreateValues.Add(createValue);
            }
        }

        if (missingCreateValues.Count > 0) {
            var missingRecords = view.Create(missingCreateValues);
            Env.Ref<IrModelData.IrModelData>("ir.model.data").Create(missingRecords.Select(x => new Dictionary<string, object>() {
                { "Name", x.Key.Split('.')[1] },
                { "Module", x.Key.Split('.')[0] },
                { "Model", "IrUiView" },
                { "ResId", x.Id },
                { "Noupdate", true }
            }).ToList());

            Env.Logger.Info($"Generated {missingCreateValues.Count} primary page templates for {this.Name}");
        }

        if (updateCount > 0) {
            Env.Logger.Info($"Updated {updateCount} primary page templates for {this.Name}");
        }
    }

    private Dictionary<string, object> GetCreateVals(string name, string snippetKey, string parentWrap, string newWrap) {
        var module = snippetKey.Contains('.') ? snippetKey.Split('.')[0] : "website";
        var xmlid = snippetKey.Contains('.') ? snippetKey.Split('.')[1] : snippetKey;
        var parentKey = $"{module}.{parentWrap % xmlid}";

        var parent = Env.Ref<IrModelData.IrModelData>("ir.model.data")._XmlidToResModelResId(parentKey, null);
        if (parent == null) {
            Env.Logger.Warning($"No such snippet template: {parentKey}");
            return null;
        }

        return new Dictionary<string, object>() {
            { "Name", name },
            { "Key", $"{module}.{newWrap % xmlid}" },
            { "InheritId", parent[1] },
            { "Mode", "primary" },
            { "Type", "qweb" },
            { "Arch", "<t/>" },
        };
    }

    private List<object> GetDistinctSnippetNames(Dictionary<string, object> structure) {
        var items = new List<object>();
        foreach (var value in structure.Values) {
            if (value is List<object> list) {
                items.AddRange(list);
            } else if (value is Dictionary<string, object> dict) {
                items.AddRange(GetDistinctSnippetNames(dict));
            }
        }

        return items.ToHashSet().ToList();
    }

    private int CreateMissingViews(List<Dictionary<string, object>> createValues) {
        var view = Env.Ref<IrUiView.IrUiView>("ir.ui.view");
        var keys = createValues.Select(x => x["Key"].ToString()).ToList();
        var existingPrimaryTemplateKeys = view.SearchFetch(new[] {
            new[] { "Mode", "=", "primary" },
            new[] { "Key", "in", keys }
        }, new[] { "Key" }).Select(x => x["Key"].ToString()).ToList();
        var missingCreateValues = createValues.Where(x => !existingPrimaryTemplateKeys.Contains(x["Key"].ToString())).ToList();
        var missingRecords = view.WithContext(new[] {
            new[] { "no_cow", true }
        }).Create(missingCreateValues);
        Env.Ref<IrModelData.IrModelData>("ir.model.data").Create(missingRecords.Select(x => new Dictionary<string, object>() {
            { "Name", x.Key.Split('.')[1] },
            { "Module", x.Key.Split('.')[0] },
            { "Model", "IrUiView" },
            { "ResId", x.Id },
            { "Noupdate", true }
        }).ToList());
        return missingRecords.Count();
    }
}
