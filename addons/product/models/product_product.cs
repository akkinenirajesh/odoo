csharp
public partial class ProductProduct
{
    public virtual void ComputeCanImageVariant1024BeZoomed()
    {
        this.CanImageVariant1024BeZoomed = this.ImageVariant1920 != null &&
            Tools.IsImageSizeAbove(this.ImageVariant1920, this.ImageVariant1024);
    }

    public virtual void SetTemplateField(string templateField, string variantField)
    {
        if ((!this.GetValue<byte[]>(templateField) && !this.GetValue<byte[]>(variantField)) ||
            (this.GetValue<byte[]>(templateField) && !this.ProductTmplId.GetValue<byte[]>(templateField)) ||
            Env.Get<ProductProduct>().SearchCount(new[] {
                ("ProductTmplId", "=", this.ProductTmplId.Id),
                ("Active", "=", true)
            }) <= 1)
        {
            this.SetValue<byte[]>(variantField, null);
            this.ProductTmplId.SetValue<byte[]>(templateField, this.GetValue<byte[]>(templateField));
        }
        else
        {
            this.SetValue<byte[]>(variantField, this.GetValue<byte[]>(templateField));
        }
    }

    public virtual void ComputeWriteDate()
    {
        this.WriteDate = DateTime.Now > this.WriteDate ? DateTime.Now : this.ProductTmplId.WriteDate;
    }

    public virtual void ComputeImage1920()
    {
        this.Image1920 = this.ImageVariant1920 ?? this.ProductTmplId.Image1920;
    }

    public virtual void SetImage1920(byte[] value)
    {
        SetTemplateField("Image1920", "ImageVariant1920");
    }

    public virtual void ComputeImage1024()
    {
        this.Image1024 = this.ImageVariant1024 ?? this.ProductTmplId.Image1024;
    }

    public virtual void ComputeImage512()
    {
        this.Image512 = this.ImageVariant512 ?? this.ProductTmplId.Image512;
    }

    public virtual void ComputeImage256()
    {
        this.Image256 = this.ImageVariant256 ?? this.ProductTmplId.Image256;
    }

    public virtual void ComputeImage128()
    {
        this.Image128 = this.ImageVariant128 ?? this.ProductTmplId.Image128;
    }

    public virtual void ComputeCanImage1024BeZoomed()
    {
        this.CanImage1024BeZoomed = this.ImageVariant1920 != null ?
            this.CanImageVariant1024BeZoomed : this.ProductTmplId.CanImage1024BeZoomed;
    }

    public virtual string GetPlaceholderFilename(string field)
    {
        if (new[] { "Image1920", "Image1024", "Image512", "Image256", "Image128" }.Contains(field))
        {
            return "product/static/img/placeholder_thumbnail.png";
        }
        return base.GetPlaceholderFilename(field);
    }

    public virtual void Init()
    {
        Env.Cr.Execute("CREATE UNIQUE INDEX IF NOT EXISTS product_product_combination_unique ON %s (product_tmpl_id, combination_indices) WHERE active is true"
            % this._table);
    }

    public virtual List<Tuple<int, List<string>>> GetBarcodesByCompany()
    {
        var groupedProducts = Env.Get<ProductProduct>().GroupBy(x => x.CompanyId.Id);
        return groupedProducts.Select(group => new Tuple<int, List<string>>(group.Key, group.Select(p => p.Barcode).ToList())).ToList();
    }

    public virtual List<object> GetBarcodeSearchDomain(List<string> barcodesWithinCompany, int companyId)
    {
        var domain = new List<object> { ("Barcode", "in", barcodesWithinCompany) };
        if (companyId != 0)
        {
            domain.Add(("CompanyId", "in", new[] { 0, companyId }));
        }
        return domain;
    }

    public virtual void CheckDuplicatedProductBarcodes(List<string> barcodesWithinCompany, int companyId)
    {
        var domain = GetBarcodeSearchDomain(barcodesWithinCompany, companyId);
        var productsByBarcode = Env.Get<ProductProduct>().Sudo().ReadGroup(domain, new[] { "Barcode", "Id:array_agg" }, new[] { "Barcode" });

        var duplicatesAsStr = productsByBarcode
            .Where(record => record["Id"].Length > 1)
            .Select(record => string.Format(
                "- Barcode \"{0}\" already assigned to product(s): {1}",
                record["Barcode"],
                string.Join(", ", Env.Get<ProductProduct>().Search(new[] { ("Id", "in", record["Id"]) }).Select(p => p.DisplayName).ToList())
            ))
            .Aggregate(string.Empty, (current, next) => current + "\n" + next);
        if (!string.IsNullOrEmpty(duplicatesAsStr))
        {
            duplicatesAsStr += "\n\nNote: products that you don't have access to will not be shown above.";
            throw new ValidationError(string.Format("Barcode(s) already assigned:\n\n{0}", duplicatesAsStr));
        }
    }

    public virtual void CheckDuplicatedPackagingBarcodes(List<string> barcodesWithinCompany, int companyId)
    {
        var packagingDomain = GetBarcodeSearchDomain(barcodesWithinCompany, companyId);
        if (Env.Get<ProductPackaging>().Sudo().SearchCount(packagingDomain, 1) > 0)
        {
            throw new ValidationError("A packaging already uses the barcode");
        }
    }

    public virtual void CheckBarcodeUniqueness()
    {
        foreach (var (companyId, barcodesWithinCompany) in GetBarcodesByCompany())
        {
            CheckDuplicatedProductBarcodes(barcodesWithinCompany, companyId);
            CheckDuplicatedPackagingBarcodes(barcodesWithinCompany, companyId);
        }
    }

    public virtual bool GetInvoicePolicy()
    {
        return false;
    }

    public virtual void ComputeCombinationIndices()
    {
        this.CombinationIndices = this.ProductTemplateAttributeValueIds.Ids2Str();
    }

    public virtual void ComputeIsProductVariant()
    {
        this.IsProductVariant = true;
    }

    public virtual void OnChangeLstPrice()
    {
        var uom = Env.Context.Get<int>("uom");
        var value = uom != 0 ? Env.Get<UomUom>().Browse(uom)._ComputePrice(this.LstPrice, this.UomId) : this.LstPrice;
        value -= this.PriceExtra;
        this.SetValue<decimal>("ListPrice", value);
    }

    public virtual void ComputeProductPriceExtra()
    {
        this.PriceExtra = this.ProductTemplateAttributeValueIds.Sum(x => x.PriceExtra);
    }

    public virtual void ComputeProductLstPrice()
    {
        var uom = Env.Context.Get<int>("uom");
        var toUom = uom != 0 ? Env.Get<UomUom>().Browse(uom) : null;
        var listPrice = toUom != null ? this.UomId._ComputePrice(this.ListPrice, toUom) : this.ListPrice;
        this.LstPrice = listPrice + this.PriceExtra;
    }

    public virtual void ComputeProductCode()
    {
        this.Code = this.DefaultCode;
        if (Env.Get<IrModelAccess>().Check("Product.Supplierinfo", "read", false))
        {
            foreach (var supplierInfo in this.SellerIds)
            {
                if (supplierInfo.PartnerId.Id == Env.Context.Get<int>("partner_id"))
                {
                    this.Code = supplierInfo.ProductCode ?? this.DefaultCode;
                    break;
                }
            }
        }
    }

    public virtual void ComputePartnerRef()
    {
        foreach (var supplierInfo in this.SellerIds)
        {
            if (supplierInfo.PartnerId.Id == Env.Context.Get<int>("partner_id"))
            {
                this.PartnerRef = string.Format("{0}{1}", this.Code != null ? string.Format("[{0}] ", this.Code) : string.Empty,
                    supplierInfo.ProductName ?? this.DefaultCode ?? this.Name);
                break;
            }
        }
        if (string.IsNullOrEmpty(this.PartnerRef))
        {
            this.PartnerRef = this.DisplayName;
        }
    }

    public virtual void ComputeVariantItemCount()
    {
        var domain = new[] {
            ("PricelistId.Active", "=", true),
            "|",
                "&", ("ProductTmplId", "=", this.ProductTmplId.Id), ("AppliedOn", "=", 1),
                "&", ("ProductId", "=", this.Id), ("AppliedOn", "=", 0)
        };
        this.PricelistItemCount = Env.Get<ProductPricelistItem>().SearchCount(domain);
    }

    public virtual void ComputeProductDocumentCount()
    {
        this.ProductDocumentCount = Env.Get<ProductDocument>().SearchCount(new[] {
            ("ResModel", "=", "Product.ProductProduct"),
            ("ResId", "=", this.Id)
        });
    }

    public virtual void ComputeAllProductTagIds()
    {
        this.AllProductTagIds = (this.ProductTagIds | this.AdditionalProductTagIds).OrderBy(x => x.Sequence).ToList();
    }

    public virtual List<object> SearchAllProductTagIds(string operatorName, object operand)
    {
        if (new[] { "!", "not in", "!in" }.Contains(operatorName))
        {
            return new List<object> {
                ("ProductTagIds", operatorName, operand),
                ("AdditionalProductTagIds", operatorName, operand)
            };
        }
        return new List<object> {
            "|",
                ("ProductTagIds", operatorName, operand),
                ("AdditionalProductTagIds", operatorName, operand)
        };
    }

    public virtual void OnChangeUomId()
    {
        if (this.UomId != null)
        {
            this.UomPoId = this.UomId.Id;
        }
    }

    public virtual void OnChangeUomPoId()
    {
        if (this.UomId != null && this.UomPoId != null && this.UomId.CategoryId != this.UomPoId.CategoryId)
        {
            this.UomPoId = this.UomId;
        }
    }

    public virtual void OnChangeDefaultCode()
    {
        if (string.IsNullOrEmpty(this.DefaultCode))
        {
            return;
        }
        var domain = new[] { ("DefaultCode", "=", this.DefaultCode) };
        if (this.Id.Origin != null)
        {
            domain.Add(("Id", "!=", this.Id.Origin));
        }
        if (Env.Get<ProductProduct>().SearchCount(domain, 1) > 0)
        {
            throw new Warning("The Reference '{0}' already exists.".Format(this.DefaultCode));
        }
    }

    public virtual ProductProduct Create(Dictionary<string, object> vals)
    {
        return Env.Get<ProductProduct>().WithContext(new Dictionary<string, object> { { "create_product_product", false } }).Create(vals);
    }

    public virtual void Write(Dictionary<string, object> values)
    {
        base.Write(values);
        if (values.ContainsKey("ProductTemplateAttributeValueIds"))
        {
            Env.Registry.ClearCache();
        }
        else if (values.ContainsKey("Active"))
        {
            Env.Registry.ClearCache();
        }
    }

    public virtual void Unlink()
    {
        var unlinkProducts = Env.Get<ProductProduct>();
        var unlinkTemplates = Env.Get<ProductTemplate>();
        foreach (var product in this)
        {
            if (product.ImageVariant1920 != null && product.ProductTmplId.Image1920 == null)
            {
                product.ProductTmplId.Image1920 = product.ImageVariant1920;
            }
            if (!product.Exists())
            {
                continue;
            }
            var otherProducts = Env.Get<ProductProduct>().Search(new[] { ("ProductTmplId", "=", product.ProductTmplId.Id), ("Id", "!=", product.Id) });
            if (!otherProducts && !product.ProductTmplId.HasDynamicAttributes())
            {
                unlinkTemplates |= product.ProductTmplId;
            }
            unlinkProducts |= product;
        }
        base.Unlink();
        unlinkTemplates.Unlink();
        Env.Registry.ClearCache();
    }

    public virtual ProductProduct FilterToUnlink(bool checkAccess = true)
    {
        return this;
    }

    public virtual void UnlinkOrArchive(bool checkAccess = true)
    {
        if (checkAccess)
        {
            CheckAccessRights("unlink");
            CheckAccessRule("unlink");
            CheckAccessRights("write");
            CheckAccessRule("write");
            this = Sudo();
            var toUnlink = FilterToUnlink();
            var toArchive = this - toUnlink;
            toArchive.SetValue<bool>("Active", false);
            this = toUnlink;
        }

        using (Env.Cr.Savepoint(), Tools.MuteLogger("odoo.sql_db"))
        {
            try
            {
                this.Unlink();
            }
            catch (Exception)
            {
                if (this.Length > 1)
                {
                    this.Take(this.Length / 2).UnlinkOrArchive(false);
                    this.Skip(this.Length / 2).UnlinkOrArchive(false);
                }
                else
                {
                    if (this.GetValue<bool>("Active"))
                    {
                        this.SetValue<bool>("Active", false);
                    }
                }
            }
        }
    }

    public virtual ProductProduct Copy(Dictionary<string, object> defaultValues = null)
    {
        var templates = this.Select(x => x.ProductTmplId).ToList();
        var templatesToCopy = Env.Get<ProductTemplate>().Concat(templates);
        var newTemplates = templatesToCopy.Copy(defaultValues);
        var newProducts = Env.Get<ProductProduct>();
        foreach (var newTemplate in newTemplates)
        {
            newProducts += newTemplate.ProductVariantId ?? newTemplate._CreateFirstProductVariant();
        }
        return newProducts;
    }

    public virtual List<object> Search(List<object> domain, int offset = 0, int? limit = null, string order = null, int? accessRightsUid = null)
    {
        if (Env.Context.ContainsKey("search_default_categ_id"))
        {
            domain = domain.ToList();
            domain.Add(("CategId", "child_of", Env.Context.Get<int>("search_default_categ_id")));
        }
        return base.Search(domain, offset, limit, order, accessRightsUid);
    }

    public virtual void ComputeDisplayName()
    {
        string GetDisplayName(string name, string code)
        {
            return Env.Context.Get<bool>("display_default_code", true) && code != null ? string.Format("[{0}] {1}", code, name) : name;
        }

        var partnerId = Env.Context.Get<int>("partner_id");
        var partnerIds = partnerId != 0 ? new[] { partnerId, Env.Get<ResPartner>().Browse(partnerId).CommercialPartnerId.Id } : new int[0];
        var companyId = Env.Context.Get<int>("company_id");

        CheckAccessRights("read");
        CheckAccessRule("read");

        var productTemplateIds = this.Sudo().ProductTmplId.Select(x => x.Id).ToList();

        List<Supplierinfo> supplierInfo = new List<Supplierinfo>();
        if (partnerIds.Length > 0)
        {
            supplierInfo = Env.Get<Supplierinfo>().Sudo().SearchFetch(new[] {
                ("ProductTmplId", "in", productTemplateIds),
                ("PartnerId", "in", partnerIds)
            }, new[] { "ProductTmplId", "ProductId", "CompanyId", "ProductName", "ProductCode" });

            var supplierInfoByTemplate = new Dictionary<int, List<Supplierinfo>>();
            foreach (var r in supplierInfo)
            {
                if (!supplierInfoByTemplate.ContainsKey(r.ProductTmplId.Id))
                {
                    supplierInfoByTemplate.Add(r.ProductTmplId.Id, new List<Supplierinfo>());
                }
                supplierInfoByTemplate[r.ProductTmplId.Id].Add(r);
            }
        }

        foreach (var product in this.Sudo())
        {
            var variant = product.ProductTemplateAttributeValueIds._GetCombinationName();
            var name = variant != null ? string.Format("{0} ({1})", product.Name, variant) : product.Name;
            var sellers = Env.Context.Get<List<int>>("seller_id") != null ? Env.Get<Supplierinfo>().Sudo().Browse(Env.Context.Get<List<int>>("seller_id")) : new Supplierinfo[0];

            if (sellers.Length == 0 && partnerIds.Length > 0)
            {
                sellers = supplierInfoByTemplate.ContainsKey(product.ProductTmplId.Id) ? supplierInfoByTemplate[product.ProductTmplId.Id]
                    .Where(x => x.ProductId != null && x.ProductId == product || (x.ProductId == null && x.ProductTmplId == product.ProductTmplId)).ToList() : new Supplierinfo[0];
            }

            if (companyId != 0)
            {
                sellers = sellers.Where(x => x.CompanyId.Id == companyId || x.CompanyId.Id == 0).ToList();
            }

            if (sellers.Length > 0)
            {
                var temp = new List<string>();
                foreach (var s in sellers)
                {
                    var sellerVariant = !string.IsNullOrEmpty(s.ProductName) ?
                        variant != null ? string.Format("{0} ({1})", s.ProductName, variant) : s.ProductName : null;
                    temp.Add(GetDisplayName(sellerVariant ?? name, s.ProductCode ?? product.DefaultCode));
                }

                product.DisplayName = string.Join(", ", temp.Distinct().ToList());
            }
            else
            {
                product.DisplayName = GetDisplayName(name, product.DefaultCode);
            }
        }
    }

    public virtual List<object> NameSearch(string name, List<object> domain = null, string operatorName = "ilike", int? limit = null, string order = null)
    {
        domain = domain ?? new List<object>();
        if (!string.IsNullOrEmpty(name))
        {
            var positiveOperators = new[] { "=", "ilike", "=ilike", "like", "=like" };
            var productIds = new List<int>();
            if (positiveOperators.Contains(operatorName))
            {
                productIds = this.Search(new[] { ("DefaultCode", "=", name) }.Concat(domain).ToList(), limit: limit, order: order).Select(x => x.Id).ToList();
                if (productIds.Count == 0)
                {
                    productIds = this.Search(new[] { ("Barcode", "=", name) }.Concat(domain).ToList(), limit: limit, order: order).Select(x => x.Id).ToList();
                }
            }
            if (productIds.Count == 0 && !new[] { "!", "not in", "!in" }.Contains(operatorName))
            {
                productIds = this.Search(domain.Concat(new[] { ("DefaultCode", operatorName, name) }).ToList(), limit: limit, order: order).Select(x => x.Id).ToList();
                if (limit == null || productIds.Count < limit)
                {
                    var limit2 = limit != null ? limit - productIds.Count : 0;
                    var product2Ids = this.Search(domain.Concat(new[] { ("Name", operatorName, name), ("Id", "not in", productIds) }).ToList(), limit: limit2, order: order).Select(x => x.Id).ToList();
                    productIds.AddRange(product2Ids);
                }
            }
            else if (productIds.Count == 0 && new[] { "!", "not in", "!in" }.Contains(operatorName))
            {
                var domain2 = new List<object> {
                    "&",
                        ("DefaultCode", operatorName, name),
                        ("Name", operatorName, name),
                    "|",
                        "&", ("DefaultCode", "=", null), ("Name", operatorName, name)
                };
                domain2 = domain2.Concat(domain).ToList();
                productIds = this.Search(domain2, limit: limit, order: order).Select(x => x.Id).ToList();
            }
            if (productIds.Count == 0 && positiveOperators.Contains(operatorName))
            {
                var ptrn = new Regex(r"(\[(.*?)\])");
                var res = ptrn.Match(name);
                if (res.Success)
                {
                    productIds = this.Search(new[] { ("DefaultCode", "=", res.Groups[2].Value) }.Concat(domain).ToList(), limit: limit, order: order).Select(x => x.Id).ToList();
                }
            }
            if (productIds.Count == 0 && Env.Context.ContainsKey("partner_id"))
            {
                var suppliersIds = Env.Get<Supplierinfo>()._Search(new[] {
                    ("PartnerId", "=", Env.Context.Get<int>("partner_id")),
                    "|",
                        ("ProductCode", operatorName, name),
                        ("ProductName", operatorName, name)
                });
                if (suppliersIds.Count > 0)
                {
                    productIds = this.Search(new[] { ("ProductTmplId.SellerIds", "in", suppliersIds) }, limit: limit, order: order).Select(x => x.Id).ToList();
                }
            }
        }
        else
        {
            productIds = this.Search(domain, limit: limit, order: order).Select(x => x.Id).ToList();
        }
        return productIds;
    }

    public virtual string ViewHeaderGet(int viewId, string viewType)
    {
        return Env.Context.ContainsKey("categ_id") ? string.Format("Products: {0}",
            Env.Get<ProductCategory>().Browse(Env.Context.Get<int>("categ_id")).Name) : base.ViewHeaderGet(viewId, viewType);
    }

    public virtual Action ActionOpenLabelLayout()
    {
        var action = Env.Get<IrActionsActWindow>()._ForXmlId("product.action_open_label_layout");
        action.Context = new Dictionary<string, object> { { "default_product_ids", this.Select(x => x.Id).ToList() } };
        return action;
    }

    public virtual Action OpenPricelistRules()
    {
        var domain = new[] {
            "|",
                "&", ("ProductTmplId", "=", this.ProductTmplId.Id), ("AppliedOn", "=", 1),
                "&", ("ProductId", "=", this.Id), ("AppliedOn", "=", 0)
        };
        return new Action {
            Name = "Price Rules",
            ViewMode = "tree,form",
            Views = new[] { (Env.Ref("product.product_pricelist_item_tree_view_from_product").Id, "tree"), (0, "form") },
            ResModel = "product.pricelist.item",
            Type = "ir.actions.act_window",
            Target = "current",
            Domain = domain,
            Context = new Dictionary<string, object> {
                { "default_product_id", this.Id },
                { "default_applied_on", 0 },
                { "search_default_visible", true }
            }
        };
    }

    public virtual Action OpenProductTemplate()
    {
        return new Action {
            Type = "ir.actions.act_window",
            ResModel = "product.template",
            ViewMode = "form",
            ResId = this.ProductTmplId.Id,
            Target = "new"
        };
    }

    public virtual Action ActionOpenDocuments()
    {
        var res = this.ProductTmplId.ActionOpenDocuments();
        res.Context.Add("default_res_model", "Product.ProductProduct");
        res.Context.Add("default_res_id", this.Id);
        res.Context.Add("search_default_context_variant", true);
        return res;
    }

    public virtual List<Supplierinfo> PrepareSellers(Dictionary<string, object> params = null)
    {
        var sellers = this.SellerIds.Where(s => s.PartnerId.Active && (s.ProductId == null || s.ProductId == this)).ToList();
        return sellers.OrderBy(s => (s.Sequence, -s.MinQty, s.Price, s.Id)).ToList();
    }

    public virtual List<Supplierinfo> GetFilteredSellers(int? partnerId = null, decimal quantity = 0.0m, DateTime? date = null, int? uomId = null, Dictionary<string, object> params = null)
    {
        if (date == null)
        {
            date = DateTime.Now;
        }
        var precision = Env.Get<DecimalPrecision>().PrecisionGet("Product Unit of Measure");

        var sellersFiltered = PrepareSellers(params);
        sellersFiltered = sellersFiltered.Where(s => s.CompanyId == null || s.CompanyId.Id == Env.Company.Id).ToList();
        var sellers = Env.Get<Supplierinfo>();

        foreach (var seller in sellersFiltered)
        {
            var quantityUomSeller = quantity;
            if (quantityUomSeller != 0 && uomId != 0 && uomId != seller.ProductUom.Id)
            {
                quantityUomSeller = Env.Get<UomUom>().Browse(uomId)._ComputeQuantity(quantityUomSeller, seller.ProductUom);
            }
            if (seller.DateStart != null && seller.DateStart > date)
            {
                continue;
            }
            if (seller.DateEnd != null && seller.DateEnd < date)
            {
                continue;
            }
            if (partnerId != null && !new[] { partnerId, Env.Get<ResPartner>().Browse(partnerId).ParentId.Id }.Contains(seller.PartnerId.Id))
            {
                continue;
            }
            if (quantity != 0 && Decimal.Compare(quantityUomSeller, seller.MinQty) == -1)
            {
                continue;
            }
            if (seller.ProductId != null && seller.ProductId != this)
            {
                continue;
            }
            sellers |= seller;
        }
        return sellers.OrderBy(s => (s.Sequence, -s.MinQty, s.Price, s.Id)).ToList();
    }

    public virtual List<Supplierinfo> SelectSeller(int? partnerId = null, decimal quantity = 0.0m, DateTime? date = null, int? uomId = null, string orderedBy = "price_discounted", Dictionary<string, object> params = null)
    {
        var sortKey = new Func<Supplierinfo, Tuple<decimal, int, int>>((s) => new Tuple<decimal, int, int>(s.PriceDiscounted, s.Sequence, s.Id));
        if (orderedBy != "price_discounted")
        {
            sortKey = new Func<Supplierinfo, Tuple<object, decimal, int, int>>((s) => new Tuple<object, decimal, int, int>(s.GetValue(orderedBy), s.PriceDiscounted, s.Sequence, s.Id));
        }
        var sellers = GetFilteredSellers(partnerId, quantity, date, uomId, params);
        var res = Env.Get<Supplierinfo>();
        foreach (var seller in sellers)
        {
            if (res.Length == 0 || res.PartnerId == seller.PartnerId)
            {
                res |= seller;
            }
        }
        return res.OrderBy(sortKey).Take(1).ToList();
    }

    public virtual Dictionary<string, object> GetProductPriceContext(ProductTemplateAttributeValue combination)
    {
        var res = new Dictionary<string, object>();
        var noVariantAttributesPriceExtra = combination.Where(ptav =>
            ptav.PriceExtra != 0 && ptav.ProductTmplId == this.ProductTmplId &&
            !this.ProductTemplateAttributeValueIds.Contains(ptav)).Select(x => x.PriceExtra).ToList();
        if (noVariantAttributesPriceExtra.Count > 0)
        {
            res.Add("no_variant_attributes_price_extra", noVariantAttributesPrice