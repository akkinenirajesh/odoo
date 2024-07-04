C#
public partial class ThemeUtils {
    // all the model methods are written here.

    public static string[] HeaderTemplates { get; } = new string[]
    {
        "Website.template_header_hamburger",
        "Website.template_header_vertical",
        "Website.template_header_sidebar",
        "Website.template_header_boxed",
        "Website.template_header_stretch",
        "Website.template_header_search",
        "Website.template_header_sales_one",
        "Website.template_header_sales_two",
        "Website.template_header_sales_three",
        "Website.template_header_sales_four",
        // Default one, keep it last
        "Website.template_header_default",
    };

    public static string[] FooterTemplates { get; } = new string[]
    {
        "Website.template_footer_descriptive",
        "Website.template_footer_centered",
        "Website.template_footer_links",
        "Website.template_footer_minimalist",
        "Website.template_footer_contact",
        "Website.template_footer_call_to_action",
        "Website.template_footer_headline",
        // Default one, keep it last
        "Website.footer_custom",
    };

    public void PostCopy(string moduleName) {
        string themePostCopyMethodName = "_" + moduleName + "_PostCopy";
        if (this.GetType().GetMethod(themePostCopyMethodName) != null) {
            this.GetType().GetMethod(themePostCopyMethodName).Invoke(this, new object[] { moduleName });
        }
    }

    public void ResetDefaultConfig() {
        // Reinitialize some css customizations
        Env.Call("web_editor.assets", "MakeScssCustomization", new object[] {
            "/website/static/src/scss/options/user_values.scss",
            new Dictionary<string, object>() {
                { "font", "null" },
                { "headings-font", "null" },
                { "navbar-font", "null" },
                { "buttons-font", "null" },
                { "color-palettes-number", "null" },
                { "color-palettes-name", "null" },
                { "btn-ripple", "null" },
                { "header-template", "null" },
                { "footer-template", "null" },
                { "footer-scrolltop", "null" },
            }
        });

        // Reinitialize effets
        DisableAsset("website.ripple_effect_scss");
        DisableAsset("website.ripple_effect_js");

        // Reinitialize header templates
        foreach (string view in HeaderTemplates.Except(new string[] { HeaderTemplates.Last() })) {
            DisableView(view);
        }
        EnableView(HeaderTemplates.Last());

        // Reinitialize footer templates
        foreach (string view in FooterTemplates.Except(new string[] { FooterTemplates.Last() })) {
            DisableView(view);
        }
        EnableView(FooterTemplates.Last());

        // Reinitialize footer scrolltop template
        DisableView("website.option_footer_scrolltop");
    }

    public void ToggleAsset(string key, bool active) {
        ThemeIrAsset themeIrAsset = Env.Model<ThemeIrAsset>().WithContext(new Dictionary<string, object>() { { "ActiveTest", false } }).Search(new Dictionary<string, object>() { { "Key", key } });
        Website website = Env.Model<Website>().GetCurrentWebsite();
        if (themeIrAsset != null) {
            themeIrAsset = themeIrAsset.CopyIds.Where(x => x.WebsiteId == website).FirstOrDefault();
        } else {
            IrAsset irAsset = Env.Model<IrAsset>().WithContext(new Dictionary<string, object>() { { "ActiveTest", false } }).Search(new Dictionary<string, object>() { { "Key", key } }).FirstOrDefault();
            bool hasSpecific = irAsset != null && Env.Model<IrAsset>().SearchCount(new Dictionary<string, object>() { { "Key", irAsset.Key }, { "WebsiteId", website.Id } }) >= 1;
            if (!hasSpecific && active == irAsset.Active) {
                return;
            }
        }
        themeIrAsset.Active = active;
    }

    public void ToggleView(string xmlId, bool active) {
        object obj = Env.Ref(xmlId);
        Website website = Env.Model<Website>().GetCurrentWebsite();
        if (obj.GetType().Name == "Website.ThemeIrUiView") {
            ThemeIrUiView themeIrUiView = (ThemeIrUiView)obj;
            themeIrUiView = themeIrUiView.WithContext(new Dictionary<string, object>() { { "ActiveTest", false } });
            themeIrUiView = themeIrUiView.CopyIds.Where(x => x.WebsiteId == website).FirstOrDefault();
        } else {
            // If a theme post copy wants to enable/disable a view, this is to
            // enable/disable a given functionality which is disabled/enabled
            // by default. So if a post copy asks to enable/disable a view which
            // is already enabled/disabled, we would not consider it otherwise it
            // would COW the view for nothing.
            IrUiView irUiView = Env.Model<IrUiView>().WithContext(new Dictionary<string, object>() { { "ActiveTest", false } }).Search(new Dictionary<string, object>() { { "Key", xmlId }, { "WebsiteId", website.Id } }).FirstOrDefault();
            bool hasSpecific = irUiView != null && Env.Model<IrUiView>().SearchCount(new Dictionary<string, object>() { { "Key", irUiView.Key }, { "WebsiteId", website.Id } }) >= 1;
            if (!hasSpecific && active == irUiView.Active) {
                return;
            }
        }
        obj.Set("Active", active);
    }

    public void EnableAsset(string name) {
        ToggleAsset(name, true);
    }

    public void DisableAsset(string name) {
        ToggleAsset(name, false);
    }

    public void EnableView(string xmlId) {
        if (HeaderTemplates.Contains(xmlId)) {
            foreach (string view in HeaderTemplates) {
                DisableView(view);
            }
        } else if (FooterTemplates.Contains(xmlId)) {
            foreach (string view in FooterTemplates) {
                DisableView(view);
            }
        }
        ToggleView(xmlId, true);
    }

    public void DisableView(string xmlId) {
        ToggleView(xmlId, false);
    }
}
