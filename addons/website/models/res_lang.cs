csharp
public partial class WebsiteLang {
    public void Write(Dictionary<string, object> vals) {
        if (vals.ContainsKey("Active") && !(bool)vals["Active"]) {
            if (Env.Get("Website").SearchCount(new Dictionary<string, object>() { { "LanguageIds", this.Id } }, 1) > 0) {
                throw new UserError("Cannot deactivate a language that is currently used on a website.");
            }
        }
        base.Write(vals);
    }

    public LangDataDict GetFrontend() {
        if (Env.Request != null && Env.Request.IsFrontend) {
            var website = Env.Get("Website").GetCurrentWebsite();
            var langIds = website.LanguageIds.OrderBy(x => x.Name).Select(x => x.Id).ToList();
            var langs = langIds.Select(x => Env.Get("ResLang").GetData(x)).ToList();
            var es419Exists = langs.Any(x => x.Code == "es_419");
            var alreadyShortened = new List<string>();
            foreach (var lang in langs) {
                var code = lang.Code;
                var shortCode = code.Split('_')[0];
                if (shortCode != alreadyShortened && !(shortCode == "es" && code != "es_419" && es419Exists)) {
                    lang.Hreflang = shortCode;
                    alreadyShortened.Add(shortCode);
                } else {
                    lang.Hreflang = code.ToLower().Replace('_', '-');
                }
            }
            return new LangDataDict(langs.ToDictionary(x => x.Code, x => new LangData(x)));
        }
        return base.GetFrontend();
    }

    public Dictionary<string, object> ActionActivateLangs() {
        return new Dictionary<string, object>() {
            { "type", "ir.actions.act_window" },
            { "name", "Add languages" },
            { "view_mode", "form" },
            { "res_model", "Base.LanguageInstall" },
            { "views", new List<List<object>>() { new List<object>() { false, "form" } } },
            { "target", "new" }
        };
    }
}
