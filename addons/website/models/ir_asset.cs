csharp
public partial class WebsiteIrAsset {
    public string GetAssetBundleUrl(string filename, string unique, Dictionary<string, object> assetsParams, bool ignoreParams = false) {
        string routePrefix = "/web/assets";
        if (ignoreParams) {
            routePrefix = "/web/assets%";
        } else if (assetsParams.ContainsKey("WebsiteId") && assetsParams["WebsiteId"] != null) {
            routePrefix = $"/web/assets/{assetsParams["WebsiteId"]}";
        }
        return $"{routePrefix}/{unique}/{filename}";
    }

    public List<WebsiteIrAsset> GetRelatedAssets(List<object> domain, int? websiteId, Dictionary<string, object> params) {
        if (websiteId != null) {
            domain.AddRange(Env.Website.WebsiteDomain(websiteId.Value));
        }
        List<WebsiteIrAsset> assets = Env.WebsiteIrAsset.Search(domain, params);
        return assets.FilterDuplicate(websiteId);
    }

    public List<string> GetActiveAddonsList(int? websiteId, Dictionary<string, object> params) {
        List<string> addonsList = Env.WebsiteIrAsset.GetActiveAddonsList(params);

        if (websiteId != null) {
            IrModuleModule irModule = Env.IrModuleModule.Sudo();
            List<IrModuleModule> themes = irModule.Search(irModule.GetThemesDomain());
            themes.RemoveAll(t => t.Id == Env.Website.Browse(websiteId.Value).ThemeId);

            List<string> toRemove = themes.Select(t => t.Name).ToList();

            addonsList = addonsList.Where(name => !toRemove.Contains(name)).ToList();
        }
        return addonsList;
    }

    public List<WebsiteIrAsset> FilterDuplicate(int? websiteId) {
        Website website = websiteId != null ? Env.Website.Browse(websiteId.Value) : Env.Website.GetCurrentWebsite(false);
        if (website == null) {
            return this.Where(a => a.WebsiteId == null).ToList();
        }

        List<WebsiteIrAsset> mostSpecificAssets = new List<WebsiteIrAsset>();
        foreach (WebsiteIrAsset asset in this) {
            if (asset.WebsiteId == website) {
                mostSpecificAssets.Add(asset);
            } else if (asset.WebsiteId == null) {
                if (string.IsNullOrEmpty(asset.Key)) {
                    mostSpecificAssets.Add(asset);
                } else if (!this.Any(a => a.Key == asset.Key && a.WebsiteId == website)) {
                    mostSpecificAssets.Add(asset);
                }
            }
        }
        return mostSpecificAssets;
    }

    public bool Write(Dictionary<string, object> vals) {
        int? currentWebsiteId = Env.Context.Get<int?>("website_id");
        if (currentWebsiteId == null || Env.Context.Get<bool?>("no_cow") == true) {
            return base.Write(vals);
        }

        foreach (WebsiteIrAsset asset in this.Where(a => a.Active == true)) {
            if (asset.WebsiteId != null) {
                base.Write(vals, asset);
                continue;
            }

            WebsiteIrAsset websiteSpecificAsset = this.Where(a => a.Key == asset.Key && a.WebsiteId == currentWebsiteId).FirstOrDefault();
            if (websiteSpecificAsset != null) {
                base.Write(vals, websiteSpecificAsset);
                continue;
            }

            Dictionary<string, object> copyVals = new Dictionary<string, object>() { { "WebsiteId", currentWebsiteId }, { "Key", asset.Key } };
            WebsiteIrAsset newAsset = asset.Copy(copyVals);
            base.Write(vals, newAsset);
        }
        return true;
    }
}
