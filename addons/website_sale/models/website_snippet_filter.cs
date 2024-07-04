csharp
public partial class WebsiteSnippetFilter {
    public bool ProductCrossSelling { get; set; }

    public List<dynamic> PrepareValues(Dictionary<string, object> kwargs) {
        var website = Env.Get("website").GetActiveWebsite();
        if (this.ModelName == "product.product" && !website.HasEcommerceAccess()) {
            return new List<dynamic>();
        }
        return Env.Call("Website.WebsiteSnippetFilter", "PrepareValues", kwargs);
    }

    public dynamic GetWebsiteCurrency() {
        var website = Env.Get("website").GetActiveWebsite();
        return website.CurrencyId;
    }

    public List<dynamic> GetHardcodedSample(dynamic model) {
        var samples = Env.Call("Website.WebsiteSnippetFilter", "GetHardcodedSample", model);
        if (model.Name == "product.product") {
            var data = new List<dynamic>() {
                new Dictionary<string, object>() { 
                    {"Image512", "/product/static/img/product_chair.jpg"},
                    {"DisplayName", "Chair"},
                    {"DescriptionSale", "Sit comfortably"},
                },
                new Dictionary<string, object>() { 
                    {"Image512", "/product/static/img/product_lamp.png"},
                    {"DisplayName", "Lamp"},
                    {"DescriptionSale", "Lightbulb sold separately"},
                },
                new Dictionary<string, object>() { 
                    {"Image512", "/product/static/img/product_product_20-image.png"},
                    {"DisplayName", "Whiteboard"},
                    {"DescriptionSale", "With three feet"},
                },
                new Dictionary<string, object>() { 
                    {"Image512", "/product/static/img/product_product_27-image.jpg"},
                    {"DisplayName", "Drawer"},
                    {"DescriptionSale", "On wheels"},
                },
                new Dictionary<string, object>() { 
                    {"Image512", "/product/static/img/product_product_7-image.png"},
                    {"DisplayName", "Box"},
                    {"DescriptionSale", "Reinforced for heavy loads"},
                },
                new Dictionary<string, object>() { 
                    {"Image512", "/product/static/img/product_product_9-image.jpg"},
                    {"DisplayName", "Bin"},
                    {"DescriptionSale", "Pedal-based opening system"},
                }
            };
            var merged = new List<dynamic>();
            for (var index = 0; index < Math.Max(samples.Count, data.Count); index++) {
                merged.Add(new Dictionary<string, object>() { 
                    { "Image512", samples[index % samples.Count]["Image512"] },
                    { "DisplayName", samples[index % samples.Count]["DisplayName"] },
                    { "DescriptionSale", samples[index % samples.Count]["DescriptionSale"] },
                    { "Image512", data[index % data.Count]["Image512"] },
                    { "DisplayName", data[index % data.Count]["DisplayName"] },
                    { "DescriptionSale", data[index % data.Count]["DescriptionSale"] },
                });
            }
            return merged;
        }
        return samples;
    }

    public List<dynamic> FilterRecordsToValues(List<dynamic> records, bool isSample = false) {
        var hideVariants = Env.Context.Get("hideVariants") && records.Count == 1;
        if (hideVariants) {
            records = records.Select(r => r.ProductTemplateId).ToList();
        }
        var resProducts = Env.Call("Website.WebsiteSnippetFilter", "FilterRecordsToValues", records, isSample);
        if (this.ModelName == "product.product") {
            foreach (var resProduct in resProducts) {
                var product = resProduct["_record"];
                if (!isSample) {
                    if (hideVariants && !product.HasConfigurableAttributes) {
                        resProduct["_record"] = product = product.ProductVariantId;
                    }
                    if (product.IsProductVariant) {
                        resProduct.Update(product.GetCombinationInfoVariant());
                    } else {
                        resProduct.Update(product.GetCombinationInfo());
                    }
                    if (Env.Context.Get("add2cartRerender")) {
                        resProduct["_add2cartRerender"] = true;
                    }
                } else {
                    resProduct.Update(new Dictionary<string, object>() { {"isSample", true} });
                }
            }
        }
        return resProducts;
    }

    public List<dynamic> GetProducts(string mode, Dictionary<string, object> kwargs) {
        var dynamicFilter = Env.Context.Get("dynamicFilter");
        var handler = mode switch {
            "latestSold" => _GetProductsLatestSold,
            "latestViewed" => _GetProductsLatestViewed,
            "recentlySoldWith" => _GetProductsRecentlySoldWith,
            "accessories" => _GetProductsAccessories,
            "alternativeProducts" => _GetProductsAlternativeProducts,
            _ => _GetProductsLatestSold,
        };
        var website = Env.Get("website").GetActiveWebsite();
        var searchDomain = Env.Context.Get("searchDomain");
        var limit = Env.Context.Get("limit");
        var hideVariants = false;
        if (searchDomain != null && searchDomain.Contains("hideVariants")) {
            hideVariants = true;
            searchDomain.Remove("hideVariants");
        }
        var domain = new List<List<object>>() {
            Env.User.IsPublic() ? new List<object>() { ("websitePublished", "=", true) } : new List<object>(),
            website.WebsiteDomain(),
            new List<object>() { ("companyId", "in", new List<object>() { null, website.CompanyId.Id } )},
            searchDomain ?? new List<object>(),
        };
        var products = handler(website, limit, domain, kwargs);
        return dynamicFilter.WithContext(new Dictionary<string, object>() { {"hideVariants", hideVariants } }).FilterRecordsToValues(products, false);
    }

    public List<dynamic> _GetProductsLatestSold(dynamic website, int limit, List<List<object>> domain, Dictionary<string, object> kwargs) {
        var products = Env.Get("product.product");
        var saleOrders = Env.Get("sale.order").With("sudo").Search(new List<List<object>>() {
            ("websiteId", "=", website.Id),
            ("state", "=", "sale"),
        }, limit: 8, order: "dateOrder DESC");
        if (saleOrders.Count > 0) {
            var soldProducts = saleOrders.SelectMany(so => so.OrderLine).Select(ol => ol.ProductId.Id).ToList();
            var productsIds = soldProducts.GroupBy(id => id).OrderByDescending(g => g.Count()).Take(limit).Select(g => g.Key).ToList();
            if (productsIds.Count > 0) {
                domain = new List<List<object>>() {
                    domain,
                    new List<object>() { ("id", "in", productsIds ) },
                };
                products = Env.Get("product.product").WithContext(new Dictionary<string, object>() { {"displayDefaultCode", false } }).Search(domain);
                products = products.OrderBy(p => productsIds.IndexOf(p.Id)).Take(limit).ToList();
            }
        }
        return products;
    }

    public List<dynamic> _GetProductsLatestViewed(dynamic website, int limit, List<List<object>> domain, Dictionary<string, object> kwargs) {
        var products = Env.Get("product.product");
        var visitor = Env.Get("website.visitor").GetVisitorFromRequest();
        if (visitor != null) {
            var excludedProducts = website.SaleGetOrder().OrderLine.ProductId.Ids;
            var trackedProducts = Env.Get("website.track").With("sudo").ReadGroup(new List<List<object>>() {
                ("visitorId", "=", visitor.Id),
                ("productId", "!=", null),
                ("productId.websitePublished", "=", true),
                ("productId", "not in", excludedProducts),
            }, new List<string>() { "productId" }, limit: limit, order: "visitDateTime:max DESC");
            var productsIds = trackedProducts.Select(tp => tp[0].Id).ToList();
            if (productsIds.Count > 0) {
                domain = new List<List<object>>() {
                    domain,
                    new List<object>() { ("id", "in", productsIds) },
                };
                products = Env.Get("product.product").WithContext(new Dictionary<string, object>() { 
                    {"displayDefaultCode", false },
                    {"add2cartRerender", true } 
                }).Search(domain, limit);
            }
        }
        return products;
    }

    public List<dynamic> _GetProductsRecentlySoldWith(dynamic website, int limit, List<List<object>> domain, dynamic productTemplateId, Dictionary<string, object> kwargs) {
        var products = Env.Get("product.product");
        var currentTemplate = Env.Get("product.template").Browse(productTemplateId.Id).Exists();
        if (currentTemplate != null) {
            var saleOrders = Env.Get("sale.order").With("sudo").Search(new List<List<object>>() {
                ("websiteId", "=", website.Id),
                ("state", "=", "sale"),
                ("orderLine.productId.productTemplateId", "=", currentTemplate.Id),
            }, limit: 8, order: "dateOrder DESC");
            if (saleOrders.Count > 0) {
                var excludedProducts = website.SaleGetOrder().OrderLine.ProductId.ProductTemplateId.ProductVariantIds.Ids;
                excludedProducts.AddRange(currentTemplate.ProductVariantIds.Ids);
                var includedProducts = new List<int>();
                foreach (var saleOrder in saleOrders) {
                    includedProducts.AddRange(saleOrder.OrderLine.ProductId.Ids);
                }
                var productsIds = includedProducts.Except(excludedProducts).ToList();
                if (productsIds.Count > 0) {
                    domain = new List<List<object>>() {
                        domain,
                        new List<object>() { ("id", "in", productsIds) },
                    };
                    products = Env.Get("product.product").WithContext(new Dictionary<string, object>() { {"displayDefaultCode", false } }).Search(domain, limit);
                }
            }
        }
        return products;
    }

    public List<dynamic> _GetProductsAccessories(dynamic website, int limit, List<List<object>> domain, dynamic productTemplateId = null, Dictionary<string, object> kwargs) {
        var products = Env.Get("product.product");
        var currentTemplate = Env.Get("product.template").Browse(productTemplateId.Id).Exists();
        if (currentTemplate != null) {
            var excludedProducts = website.SaleGetOrder().OrderLine.ProductId.Ids;
            excludedProducts.AddRange(currentTemplate.ProductVariantIds.Ids);
            var includedProducts = currentTemplate.GetWebsiteAccessoryProduct().Ids;
            var productsIds = includedProducts.Except(excludedProducts).ToList();
            if (productsIds.Count > 0) {
                domain = new List<List<object>>() {
                    domain,
                    new List<object>() { ("id", "in", productsIds) },
                };
                products = Env.Get("product.product").WithContext(new Dictionary<string, object>() { {"displayDefaultCode", false } }).Search(domain, limit);
            }
        }
        return products;
    }

    public List<dynamic> _GetProductsAlternativeProducts(dynamic website, int limit, List<List<object>> domain, dynamic productTemplateId = null, Dictionary<string, object> kwargs) {
        var products = Env.Get("product.product");
        var currentTemplate = Env.Get("product.template").Browse(productTemplateId.Id).Exists();
        if (currentTemplate != null) {
            var excludedProducts = website.SaleGetOrder().OrderLine.ProductId;
            excludedProducts.AddRange(currentTemplate.ProductVariantIds);
            var includedProducts = currentTemplate.AlternativeProductIds.ProductVariantIds;
            products = includedProducts.Except(excludedProducts).ToList();
            if (website.PreventZeroPriceSale) {
                products = products.Where(p => p.GetContextualPrice() != 0).ToList();
            }
            if (products.Count > 0) {
                domain = new List<List<object>>() {
                    domain,
                    new List<object>() { ("id", "in", products.Select(p => p.Id).ToList()) },
                };
                products = Env.Get("product.product").WithContext(new Dictionary<string, object>() { {"displayDefaultCode", false } }).Search(domain, limit);
            }
        }
        return products;
    }

    public List<dynamic> PrepareValues(int? limit = null, List<List<object>> searchDomain = null) {
        var hideVariants = false;
        if (this.FilterId != null && searchDomain != null && searchDomain.Contains("hideVariants")) {
            hideVariants = true;
            searchDomain.Remove("hideVariants");
        }
        return Env.Call("Website.WebsiteSnippetFilter", "PrepareValues", limit, searchDomain);
    }
}
