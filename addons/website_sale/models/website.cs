C#
public partial class Website 
{
    public Website()
    {
        // Default methods
        DefaultSalesteamId = DefaultSalesteamId();
        CartRecoveryMailTemplateId = DefaultRecoveryMailTemplate();
    }

    public int DefaultSalesteamId()
    {
        var team = Env.Ref("SalesTeam.SalesteamWebsiteSales", false);
        return team != null && team.Active ? team.Id : 0;
    }

    public int DefaultRecoveryMailTemplate()
    {
        try
        {
            return Env.Ref("WebsiteSale.MailTemplateSaleCartRecovery").Id;
        }
        catch
        {
            return 0;
        }
    }

    // Compute methods
    public void ComputePricelistIds()
    {
        var website = this.WithCompany(this.CompanyId);
        var productPricelist = website.Env.GetModel<Product.Pricelist>();
        PricelistIds = productPricelist.Search(productPricelist.GetWebsitePricelistsDomain(website)).ToCollection();
    }

    public void ComputePricelistId()
    {
        PricelistId = GetCurrentPricelist().Id;
    }

    public void ComputeFiscalPositionId()
    {
        FiscalPositionId = GetCurrentFiscalPosition().Id;
    }

    public void ComputeCurrencyId()
    {
        CurrencyId = PricelistId > 0 ? Env.GetModel<Product.Pricelist>().Browse(PricelistId).CurrencyId.Id : CompanyId > 0 ? Env.GetModel<Res.Company>().Browse(CompanyId).CurrencyId.Id : 0;
    }

    // Selection methods
    public List<KeyValuePair<string, string>> GetProductSortMapping()
    {
        return new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("website_sequence asc", Env.Translate("Featured")),
            new KeyValuePair<string, string>("create_date desc", Env.Translate("Newest Arrivals")),
            new KeyValuePair<string, string>("name asc", Env.Translate("Name (A-Z)")),
            new KeyValuePair<string, string>("list_price asc", Env.Translate("Price - Low to High")),
            new KeyValuePair<string, string>("list_price desc", Env.Translate("Price - High to Low"))
        };
    }

    // Business methods
    public List<int> GetPlPartnerOrder(string countryCode, bool showVisible, int currentPlId, List<int> websitePricelistIds, int partnerPlId = 0, int orderPlId = 0)
    {
        var pricelists = Env.GetModel<Product.Pricelist>();

        if (showVisible)
        {
            // Only show selectable or currently used pricelist (cart or session)
            var checkPricelist = (Product.Pricelist pl) => pl.Selectable || pl.Id == currentPlId || pl.Id == orderPlId;
            pricelists |= pricelists.Browse(websitePricelistIds).Where(checkPricelist);
        }
        else
        {
            var checkPricelist = (Product.Pricelist pl) => true;
            pricelists |= pricelists.Browse(websitePricelistIds).Where(checkPricelist);
        }

        if (!string.IsNullOrEmpty(countryCode))
        {
            var pricelistsByCountry = Env.GetModel<Res.CountryGroup>().Search(x => x.CountryIds.Any(c => c.Code == countryCode)).SelectMany(x => x.PricelistIds);
            pricelists |= pricelistsByCountry.Where(x => x.IsAvailableOnWebsite(this) && (showVisible ? x.Selectable : true));
        }

        if (!pricelists.Any())
        {
            pricelists |= pricelists.Browse(websitePricelistIds).Where(pl => (showVisible ? pl.Selectable : true) && (!string.IsNullOrEmpty(countryCode) ? !pl.CountryGroupIds.Any() : true));
        }

        if (!Env.User.IsPublic())
        {
            var partnerPricelist = pricelists.Browse(partnerPlId).Where(pl => pl.IsAvailableOnWebsite(this) && (showVisible ? pl.Selectable : true) && (!string.IsNullOrEmpty(countryCode) ? pl.IsAvailableInCountry(countryCode) : true));
            pricelists |= partnerPricelist;
        }

        return pricelists.Select(x => x.Id).ToList();
    }

    public List<Product.Pricelist> GetPricelistAvailable(bool showVisible = false)
    {
        var countryCode = GetGeoIpCountryCode();
        var website = this.WithCompany(this.CompanyId);

        var partnerSudo = website.Env.User.PartnerId;
        var isUserPublic = website.Env.User.IsPublic();
        var lastOrderPricelist = isUserPublic ? null : partnerSudo.LastWebsiteSoId.PricelistId;
        var partnerPricelist = isUserPublic ? null : partnerSudo.PropertyProductPricelistId;
        var websitePricelists = website.PricelistIds;

        var currentPricelistId = GetCachedPricelistId();

        var pricelistIds = GetPlPartnerOrder(countryCode, showVisible, currentPricelistId, websitePricelists.Select(x => x.Id).ToList(), partnerPricelist?.Id ?? 0, lastOrderPricelist?.Id ?? 0);

        return Env.GetModel<Product.Pricelist>().Browse(pricelistIds);
    }

    public bool IsPricelistAvailable(int plId)
    {
        return GetPricelistAvailable(false).Any(x => x.Id == plId);
    }

    public string GetGeoIpCountryCode()
    {
        return Env.Context.Get("geoip")?.Get("country_code");
    }

    public int? GetCachedPricelistId()
    {
        return Env.Context.Get("session")?.Get("website_sale_current_pl") as int?;
    }

    public Product.Pricelist GetCurrentPricelist()
    {
        var website = this.WithCompany(this.CompanyId);
        var productPricelist = website.Env.GetModel<Product.Pricelist>();
        var pricelist = productPricelist;

        if (Env.Context.Get("session")?.Get("website_sale_current_pl") is int pricelistId)
        {
            pricelist = productPricelist.Browse(pricelistId);

            var countryCode = GetGeoIpCountryCode();
            if (pricelist.IsAvailableOnWebsite(this) && (string.IsNullOrEmpty(countryCode) || pricelist.IsAvailableInCountry(countryCode)))
            {
                return pricelist;
            }

            Env.Context.Get("session").Remove("website_sale_current_pl");
            pricelist = productPricelist;
        }

        if (pricelist == null)
        {
            var partnerSudo = Env.User.PartnerId;

            pricelist = partnerSudo.LastWebsiteSoId?.PricelistId;
            if (pricelist == null)
            {
                pricelist = partnerSudo.PropertyProductPricelistId;
            }

            var availablePricelists = GetPricelistAvailable();
            if (availablePricelists.Any() && !availablePricelists.Contains(pricelist))
            {
                return availablePricelists.FirstOrDefault();
            }
        }

        return pricelist;
    }

    public List<int> SaleProductDomain()
    {
        var websiteDomain = GetCurrentWebsite().WebsiteDomain();
        var userIsInternal = Env.User.IsInternal();
        var productTemplateModel = Env.GetModel<Product.Template>();
        var saleableTrackingTypes = productTemplateModel.GetSaleableTrackingTypes();

        if (!userIsInternal)
        {
            websiteDomain = websiteDomain.And(new List<object> { new object[] { "IsPublished", "=", true }, new object[] { "ServiceTracking", "in", saleableTrackingTypes } });
        }

        return websiteDomain.And(new List<object> { new object[] { "SaleOk", "=", true } });
    }

    public List<int> ProductDomain()
    {
        return new List<int> { new object[] { "SaleOk", "=", true } };
    }

    public Sale.Order SaleGetOrder(bool forceCreate = false, bool updatePricelist = false)
    {
        var website = this.WithCompany(this.CompanyId);
        var saleOrderModel = website.Env.GetModel<Sale.Order>();
        var saleOrderId = Env.Context.Get("session")?.Get("sale_order_id") as int?;

        var saleOrderSudo = saleOrderId != null ? saleOrderModel.Browse(saleOrderId).Exists() : Env.User != null && !Env.User.IsPublic() ? Env.User.PartnerId.LastWebsiteSoId : null;

        if (saleOrderSudo != null && saleOrderSudo.GetPortalLastTransaction().State in new[] { "pending", "authorized", "done" })
        {
            saleOrderSudo = null;
        }

        if (saleOrderSudo == null && !forceCreate)
        {
            Env.Context.Get("session").Remove("sale_order_id");
            Env.Context.Get("session").Remove("website_sale_cart_quantity");
            return saleOrderModel.Browse(new int[] { });
        }

        // Only set when neeeded
        var pricelistId = 0;

        var partnerSudo = Env.User.PartnerId;

        if (saleOrderSudo == null)
        {
            var soData = PrepareSaleOrderValues(partnerSudo);
            saleOrderSudo = saleOrderModel.WithUser(Env.SuperUserId).Create(soData);
            Env.Context.Get("session")["sale_order_id"] = saleOrderSudo.Id;
            Env.Context.Get("session")["website_sale_cart_quantity"] = saleOrderSudo.CartQuantity;
            return saleOrderSudo.WithUser(Env.User).Sudo();
        }

        if (Env.Context.Get("session")?.Get("sale_order_id") == null)
        {
            Env.Context.Get("session")["sale_order_id"] = saleOrderSudo.Id;
            Env.Context.Get("session")["website_sale_cart_quantity"] = saleOrderSudo.CartQuantity;
        }

        if (saleOrderSudo.PartnerId.Id != partnerSudo.Id && website.PartnerId.Id != partnerSudo.Id)
        {
            var previousFiscalPosition = saleOrderSudo.FiscalPositionId;
            var previousPricelist = saleOrderSudo.PricelistId;

            // Reset the session pricelist according to logged partner pl
            Env.Context.Get("session").Remove("website_sale_current_pl");
            // Force recomputation of the website pricelist after reset
            this.InvalidateRecordset(new List<string> { "PricelistId" });
            pricelistId = this.PricelistId.Id;
            Env.Context.Get("session")["website_sale_current_pl"] = pricelistId;

            // change the partner, and trigger the computes (fpos)
            saleOrderSudo.Write(new Dictionary<string, object>
            {
                { "PartnerId", partnerSudo.Id },
                { "PricelistId", pricelistId },
            });

            if (saleOrderSudo.FiscalPositionId != previousFiscalPosition)
            {
                saleOrderSudo.OrderLine.ComputeTaxId();
            }

            if (saleOrderSudo.PricelistId != previousPricelist)
            {
                updatePricelist = true;
            }
        }
        else if (updatePricelist)
        {
            // Only compute pricelist if needed
            pricelistId = this.PricelistId.Id;
        }

        if (updatePricelist)
        {
            Env.Context.Get("session")["website_sale_current_pl"] = pricelistId;
            saleOrderSudo.Write(new Dictionary<string, object>
            {
                { "PricelistId", pricelistId },
            });
            saleOrderSudo.RecomputePrices();
        }

        return saleOrderSudo;
    }

    public Dictionary<string, object> PrepareSaleOrderValues(Res.Partner partnerSudo)
    {
        var addr = partnerSudo.AddressGet(new List<string> { "delivery", "invoice" });

        if (!Env.Context.Get("website").IsPublicUser())
        {
            var lastSaleOrder = Env.GetModel<Sale.Order>().Sudo().Search(new List<object>
            {
                new object[] { "PartnerId", "=", partnerSudo.Id },
                new object[] { "WebsiteId", "=", this.Id },
            }, 1, new List<string> { "DateOrder", "Id" }, OrderDirection.Descending);

            if (lastSaleOrder != null && lastSaleOrder.PartnerShippingId.Active)
            {
                addr["delivery"] = lastSaleOrder.PartnerShippingId.Id;
            }

            if (lastSaleOrder != null && lastSaleOrder.PartnerInvoiceId.Active)
            {
                addr["invoice"] = lastSaleOrder.PartnerInvoiceId.Id;
            }
        }

        var affiliateId = Env.Context.Get("session")?.Get("affiliate_id") as int?;
        var salespersonUserSudo = affiliateId != null ? Env.GetModel<Res.Users>().Sudo().Browse(affiliateId).Exists() : null;
        if (salespersonUserSudo == null)
        {
            salespersonUserSudo = SalespersonId > 0 ? Env.GetModel<Res.Users>().Sudo().Browse(SalespersonId) : partnerSudo.ParentId?.UserId ?? partnerSudo.UserId;
        }

        return new Dictionary<string, object>
        {
            { "CompanyId", this.CompanyId.Id },
            { "FiscalPositionId", this.FiscalPositionId.Id },
            { "PartnerId", partnerSudo.Id },
            { "PartnerInvoiceId", addr["invoice"] },
            { "PartnerShippingId", addr["delivery"] },
            { "PricelistId", this.PricelistId.Id },
            { "TeamId", this.SalesteamId.Id },
            { "UserId", salespersonUserSudo?.Id },
            { "WebsiteId", this.Id },
        };
    }

    public Account.FiscalPosition GetCurrentFiscalPosition()
    {
        var accountFiscalPositionModel = Env.GetModel<Account.FiscalPosition>().Sudo();
        var fpos = accountFiscalPositionModel;
        var partnerSudo = Env.User.PartnerId;

        if (Env.Context.Get("geoip")?.Get("country_code") is string countryCode && this.PartnerId.Id == partnerSudo.Id)
        {
            var country = Env.GetModel<Res.Country>().Search(new List<object> { new object[] { "Code", "=", countryCode } }, 1);
            var partnerGeoIp = Env.GetModel<Res.Partner>().New(new Dictionary<string, object> { { "CountryId", country?.Id } });
            fpos = accountFiscalPositionModel.GetFiscalPosition(partnerGeoIp);
        }

        if (fpos == null)
        {
            fpos = accountFiscalPositionModel.GetFiscalPosition(partnerSudo);
        }

        return fpos;
    }

    public void SaleReset()
    {
        Env.Context.Get("session").Remove("sale_order_id");
        Env.Context.Get("session").Remove("website_sale_current_pl");
        Env.Context.Get("session").Remove("website_sale_cart_quantity");
    }

    public Ir.Actions.Actions ActionDashboardRedirect()
    {
        if (Env.User.HasGroup("SalesTeam.GroupSaleSalesman"))
        {
            return Env.GetModel<Ir.Actions.Actions>().ForXmlId("Website.BackendDashboard");
        }

        return base.ActionDashboardRedirect();
    }

    public List<Tuple<string, string, string>> GetSuggestedControllers()
    {
        var suggestedControllers = base.GetSuggestedControllers();
        suggestedControllers.Add(new Tuple<string, string, string>(Env.Translate("eCommerce"), "/shop", "website_sale"));
        return suggestedControllers;
    }

    public List<object> SearchGetDetails(string searchType, string order, Dictionary<string, object> options)
    {
        var result = base.SearchGetDetails(searchType, order, options);

        if (!HasEcommerceAccess())
        {
            return result;
        }

        if (searchType == "products" || searchType == "product_categories_only" || searchType == "all")
        {
            result.Add(Env.GetModel<Product.PublicCategory>().SearchGetDetail(this, order, options));
        }

        if (searchType == "products" || searchType == "products_only" || searchType == "all")
        {
            result.Add(Env.GetModel<Product.Template>().SearchGetDetail(this, order, options));
        }

        return result;
    }

    public Tuple<int, int> GetProductPageProportions()
    {
        var imageWidthMap = new Dictionary<string, Tuple<int, int>>
        {
            { "none", new Tuple<int, int>(0, 12) },
            { "50_pc", new Tuple<int, int>(6, 6) },
            { "66_pc", new Tuple<int, int>(8, 4) },
            { "100_pc", new Tuple<int, int>(12, 12) },
        };

        return imageWidthMap[this.ProductPageImageWidth];
    }

    public string GetProductPageGridImageClasses()
    {
        var spacingMap = new Dictionary<string, string>
        {
            { "none", "p-0" },
            { "small", "p-2" },
            { "medium", "p-3" },
            { "big", "p-4" },
        };

        var columnsMap = new Dictionary<int, string>
        {
            { 1, "col-12" },
            { 2, "col-6" },
            { 3, "col-4" },
        };

        return spacingMap[this.ProductPageImageSpacing] + " " + columnsMap[this.ProductPageGridColumns];
    }

    public void SendAbandonedCartEmail()
    {
        var websites = Env.GetModel<Website>().Search(new List<object> { });

        foreach (var website in websites)
        {
            if (!website.SendAbandonedCartEmail)
            {
                continue;
            }

            var allAbandonedCarts = Env.GetModel<Sale.Order>().Search(new List<object>
            {
                new object[] { "IsAbandonedCart", "=", true },
                new object[] { "CartRecoveryEmailSent", "=", false },
                new object[] { "WebsiteId", "=", website.Id },
            });

            if (!allAbandonedCarts.Any())
            {
                continue;
            }

            var abandonedCarts = allAbandonedCarts.FilterCanSendAbandonedCartMail();
            (allAbandonedCarts - abandonedCarts).ForEach(x => x.CartRecoveryEmailSent = true);
            foreach (var saleOrder in abandonedCarts)
            {
                var template = Env.Ref("WebsiteSale.MailTemplateSaleCartRecovery");
                template.SendMail(saleOrder.Id, new Dictionary<string, object> { { "email_to", saleOrder.PartnerId.Email } });
                saleOrder.CartRecoveryEmailSent = true;
            }
        }
    }

    public bool DisplayPartnerB2BFields()
    {
        return IsViewActive("website_sale.address_b2b");
    }

    public List<Tuple<List<string>, Dictionary<string, object>>> GetCheckoutStepList()
    {
        var isExtraStepActive = Env.ViewRef("website_sale.extra_info").Active;
        var redirectToSignIn = AccountOnCheckout == "mandatory" && Env.Context.Get("website").IsPublicUser();

        var steps = new List<Tuple<List<string>, Dictionary<string, object>>>
        {
            new Tuple<List<string>, Dictionary<string, object>>(new List<string> { "website_sale.cart" }, new Dictionary<string, object>
            {
                { "name", Env.Translate("Review Order") },
                { "current_href", "/shop/cart" },
                { "main_button", redirectToSignIn ? Env.Translate("Sign In") : Env.Translate("Checkout") },
                { "main_button_href", (redirectToSignIn ? "/web/login?redirect=" : "") + "/shop/checkout?express=1" },
                { "back_button", Env.Translate("Continue shopping") },
                { "back_button_href", "/shop" },
            }),
            new Tuple<List<string>, Dictionary<string, object>>(new List<string> { "website_sale.checkout", "website_sale.address" }, new Dictionary<string, object>
            {
                { "name", Env.Translate("Delivery") },
                { "current_href", "/shop/checkout" },
                { "main_button", Env.Translate("Confirm") },
                { "main_button_href", isExtraStepActive ? "/shop/extra_info" : "/shop/confirm_order" },
                { "back_button", Env.Translate("Back to cart") },
                { "back_button_href", "/shop/cart" },
            }),
        };

        if (isExtraStepActive)
        {
            steps.Add(new Tuple<List<string>, Dictionary<string, object>>(new List<string> { "website_sale.extra_info" }, new Dictionary<string, object>
            {
                { "name", Env.Translate("Extra Info") },
                { "current_href", "/shop/extra_info" },
                { "main_button", Env.Translate("Continue checkout") },
                { "main_button_href", "/shop/confirm_order" },
                { "back_button", Env.Translate("Back to delivery") },
                { "back_button_href", "/shop/checkout" },
            }));
        }

        steps.Add(new Tuple<List<string>, Dictionary<string, object>>(new List<string> { "website_sale.payment" }, new Dictionary<string, object>
        {
            { "name", Env.Translate("Payment") },
            { "current_href", "/shop/payment" },
            { "back_button", Env.Translate("Back to delivery") },
            { "back_button_href", "/shop/checkout" },
        }));

        return steps;
    }

    public object GetCheckoutSteps(string currentStep = null)
    {
        var steps = GetCheckoutStepList();

        if (currentStep != null)
        {
            return steps.FirstOrDefault(x => x.Item1.Contains(currentStep))?.Item2;
        }

        return steps;
    }

    public bool HasEcommerceAccess()
    {
        return !Env.User.IsPublic() || EcommerceAccess == "everyone";
    }
}
