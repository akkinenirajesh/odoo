csharp
public partial class WebsiteRoute {
    // all the model methods are written here.
}

public partial class WebsiteRewrite {
    public void OnChangeRouteId() {
        this.UrlFrom = this.RouteId.Path;
        this.UrlTo = this.RouteId.Path;
    }

    public void CheckUrlTo() {
        if (this.RedirectType == "301" || this.RedirectType == "302" || this.RedirectType == "308") {
            if (string.IsNullOrEmpty(this.UrlTo)) {
                throw new Exception("\"URL to\" can not be empty.");
            }
            if (string.IsNullOrEmpty(this.UrlFrom)) {
                throw new Exception("\"URL from\" can not be empty.");
            }
        }

        if (this.RedirectType == "308") {
            if (!this.UrlTo.StartsWith("/")) {
                throw new Exception("\"URL to\" must start with a leading slash.");
            }
            var urlFromParams = System.Text.RegularExpressions.Regex.Matches(this.UrlFrom, "/<.*?>");
            var urlToParams = System.Text.RegularExpressions.Regex.Matches(this.UrlTo, "/<.*?>");
            foreach (System.Text.RegularExpressions.Match param in urlFromParams) {
                if (!this.UrlTo.Contains(param.Value)) {
                    throw new Exception($"\"URL to\" must contain parameter {param.Value} used in \"URL from\".");
                }
            }
            foreach (System.Text.RegularExpressions.Match param in urlToParams) {
                if (!this.UrlFrom.Contains(param.Value)) {
                    throw new Exception($"\"URL to\" cannot contain parameter {param.Value} which is not used in \"URL from\".");
                }
            }

            if (this.UrlTo == "/") {
                throw new Exception("\"URL to\" cannot be set to \"/\". To change the homepage content, use the \"Homepage URL\" field in the website settings or the page properties on any custom page.");
            }

            var irHttp = Env.Get<IrHttp>();
            foreach (var rule in irHttp.RoutingMap().IterRules()) {
                if (rule.Rule.TrimEnd('/') == this.UrlTo.TrimEnd('/')) {
                    throw new Exception("\"URL to\" cannot be set to an existing page.");
                }
            }

            try {
                var converters = irHttp.GetConverters();
                var routingMap = new Werkzeug.Routing.Map(strictSlashes: false, converters: converters);
                var rule = new Werkzeug.Routing.Rule(this.UrlTo);
                routingMap.Add(rule);
            } catch (Exception e) {
                throw new Exception($"\"URL to\" is invalid: {e}");
            }
        }
    }

    public string DisplayName { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"{this.RedirectType} - {this.Name}";
    }

    public void InvalidateRouting() {
        Env.Registry.ClearCache("routing");
    }

    public void RefreshRoutes() {
        Env.Get<WebsiteRoute>().RefreshRoutes();
    }
}
