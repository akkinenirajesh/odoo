C#
public partial class View {
    // all the model methods are written here.
    public void GetVisibilityPasswordDisplay() {
        this.VisibilityPasswordDisplay = Env.User.IsSuperUser ? this.VisibilityPassword : "********";
    }

    public void SetVisibilityPassword() {
        if (this.Type == "qweb") {
            this.VisibilityPassword = Env.User.CryptContext.Encrypt(this.VisibilityPasswordDisplay);
            this.Visibility = this.Visibility; // double check access
        }
    }

    public void ComputeFirstPageId() {
        this.FirstPageId = Env.GetModel<Page>().Search(new[] { new Tuple<string, object>("ViewId", this.Id) }, 1);
    }

    public void Create(Dictionary<string, object> values) {
        if (Env.Context.ContainsKey("website_id")) {
            int websiteId = (int)Env.Context["website_id"];
            if (!values.ContainsKey("WebsiteId")) {
                values["WebsiteId"] = websiteId;
            } else {
                int newWebsiteId = (int)values["WebsiteId"];
                if (newWebsiteId == 0) {
                    throw new ArgumentException($"Trying to create a generic view from a website {websiteId} environment");
                } else if (newWebsiteId != websiteId) {
                    throw new ArgumentException($"Trying to create a view for website {newWebsiteId} from a website {websiteId} environment");
                }
            }
        }

        base.Create(values);
    }

    public void ComputeDisplayName() {
        if (!Env.Context.ContainsKey("display_key") && !Env.Context.ContainsKey("display_website")) {
            base.ComputeDisplayName();
            return;
        }

        string viewName = this.Name;
        if (Env.Context.ContainsKey("display_key")) {
            viewName += $" <{this.Key}>";
        }
        if (Env.Context.ContainsKey("display_website") && this.WebsiteId != null) {
            viewName += $" [{this.WebsiteId.Name}]";
        }
        this.DisplayName = viewName;
    }

    public void Write(Dictionary<string, object> values) {
        int currentWebsiteId = (int?)Env.Context["website_id"] ?? 0;
        if (currentWebsiteId == 0 || Env.Context.ContainsKey("no_cow")) {
            base.Write(values);
            return;
        }

        foreach (View view in this.Search(new[] { new Tuple<string, object>("Active", true) }, "website_id desc")) {
            if (!view.Key && !values.ContainsKey("Key")) {
                view.Write(new Dictionary<string, object> { { "Key", $"website.key_{Guid.NewGuid().ToString().Substring(0, 6)}" } });
            }

            if (view.WebsiteId != null) {
                base.Write(values);
                continue;
            }

            view.PageIds.Flush();
            view.PageIds.Invalidate();

            View specificView = view.Search(new[] {
                new Tuple<string, object>("Key", view.Key),
                new Tuple<string, object>("WebsiteId", currentWebsiteId)
            }, 1);
            if (specificView != null) {
                specificView.Write(values);
                continue;
            }

            Dictionary<string, object> copyValues = new Dictionary<string, object> {
                { "WebsiteId", currentWebsiteId },
                { "Key", view.Key }
            };
            if (values.ContainsKey("InheritId")) {
                copyValues["InheritId"] = values["InheritId"];
            }
            specificView = view.Copy(copyValues);

            view.CreateWebsiteSpecificPagesForView(specificView, Env.GetModel<Website>().Browse(currentWebsiteId));

            foreach (View inheritChild in view.InheritChildrenIds.FilterDuplicate().OrderBy(v => (v.Priority, v.Id))) {
                if (inheritChild.WebsiteId.Id == currentWebsiteId) {
                    View child = inheritChild.Copy(new Dictionary<string, object> {
                        { "InheritId", specificView.Id },
                        { "Key", inheritChild.Key }
                    });
                    inheritChild.InheritChildrenIds.Write(new Dictionary<string, object> { { "InheritId", child.Id } });
                    inheritChild.Delete();
                } else {
                    inheritChild.Write(new Dictionary<string, object> { { "InheritId", specificView.Id } });
                }
            }

            specificView.Write(values);
        }
    }

    public void LoadRecordsWriteOnCow(View cowView, int inheritId, Dictionary<string, object> values) {
        int newInheritId = this.Search(new[] {
            new Tuple<string, object>("Key", this.Browse(inheritId).Key),
            new Tuple<string, object>("WebsiteId", (cowView.WebsiteId != null) ? (object)cowView.WebsiteId.Id : (object)null)
        }, "website_id", 1).Id;
        values["InheritId"] = newInheritId;
        cowView.Write(values, new Dictionary<string, object> { { "no_cow", true } });
    }

    public void CreateAllSpecificViews(List<string> processedModules) {
        string regex = "^(" + string.Join("|", processedModules) + ")[.]";
        var result = Env.Cr.ExecuteAndFetchAll($"SELECT generic.id, ARRAY[array_agg(spec_parent.id), array_agg(spec_parent.website_id)] FROM ir_ui_view generic INNER JOIN ir_ui_view generic_parent ON generic_parent.id = generic.inherit_id INNER JOIN ir_ui_view spec_parent ON spec_parent.key = generic_parent.key LEFT JOIN ir_ui_view specific ON specific.key = generic.key AND specific.website_id = spec_parent.website_id WHERE generic.type='qweb' AND generic.website_id IS NULL AND generic.key ~ {regex} AND spec_parent.website_id IS NOT NULL AND specific.id IS NULL GROUP BY generic.id", new Dictionary<string, object>());

        foreach (var record in this.Browse(result.Select(r => (int)r[0]))) {
            List<int> specificParentViewIds = (List<int>)result.First(r => (int)r[0] == record.Id)[1];
            List<int> websiteIds = (List<int>)result.First(r => (int)r[0] == record.Id)[2];
            for (int i = 0; i < specificParentViewIds.Count; i++) {
                record.Write(new Dictionary<string, object> { { "InheritId", specificParentViewIds[i] } }, new Dictionary<string, object> { { "website_id", websiteIds[i] } });
            }
        }

        base.CreateAllSpecificViews(processedModules);
    }

    public void Delete() {
        int currentWebsiteId = (int?)Env.Context["website_id"] ?? 0;
        if (currentWebsiteId != 0 && !Env.Context.ContainsKey("no_cow")) {
            foreach (View view in this.Where(v => v.WebsiteId == null)) {
                foreach (Website website in Env.GetModel<Website>().Search(new[] { new Tuple<string, object>("Id", currentWebsiteId) })) {
                    view.Write(new Dictionary<string, object> { { "Name", view.Name } }, new Dictionary<string, object> { { "website_id", website.Id } });
                }
            }
        }

        List<View> specificViews = new List<View>();
        if (this != null && Env.Registry.IsInit) {
            foreach (View view in this.Where(v => v.WebsiteId == null)) {
                specificViews.AddRange(view.GetSpecificViews());
            }
        }

        base.Delete(this.Concat(specificViews));
        Env.Registry.ClearCache("templates");
    }

    public void CreateWebsiteSpecificPagesForView(View newView, Website website) {
        foreach (Page page in this.PageIds) {
            Page newPage = page.Copy(new Dictionary<string, object> {
                { "ViewId", newView.Id },
                { "IsPublished", page.IsPublished }
            });
            page.MenuIds.Where(m => m.WebsiteId.Id == website.Id).ForEach(m => m.PageId = newPage.Id);
        }
    }

    public Dictionary<string, object> GetViewHierarchy() {
        this.EnsureOne();
        View topLevelView = this;
        while (topLevelView.InheritId != null) {
            topLevelView = topLevelView.InheritId;
        }
        topLevelView = topLevelView.WithContext(new Dictionary<string, object> { { "active_test", false } });
        List<Dictionary<string, object>> siblingViews = topLevelView.SearchRead(new[] {
            new Tuple<string, object>("Key", topLevelView.Key),
            new Tuple<string, object>("Id", topLevelView.Id, "!=")
        });
        return new Dictionary<string, object> {
            { "sibling_views", siblingViews },
            { "hierarchy", topLevelView.BuildHierarchyDatastructure() }
        };
    }

    public Dictionary<string, object> BuildHierarchyDatastructure() {
        List<Dictionary<string, object>> inheritChildren = new List<Dictionary<string, object>>();
        foreach (View child in this.InheritChildrenIds) {
            inheritChildren.Add(child.BuildHierarchyDatastructure());
        }
        return new Dictionary<string, object> {
            { "id", this.Id },
            { "name", this.Name },
            { "inherit_children", inheritChildren },
            { "arch_updated", this.ArchUpdated },
            { "website_name", this.WebsiteId != null ? this.WebsiteId.Name : null },
            { "active", this.Active },
            { "key", this.Key }
        };
    }

    public List<View> GetRelatedViews(string key, bool bundles = false) {
        this = this.WithContext(new Dictionary<string, object> { { "website_id", Env.GetModel<Website>().GetCurrentWebsite().Id } });
        return base.GetRelatedViews(key, bundles);
    }

    public List<View> FilterDuplicate() {
        int currentWebsiteId = (int?)Env.Context["website_id"] ?? 0;
        List<View> mostSpecificViews = new List<View>();
        if (currentWebsiteId == 0) {
            return this.Where(v => v.WebsiteId == null).ToList();
        }

        foreach (View view in this) {
            if (view.WebsiteId != null && view.WebsiteId.Id == currentWebsiteId) {
                mostSpecificViews.Add(view);
            } else if (view.WebsiteId == null && !this.Any(v => v.Key == view.Key && v.WebsiteId != null && v.WebsiteId.Id == currentWebsiteId)) {
                mostSpecificViews.Add(view);
            }
        }

        return mostSpecificViews;
    }

    public List<View> ViewGetInheritedChildren(View view) {
        return base.ViewGetInheritedChildren(view).FilterDuplicate();
    }

    public View ViewObj(object viewId) {
        if (viewId is string || viewId is int) {
            return Env.GetModel<Website>().Viewref(viewId);
        } else {
            return (View)viewId;
        }
    }

    public List<Tuple<string, object>> GetInheritingViewsDomain() {
        List<Tuple<string, object>> domain = base.GetInheritingViewsDomain();
        Website currentWebsite = Env.GetModel<Website>().Browse(Env.Context.ContainsKey("website_id") ? (int)Env.Context["website_id"] : 0);
        List<Tuple<string, object>> websiteViewsDomain = currentWebsite.WebsiteDomain();
        if (currentWebsite != null) {
            domain = domain.Where(l => !l.Item1.Equals("active")).ToList();
        }
        return Env.Expression.AND(websiteViewsDomain, domain);
    }

    public List<View> GetInheritingViews() {
        if (!Env.Context.ContainsKey("website_id")) {
            return base.GetInheritingViews();
        }

        List<View> views = base.GetInheritingViews(new Dictionary<string, object> { { "active_test", false } });
        return views.FilterDuplicate().Where(v => v.Active).ToList();
    }

    public string GetFilterXmlidQuery() {
        if (!Env.Context.ContainsKey("website_id")) {
            return base.GetFilterXmlidQuery();
        } else {
            return "SELECT res_id FROM ir_model_data WHERE res_id IN %(res_ids)s AND model = 'ir.ui.view' AND module IN %(modules)s UNION SELECT sview.id FROM ir_ui_view sview INNER JOIN ir_ui_view oview USING (key) INNER JOIN ir_model_data d ON oview.id = d.res_id AND d.model = 'ir.ui.view' AND d.module IN %(modules)s WHERE sview.id IN %(res_ids)s AND sview.website_id IS NOT NULL AND oview.website_id IS NULL;";
        }
    }

    public int GetViewId(object xmlId) {
        int websiteId = (int?)Env.Context["website_id"] ?? 0;
        if (websiteId != 0 && !(xmlId is int)) {
            Website currentWebsite = Env.GetModel<Website>().Browse(websiteId);
            List<Tuple<string, object>> domain = new List<Tuple<string, object>> {
                new Tuple<string, object>("Key", xmlId),
            }.Concat(currentWebsite.WebsiteDomain()).ToList();

            View view = this.Search(domain, "website_id", 1);
            if (view == null) {
                Env.Logger.Warning($"Could not find view object with xml_id '{xmlId}'");
                throw new ArgumentException($"View {xmlId} in website {Env.Context["website_id"]} not found");
            }
            return view.Id;
        }
        return base.GetViewId(xmlId);
    }

    public string GetCachedVisibility() {
        return this.Visibility;
    }

    public bool HandleVisibility(bool doRaise = true) {
        bool error = false;

        this = this.Sudo();

        string visibility = this.GetCachedVisibility();

        if (visibility != "" && !Env.User.HasGroup("website.group_website_designer")) {
            if (visibility == "connected" && Env.GetModel<Website>().IsPublicUser()) {
                error = true;
            } else if (visibility == "password" && (Env.GetModel<Website>().IsPublicUser() || !Env.Session.ContainsKey("views_unlock") || !((List<int>)Env.Session["views_unlock"]).Contains(this.Id))) {
                string pwd = Env.Request.Params.ContainsKey("visibility_password") ? Env.Request.Params["visibility_password"] : null;
                if (pwd != null && Env.User.CryptContext.Verify(pwd, this.VisibilityPassword)) {
                    List<int> viewsUnlock = Env.Session.ContainsKey("views_unlock") ? (List<int>)Env.Session["views_unlock"] : new List<int>();
                    viewsUnlock.Add(this.Id);
                    Env.Session["views_unlock"] = viewsUnlock;
                } else {
                    error = true;
                }
            }

            if (visibility != "password" && visibility != "connected") {
                try {
                    this.CheckViewAccess();
                } catch (AccessError) {
                    error = true;
                }
            }
        }

        if (error) {
            if (doRaise) {
                throw new UnauthorizedAccessException();
            } else {
                return false;
            }
        }
        return true;
    }

    public string RenderTemplate(string template, Dictionary<string, object> values = null) {
        View view = this.Get(template).Sudo();
        view.HandleVisibility(true);
        if (values == null) {
            values = new Dictionary<string, object>();
        }
        if (!values.ContainsKey("main_object")) {
            values["main_object"] = view;
        }
        return base.RenderTemplate(template, values);
    }

    public string GetDefaultLangCode() {
        int websiteId = (int?)Env.Context["website_id"] ?? 0;
        if (websiteId != 0) {
            return Env.GetModel<Website>().Browse(websiteId).DefaultLangId.Code;
        } else {
            return base.GetDefaultLangCode();
        }
    }

    public List<string> ReadTemplateKeys() {
        return base.ReadTemplateKeys().Concat(new List<string> { "WebsiteId" }).ToList();
    }

    public Dictionary<string, object> SaveOeStructureHook() {
        Dictionary<string, object> res = base.SaveOeStructureHook();
        res["WebsiteId"] = Env.GetModel<Website>().GetCurrentWebsite().Id;
        return res;
    }

    public void SetNoupdate() {
        if (!Env.Context.ContainsKey("website_id")) {
            base.SetNoupdate();
        }
    }

    public void Save(string value, string xpath = null) {
        this.EnsureOne();
        Website currentWebsite = Env.GetModel<Website>().GetCurrentWebsite();
        if (xpath != null && this.Key != null && currentWebsite != null) {
            View specificView = this.Search(new[] {
                new Tuple<string, object>("Key", this.Key),
                new Tuple<string, object>("WebsiteId", currentWebsite.Id)
            }, 1);
            if (specificView != null) {
                this = specificView;
            }
        }
        base.Save(value, xpath);
    }

    public List<string> GetAllowedRootAttrs() {
        return base.GetAllowedRootAttrs().Concat(new List<string> { "data-bg-video-src", "data-shape", "data-scroll-background-ratio" }).ToList();
    }

    public XElement GetCombinedArch() {
        XElement root = base.GetCombinedArch();
        add_form_signature(root, this.Sudo().Env);
        return root;
    }

    public Dictionary<string, object> SnippetSaveViewValuesHook() {
        Dictionary<string, object> res = base.SnippetSaveViewValuesHook();
        int websiteId = (int?)Env.Context["website_id"] ?? 0;
        if (websiteId != 0) {
            res["WebsiteId"] = websiteId;
        }
        return res;
    }

    public void UpdateFieldTranslations(string fname, Dictionary<string, string> translations, string digest = null, string sourceLang = null) {
        base.UpdateFieldTranslations(fname, translations, digest, sourceLang, new Dictionary<string, object> { { "no_cow", true } });
    }

    public string GetBaseLang() {
        this.EnsureOne();
        Website website = this.WebsiteId;
        if (website != null) {
            return website.DefaultLangId.Code;
        }
        return base.GetBaseLang();
    }
}
