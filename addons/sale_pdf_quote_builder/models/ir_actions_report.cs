csharp
public partial class IrActionsReport 
{
    public byte[] RenderQwebPdfPrepareStreams(string reportRef, object data, int[] resIds)
    {
        var result = Env.Call("ir.actions.report", "_render_qweb_pdf_prepare_streams", reportRef, data, resIds);
        if (GetReport(reportRef).ReportName != "sale.report_saleorder")
        {
            return (byte[])result;
        }

        var orders = Env.Call("sale.order", "Browse", resIds);

        foreach (var order in orders)
        {
            var initialStream = (byte[])result[order.Id]["stream"];
            if (initialStream != null)
            {
                var orderTemplate = (SaleOrderTemplate)order.Get("sale_order_template_id");
                var headerRecord = orderTemplate != null && orderTemplate.Get("sale_header") != null ? orderTemplate : (SaleCompany)order.Get("company_id");
                var footerRecord = orderTemplate != null && orderTemplate.Get("sale_footer") != null ? orderTemplate : (SaleCompany)order.Get("company_id");
                var hasHeader = headerRecord != null && headerRecord.Get("sale_header") != null;
                var hasFooter = footerRecord != null && footerRecord.Get("sale_footer") != null;
                var includedProductDocs = Env.Call("product.document", "Browse", new int[0]);
                var docLineIdMapping = new Dictionary<int, int>();
                foreach (var line in order.GetCollection("order_line"))
                {
                    var productProductDocs = (ProductDocument[])line.Get("product_id").GetCollection("product_document_ids");
                    var productTemplateDocs = (ProductDocument[])line.Get("product_template_id").GetCollection("product_document_ids");
                    var docToInclude = productProductDocs.Where(d => d.Get("attached_on_sale") == "inside").ToArray() ?? productTemplateDocs.Where(d => d.Get("attached_on_sale") == "inside").ToArray();
                    includedProductDocs = (ProductDocument[])includedProductDocs.Concat(docToInclude);
                    foreach (var doc in docToInclude)
                    {
                        docLineIdMapping[doc.Id] = line.Id;
                    }
                }

                if (!hasHeader && includedProductDocs.Count() == 0 && !hasFooter)
                {
                    continue;
                }

                var writer = new PdfFileWriter();
                if (hasHeader)
                {
                    AddPagesToWriter(writer, (byte[])headerRecord.Get("sale_header"), null);
                }
                if (includedProductDocs.Count() > 0)
                {
                    foreach (var doc in includedProductDocs)
                    {
                        AddPagesToWriter(writer, (byte[])doc.Get("datas"), docLineIdMapping[doc.Id]);
                    }
                }
                AddPagesToWriter(writer, initialStream, null);
                if (hasFooter)
                {
                    AddPagesToWriter(writer, (byte[])footerRecord.Get("sale_footer"), null);
                }

                var formFields = GetFormFieldsMapping(order, docLineIdMapping);
                Pdf.FillFormFieldsPdf(writer, formFields);
                using (var buffer = new MemoryStream())
                {
                    writer.Write(buffer);
                    stream = new MemoryStream(buffer.ToArray());
                }
                result[order.Id]["stream"] = stream;
            }
        }

        return (byte[])result;
    }

    private IrActionsReport GetReport(string reportRef)
    {
        return (IrActionsReport)Env.Call("ir.actions.report", "Search", new Dictionary<string, object>() { { "report_name", reportRef } });
    }

    private void AddPagesToWriter(PdfFileWriter writer, byte[] document, int? solId)
    {
        var prefix = solId != null ? $"{solId}_" : "";
        using (var reader = new PdfFileReader(new MemoryStream(document), strict: false))
        {
            var solFieldNames = GetSolFormFieldsNames();
            for (var pageId = 0; pageId < reader.NumPages; pageId++)
            {
                var page = reader.GetPage(pageId);
                if (solId != null && page.Get("/Annots") != null)
                {
                    for (var j = 0; j < page.Get("/Annots").Count; j++)
                    {
                        var readerAnnot = (PdfDictionary)page.Get("/Annots")[j].GetObject();
                        if (solFieldNames.Contains(readerAnnot.Get("/T")))
                        {
                            readerAnnot.Set("/T", createStringObject($"{prefix}{readerAnnot.Get("/T")}"));
                        }
                    }
                }
                writer.AddPage(page);
            }
        }
    }

    private string[] GetSolFormFieldsNames()
    {
        return new string[] { "description", "quantity", "uom", "price_unit", "discount", "product_sale_price", "taxes", "tax_excl_price", "tax_incl_price" };
    }

    private Dictionary<string, object> GetFormFieldsMapping(SaleOrder order, Dictionary<int, int> docLineIdMapping)
    {
        var env = Env.WithContext(use_babel: true);
        var tz = order.Get("partner_id").Get("tz") ?? Env.User.Get("tz") ?? "UTC";
        var langCode = order.Get("partner_id").Get("lang") ?? Env.User.Get("lang");
        var formFieldsMapping = new Dictionary<string, object>()
        {
            { "name", order.Get("name") },
            { "partner_id__name", order.Get("partner_id").Get("name") },
            { "user_id__name", order.Get("user_id").Get("name") },
            { "amount_untaxed", FormatAmount(env, (decimal)order.Get("amount_untaxed"), (SaleCurrency)order.Get("currency_id")) },
            { "amount_total", FormatAmount(env, (decimal)order.Get("amount_total"), (SaleCurrency)order.Get("currency_id")) },
            { "delivery_date", FormatDatetime(env, (DateTime)order.Get("commitment_date"), tz) },
            { "validity_date", FormatDate(env, (DateTime)order.Get("validity_date"), langCode) },
            { "client_order_ref", order.Get("client_order_ref") ?? "" },
        };

        var linesWithDocIds = docLineIdMapping.Values;
        foreach (var line in order.GetCollection("order_line").Where(sol => linesWithDocIds.Contains(sol.Id)))
        {
            formFieldsMapping.AddOrUpdate(GetSolFormFieldsMapping(line));
        }

        return formFieldsMapping;
    }

    private Dictionary<string, object> GetSolFormFieldsMapping(SaleOrderLine line)
    {
        var env = Env.WithContext(use_babel: true);
        return new Dictionary<string, object>()
        {
            { $"{line.Id}_description", line.Get("name") },
            { $"{line.Id}_quantity", line.Get("product_uom_qty") },
            { $"{line.Id}_uom", line.Get("product_uom").Get("name") },
            { $"{line.Id}_price_unit", FormatAmount(env, (decimal)line.Get("price_unit"), (SaleCurrency)line.Get("currency_id")) },
            { $"{line.Id}_discount", line.Get("discount") },
            { $"{line.Id}_product_sale_price", FormatAmount(env, (decimal)line.Get("product_id").Get("lst_price"), (SaleCurrency)line.Get("product_id").Get("currency_id")) },
            { $"{line.Id}_taxes", string.Join(", ", line.GetCollection("tax_id").Select(t => t.Get("name"))) },
            { $"{line.Id}_tax_excl_price", FormatAmount(env, (decimal)line.Get("price_subtotal"), (SaleCurrency)line.Get("currency_id")) },
            { $"{line.Id}_tax_incl_price", FormatAmount(env, (decimal)line.Get("price_total"), (SaleCurrency)line.Get("currency_id")) },
        };
    }
}
