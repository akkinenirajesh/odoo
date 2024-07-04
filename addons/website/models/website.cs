csharp
public partial class Website
{
    public Website()
    {
    }

    public void ComputeLanguageCount()
    {
        this.LanguageCount = this.LanguageIds.Count;
    }

    public void ComputeHasSocialDefaultImage()
    {
        this.HasSocialDefaultImage = this.SocialDefaultImage != null;
    }

    public void ComputeMenu()
    {
        var menus = Env.Get<WebsiteMenu>().Search(m => m.WebsiteId == this.Id);

        // use field parent_id (1 query) to determine field child_id (2 queries by level)"
        foreach (var menu in menus)
        {
            menu.ChildId = new List<int>();
        }
        foreach (var menu in menus)
        {
            // don't add child menu if parent is forbidden
            if (menu.ParentId != 0 && menus.Any(m => m.Id == menu.ParentId))
            {
                menus.First(m => m.Id == menu.ParentId).ChildId.Add(menu.Id);
            }
        }

        // prefetch every website.page and ir.ui.view at once
        menus.ForEach(m => m.IsVisible);

        var topMenus = menus.Where(m => m.ParentId == 0).ToList();
        this.MenuId = topMenus.Count > 0 ? topMenus[0].Id : 0;
    }

    public List<int> GetMenuIds()
    {
        return Env.Get<WebsiteMenu>().Search(m => m.WebsiteId == this.Id).Select(m => m.Id).ToList();
    }

    public bool IsMenuCacheDisabled()
    {
        // Checks if the website menu contains a record like url.
        // :return: True if the menu contains a record like url
        return GetMenuIds().Any(m =>
        {
            var menu = Env.Get<WebsiteMenu>().Browse(m);
            return (menu.Url != null && Regex.IsMatch(menu.Url, @"[/](([^/=?&]+-)?[0-9]+)([/]|$)") || menu.GroupIds != null);
        });
    }

    public void CheckHomepageUrl()
    {
        if (this.HomepageUrl != null && !this.HomepageUrl.StartsWith("/"))
        {
            throw new ValidationException("The homepage URL should be relative and start with '/'");
        }
    }

    public void UnlinkExceptDefaultWebsite()
    {
        var defaultWebsite = Env.Get<Website>().GetDefaultWebsite();
        if (defaultWebsite != null && this.Any(w => w.Id == defaultWebsite.Id))
        {
            throw new UserError("You cannot delete default website {0}. Try to change its settings instead".Format(defaultWebsite.Name));
        }
    }

    public void Unlink()
    {
        RemoveAttachmentsOnWebsiteUnlink();

        var companies = this.CompanyId;
        base.Unlink();
        companies.ForEach(c => c.ComputeWebsiteId());
    }

    public void RemoveAttachmentsOnWebsiteUnlink()
    {
        // Do not delete invoices, delete what's strictly necessary
        var attachmentsToUnlink = Env.Get<Attachment>().Search(a => a.WebsiteId == this.Id &&
                                                                  (a.Key != null || a.Url != null && a.Url.StartsWith("/_custom/%") || a.Url.Contains(".assets\\_")));
        attachmentsToUnlink.Unlink();
    }

    public void CreateAndRedirectConfigurator()
    {
        this.EnsureOne();
        var configuratorActionTodo = Env.Get<IrActionsActions>().Get("website.website_configurator_todo");
        configuratorActionTodo.ActionLaunch();
    }

    public bool IsIndexableUrl(string url)
    {
        // Returns True if the given url has to be indexed by search engines.
        // It is considered that the website must be indexed if the domain name
        // matches the URL. We check if they are equal while ignoring the www. and
        // http(s). This is to index the site even if the user put the www. in the
        // settings while he has a configuration that redirects the www. to the
        // naked domain for example (same thing for http and https).

        // :param url: the url to check
        // :return: True if the url has to be indexed, False otherwise
        return GetBaseDomain(url, true) == GetBaseDomain(this.Domain, true);
    }

    public dynamic GetCtaData(string websitePurpose, string websiteType)
    {
        return new { CtaBtnText = false, CtaBtnHref = "/contactus" };
    }

    public dynamic GetThemeConfiguratorSnippets(string themeName)
    {
        // get_manifest() is not available in C#
        // return {
        //     **get_manifest('website')['configurator_snippets'],
        //     **get_manifest(theme_name).get('configurator_snippets', {}),
        // };
        throw new NotImplementedException();
    }

    public void ConfiguratorSetMenuLinks(WebsiteMenu menuCompany, Dictionary<string, dynamic> moduleData)
    {
        var menus = Env.Get<WebsiteMenu>().Search(m => moduleData.Keys.Contains(m.Url) && m.WebsiteId == this.Id);
        foreach (var m in menus)
        {
            m.Sequence = moduleData[m.Url]["sequence"];
        }
    }

    public List<dynamic> ConfiguratorGetFooterLinks()
    {
        return new List<dynamic>
        {
            new { Text = "Privacy Policy", Href = "/privacy" }
        };
    }

    public dynamic ConfiguratorInit()
    {
        var r = new Dictionary<string, dynamic>();
        var company = this.CompanyId;
        var configuratorFeatures = Env.Get<WebsiteConfiguratorFeature>().Search(f => true);
        r["features"] = configuratorFeatures.Select(f => new
        {
            Id = f.Id,
            Name = f.Name,
            Description = f.Description,
            Type = f.PageViewId != 0 ? "page" : "app",
            Icon = f.Icon,
            WebsiteConfigPreselection = f.WebsiteConfigPreselection,
            ModuleState = f.ModuleId.State
        }).ToList();
        r["logo"] = false;
        if (!company.UsesDefaultLogo)
        {
            r["logo"] = company.Logo.ToString();
        }
        try
        {
            // _website_api_rpc is not available in C#
            // result = self._website_api_rpc('/api/website/1/configurator/industries', {'lang': self.env.context.get('lang')})
            // r['industries'] = result['industries']
            throw new NotImplementedException();
        }
        catch (AccessError e)
        {
            // logger.warning(e.args[0])
            // r['industries'] = []
            throw new NotImplementedException();
        }
        return r;
    }

    public List<dynamic> ConfiguratorRecommendedThemes(int industryId, string palette)
    {
        // Module, get_manifest(), and get_themes_domain() are not available in C#
        // domain = Module.get_themes_domain()
        // domain = AND([[('name', '!=', 'theme_default')], domain])
        // client_themes = Module.search(domain).mapped('name')
        // client_themes_img = {t: get_manifest(t).get('images_preview_theme', {}) for t in client_themes if get_manifest(t)}
        // themes_suggested = self._website_api_rpc(
        //     '/api/website/2/configurator/recommended_themes/%s' % (industry_id if industry_id > 0 else ''),
        //     {'client_themes': client_themes_img}
        // )
        // process_svg = self.env['website.configurator.feature']._process_svg
        // for theme in themes_suggested:
        //     theme['svg'] = process_svg(theme['name'], palette, theme.pop('image_urls'))
        // return themes_suggested
        throw new NotImplementedException();
    }

    public void ConfiguratorSkip()
    {
        this.EnsureOne();
        this.ConfiguratorDone = true;
    }

    public void ConfiguratorMissingIndustry(string unknownIndustry)
    {
        // _website_api_rpc is not available in C#
        // self._website_api_rpc(
        //     '/api/website/unknown_industry',
        //     {
        //         'unknown_industry': unknown_industry,
        //         'lang': self.env.context.get('lang'),
        //     }
        // )
        throw new NotImplementedException();
    }

    public dynamic ConfiguratorApply(Dictionary<string, dynamic> kwargs)
    {
        this.EnsureOne();
        var themeName = kwargs["theme_name"];
        var theme = Env.Get<IrModuleModule>().Search(m => m.Name == themeName);
        var redirectUrl = theme.ButtonChooseTheme();

        // Force to refresh env after install of module
        // assert self.env.registry is registry()
        throw new NotImplementedException();

        this.ConfiguratorDone = true;

        // Enable tour
        // tour_asset_id = self.env.ref('website.configurator_tour')
        // tour_asset_id.copy({'key': tour_asset_id.key, 'website_id': website.id, 'active': True})
        throw new NotImplementedException();

        // Set logo from generated attachment or from company's logo
        var logoAttachmentId = kwargs.Get("logo_attachment_id");
        var company = this.CompanyId;
        if (logoAttachmentId != null)
        {
            var attachment = Env.Get<Attachment>().Browse(logoAttachmentId);
            attachment.Write(new { ResModel = "Website", ResField = "Logo", ResId = this.Id });
        }
        else if (logoAttachmentId == null && !company.UsesDefaultLogo)
        {
            this.Logo = company.Logo.ToString();
        }

        // Configure the color palette
        var selectedPalette = kwargs.Get("selected_palette");
        if (selectedPalette != null)
        {
            // Assets is not available in C#
            // Assets = self.env['web_editor.assets']
            // selected_palette_name = selected_palette if isinstance(selected_palette, str) else 'base-1'
            // Assets.make_scss_customization(
            //     '/website/static/src/scss/options/user_values.scss',
            //     {'color-palettes-name': "'%s'" % selected_palette_name}
            // )
            // if isinstance(selected_palette, list):
            //     Assets.make_scss_customization(
            //         '/website/static/src/scss/options/colors/user_color_palette.scss',
            //         {f'o-color-{i}': color for i, color in enumerate(selected_palette, 1)}
            //     )
            throw new NotImplementedException();
        }

        // Update CTA
        var ctaData = GetCtaData(kwargs.Get("website_purpose"), kwargs.Get("website_type"));
        if (ctaData.CtaBtnText != null)
        {
            // xpath_view, parent_view, viewref, create are not available in C#
            // xpath_view = 'website.snippets'
            // parent_view = self.env['website'].with_context(website_id=website.id).viewref(xpath_view)
            // self.env['ir.ui.view'].create({
            //     'name': parent_view.key + ' CTA',
            //     'key': parent_view.key + "_cta",
            //     'inherit_id': parent_view.id,
            //     'website_id': website.id,
            //     'type': 'qweb',
            //     'priority': 32,
            //     'arch_db': """
            //         <data>
            //             <xpath expr="//t[@t-set='cta_btn_href']" position="replace">
            //                 <t t-set="cta_btn_href">%s</t>
            //             </xpath>
            //             <xpath expr="//t[@t-set='cta_btn_text']" position="replace">
            //                 <t t-set="cta_btn_text">%s</t>
            //             </xpath>
            //         </data>
            //     """ % (cta_data['cta_btn_href'], cta_data['cta_btn_text'])
            // })
            throw new NotImplementedException();

            // try:
            //     view_id = self.env['website'].viewref('website.header_call_to_action')
            //     if view_id:
            //         el = etree.fromstring(view_id.arch_db)
            //         btn_cta_el = el.xpath("//a[hasclass('btn_cta')]")
            //         if btn_cta_el:
            //             btn_cta_el[0].attrib['href'] = cta_data['cta_btn_href']
            //             btn_cta_el[0].text = cta_data['cta_btn_text']
            //         view_id.with_context(website_id=website.id).write({'arch_db': etree.tostring(el)})
            // except ValueError as e:
            //     logger.warning(e)
            throw new NotImplementedException();
        }

        // Configure the features
        var features = Env.Get<WebsiteConfiguratorFeature>().Browse(kwargs.Get("selected_features"));

        var menuCompany = Env.Get<WebsiteMenu>();
        if (features.Count(f => f.MenuSequence != 0) > 5 && features.Count(f => f.MenuCompany != null) > 1)
        {
            menuCompany = Env.Get<WebsiteMenu>().Create(new { Name = "Company", ParentId = this.MenuId, WebsiteId = this.Id, Sequence = 40 });
        }

        var pagesViews = new Dictionary<string, int>();
        var modules = Env.Get<IrModuleModule>();
        var moduleData = new Dictionary<string, dynamic>();
        foreach (var feature in features)
        {
            var addMenu = feature.MenuSequence != 0;
            if (feature.ModuleId != null)
            {
                if (feature.ModuleId.State != "installed")
                {
                    modules += feature.ModuleId;
                }
                if (addMenu)
                {
                    if (feature.ModuleId.Name != "website_blog")
                    {
                        moduleData[feature.FeatureUrl] = new { sequence = feature.MenuSequence };
                    }
                    else
                    {
                        var blogs = moduleData.GetOrDefault("#blog", new List<dynamic>());
                        blogs.Add(new { name = feature.Name, sequence = feature.MenuSequence });
                        moduleData["#blog"] = blogs;
                    }
                }
            }
            else if (feature.PageViewId != null)
            {
                // new_page, viewref, create are not available in C#
                // result = self.env['website'].new_page(
                //     name=feature.name,
                //     add_menu=add_menu,
                //     page_values=dict(url=feature.feature_url, is_published=True),
                //     menu_values=add_menu and {
                //         'url': feature.feature_url,
                //         'sequence': feature.menu_sequence,
                //         'parent_id': feature.menu_company and menu_company.id or website.menu_id.id,
                //     },
                //     template=feature.page_view_id.key
                // )
                // pages_views[feature.iap_page_code] = result['view_id']
                throw new NotImplementedException();
            }
        }

        if (modules != null)
        {
            modules.ButtonImmediateInstall();
            // assert self.env.registry is registry()
            throw new NotImplementedException();
        }

        ConfiguratorSetMenuLinks(menuCompany, moduleData);

        // We need to refresh the environment of the website because we installed
        // some new module and we need the overrides of these new menus e.g. for
        // the call to `get_cta_data`.
        var website = Env.Get<Website>().Browse(this.Id);

        // Update footers links, needs to be done after "Features" addition to go
        // through module overrides of `configurator_get_footer_links`.
        var footerLinks = ConfiguratorGetFooterLinks();
        var footerIds = new List<string> { "website.template_footer_contact", "website.template_footer_headline", "website.footer_custom", "website.template_footer_links", "website.template_footer_minimalist" };
        foreach (var footerId in footerIds)
        {
            try
            {
                // viewref, write are not available in C#
                // view_id = self.env['website'].viewref(footer_id)
                // if view_id:
                //     # Deliberately hardcode dynamic code inside the view arch,
                //     # it will be transformed into static nodes after a save/edit
                //     # thanks to the t-ignore in parents node.
                //     arch_string = etree.fromstring(view_id.arch_db)
                //     el = arch_string.xpath("//t[@t-set='configurator_footer_links']")[0]
                //     el.attrib['t-value'] = json.dumps(footer_links)
                //     view_id.with_context(website_id=website.id).write({'arch_db': etree.tostring(arch_string)})
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                // The xml view could have been modified in the backend, we don't
                // want the xpath error to break the configurator feature
                // logger.warning(e)
                throw new NotImplementedException();
            }
        }

        // Load suggestion from iap for selected pages
        var industryId = kwargs.Get("industry_id");
        // _website_api_rpc is not available in C#
        // custom_resources = self._website_api_rpc(
        //     '/api/website/2/configurator/custom_resources/%s' % (industry_id if industry_id > 0 else ''),
        //     {'theme': theme_name}
        // )
        throw new NotImplementedException();

        // Generate text for the pages
        var requestedPages = new HashSet<string>(pagesViews.Keys).Union(new List<string> { "homepage" });
        // get_theme_configurator_snippets is not available in C#
        // configurator_snippets = website.get_theme_configurator_snippets(theme_name)
        throw new NotImplementedException();

        var industry = kwargs.Get("industry_name");

        // IrQweb, _render are not available in C#
        // IrQweb = self.env['ir.qweb'].with_context(website_id=website.id, lang=website.default_lang_id.code)
        throw new NotImplementedException();

        var snippetsCache = new Dictionary<string, dynamic>();
        var translatedContent = new Dictionary<string, string>();

        // _compute_placeholder is not available in C#
        // def _compute_placeholder(term):
        //     return xml_translate.get_text_content(term).strip()
        throw new NotImplementedException();

        // _render_snippet is not available in C#
        // def _render_snippet(key):
        //     # Using this avoids rendering the same snippet multiple times
        //     data = snippets_cache.get(key)
        //     if data:
        //         return data

        //     render = IrQweb._render(key, cta_data)

        //     terms = []
        //     xml_translate(terms.append, render)
        //     placeholders = [_compute_placeholder(term) for term in terms]

        //     if text_must_be_translated_for_openai:
        //         # Check if terms are translated.
        //         translation_dictionary = self.env['website.page']._fields['arch_db'].get_translation_dictionary(
        //             str(IrQweb._render(key, cta_data, lang="en_US")),
        //             {text_generation_target_lang: str(render)},
        //         )
        //         # Remove all numeric keys.
        //         translation_dictionary = {k: v for k, v in translation_dictionary.items() if not _compute_placeholder(k).isnumeric()}
        //         for from_lang_term, to_lang_terms in translation_dictionary.items():
        //             translated_content[from_lang_term] = to_lang_terms[text_generation_target_lang]

        //     data = (render, placeholders)
        //     snippets_cache[key] = data
        //     return data
        throw new NotImplementedException();

        var textGenerationTargetLang = this.DefaultLangId.Code;
        // If the target language is not English, we need a good translation
        // coverage. But if the target lang is en_XX it's ok to have en_US text.
        var textMustBeTranslatedForOpenai = !textGenerationTargetLang.StartsWith("en_");
        var generatedContent = new Dictionary<string, string>();
        foreach (var pageCode in requestedPages.Except(new List<string> { "privacy_policy" }))
        {
            var snippetList = configuratorSnippets.GetOrDefault(pageCode, new List<string>());
            foreach (var snippet in snippetList)
            {
                // _render_snippet is not available in C#
                // render, placeholders = _render_snippet(f'website.configurator_{page_code}_{snippet}')
                throw new NotImplementedException();

                // Fill rendered block with AI text
                // render = xml_translate(
                //     lambda x: generated_content.get(_compute_placeholder(x), x),
                //     render
                // )
                throw new NotImplementedException();

                // el = html.fromstring(render)
                // Add the data-snippet attribute to identify the snippet
                // for compatibility code
                // el.attrib['data-snippet'] = snippet
                // Tweak the shape of the first snippet to connect it
                // properly with the header color in some themes
                // if i == 1:
                //     shape_el = el.xpath("//*[hasclass('o_we_shape')]")
                //     if shape_el:
                //         shape_el[0].attrib['class'] += ' o_header_extra_shape_mapping'
                // Tweak the shape of the last snippet to connect it
                // properly with the footer color in some themes
                // if i == nb_snippets:
                //     shape_el = el.xpath("//*[hasclass('o_we_shape')]")
                //     if shape_el:
                //         shape_el[0].attrib['class'] += ' o_footer_extra_shape_mapping'
                // rendered_snippet = pycompat.to_text(etree.tostring(el))
                // rendered_snippets.append(rendered_snippet)
                throw new NotImplementedException();
            }
        }
        if (textMustBeTranslatedForOpenai)
        {
            var nbTermsTranslated = translatedContent.Count(kvp => kvp.Key != kvp.Value);
            var nbTermsTotal = translatedContent.Count;
        }
        else
        {
            var nbTermsTranslated = generatedContent.Count;
            var nbTermsTotal = generatedContent.Count;
        }
        var translatedRatio = (double)nbTermsTranslated / nbTermsTotal;
        // logger.debug("Ratio of translated content: %s%% (%s/%s)", translated_ratio * 100, nb_terms_translated, nb_terms_total)

        if (translatedRatio > 0.8)
        {
            try
            {
                // _OLG_api_rpc is not available in C#
                // database_id = self.env['ir.config_parameter'].sudo().get_param('database.uuid')
                // response = self._OLG_api_rpc('/api/olg/1/generate_placeholder', {
                //     'placeholders': list(generated_content.keys()),
                //     'lang': website.default_lang_id.name,
                //     'industry': industry,
                //     'database_id': database_id,
                // })
                // name_replace_parser = re.compile(r"XXXX", re.MULTILINE)
                // for key in generated_content:
                //     if response.get(key):
                //         generated_content[key] = (name_replace_parser.sub(website.name, response[key], 0))
                throw new NotImplementedException();
            }
            catch (AccessError)
            {
                // If IAP is broken continue normally (without generating text)
                // pass
                throw new NotImplementedException();
            }
        }
        else
        {
            // logger.info("Skip AI text generation because translation coverage is too low (%s%%)", translated_ratio * 100)
            throw new NotImplementedException();
        }

        // Configure the pages
        foreach (var pageCode in requestedPages)
        {
            var snippetList = configuratorSnippets.GetOrDefault(pageCode, new List<string>());
            if (pageCode == "homepage")
            {
                // viewref is not available in C#
                // page_view_id = self.with_context(website_id=website.id).viewref('website.homepage')
                throw new NotImplementedException();
            }
            else
            {
                // browse is not available in C#
                // page_view_id = self.env['ir.ui.view'].browse(pages_views[page_code])
                throw new NotImplementedException();
            }
            var renderedSnippets = new List<string>();
            var nbSnippets = snippetList.Count;
            for (var i = 1; i <= nbSnippets; i++)
            {
                try
                {
                    // _render_snippet is not available in C#
                    // render, placeholders = _render_snippet(f'website.configurator_{page_code}_{snippet}')
                    throw new NotImplementedException();

                    // Fill rendered block with AI text
                    // render = xml_translate(
                    //     lambda x: generated_content.get(_compute_placeholder(x), x),
                    //     render
                    // )
                    throw new NotImplementedException();

                    // el = html.fromstring(render)
                    // Add the data-snippet attribute to identify the snippet
                    // for compatibility code
                    // el.attrib['data-snippet'] = snippet
                    // Tweak the shape of the first snippet to connect it
                    // properly with the header color in some themes
                    // if i == 1:
                    //     shape_el = el.xpath("//*[hasclass('o_we_shape')]")
                    //     if shape_el:
                    //         shape_el[0].attrib['class'] += ' o_header_extra_shape_mapping'
                    // Tweak the shape of the last snippet to connect it
                    // properly with the footer color in some themes
                    // if i == nb_snippets:
                    //     shape_el = el.xpath("//*[hasclass('o_we_shape')]")
                    //     if shape_el:
                    //         shape_el[0].attrib['class'] += ' o_footer_extra_shape_mapping'
                    // rendered_snippet = pycompat.to_text(etree.tostring(el))
                    // rendered_snippets.append(rendered_snippet)
                    throw new NotImplementedException();
                }
                catch (ValueError e)
                {
                    // logger.warning(e)
                    throw new NotImplementedException();
                }
            }
            // save, xpath are not available in C#
            // page_view_id.save(value=f'<div class="oe_structure">{"".join(rendered_snippets)}</div>',
            //               xpath="(//div[hasclass('oe_structure')])[last()]")
            throw new NotImplementedException();
        }

        // Configure the images
        var images = customResources.GetOrDefault("images", new Dictionary<string, string>());
        var names = Env.Get<IrModelData>().Search(r => r.Name.Contains("configurator\\_{0}\\_".Format(this.Id)) && r.Module == "website" && r.Model == "ir.attachment").Select(r => r.Name).ToList();
        foreach (var name in images.Keys)
        {
            var extnIdentifier = "configurator_{0}_{1}".Format(this.Id, name.Split('.')[1]);
            if (names.Contains(extnIdentifier))
            {
                continue;
            }
            try
            {
                // requests, raise_for_status are not available in C#
                // response = requests.get(image_src, timeout=3)
                // response.raise_for_status()
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                // logger.warning("Failed to download image: %s.\n%s", image_src, e)
                throw new NotImplementedException();
            }
            else
            {
                // create is not available in C#
                // attachment = self.env['ir.attachment'].create({
                //     'name': name,
                //     'website_id': website.id,
                //     'key': name,
                //     'type': 'binary',
                //     'raw': response.content,
                //     'public': True,
                // })
                // self.env['ir.model.