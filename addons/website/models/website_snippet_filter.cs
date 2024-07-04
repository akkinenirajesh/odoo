csharp
public partial class WebsiteSnippetFilter {
    public void ComputeModelName() {
        if (this.FilterId != null) {
            this.ModelName = Env.GetModel("Ir.Filters").Get(this.FilterId.ToString()).ModelId.ToString();
        } else {
            this.ModelName = Env.GetModel("Ir.ActionsServer").Get(this.ActionServerId.ToString()).ModelId.ToString();
        }
    }

    public void CheckDataSourceIsProvided() {
        if (this.ActionServerId != null && this.FilterId != null) {
            throw new Exception("Either ActionServerId or FilterId must be provided.");
        }
    }

    public void CheckLimit() {
        if (this.Limit < 1 || this.Limit > 16) {
            throw new Exception("The limit must be between 1 and 16.");
        }
    }

    public void CheckFieldNames() {
        foreach (var fieldName in this.FieldNames.Split(',')) {
            if (string.IsNullOrEmpty(fieldName.Trim())) {
                throw new Exception($"Empty field name in “{this.FieldNames}”");
            }
        }
    }

    public List<string> Render(string templateKey, int limit, List<object> searchDomain, bool withSample) {
        if (!templateKey.Contains(".dynamic_filter_template_")) {
            throw new Exception("You can only use template prefixed by dynamic_filter_template_ ");
        }

        if (this.WebsiteId != null && Env.GetModel("Website").GetCurrentWebsite() != this.WebsiteId) {
            return new List<string>();
        }

        if (!templateKey.Contains(this.ModelName.Replace(".", "_"))) {
            return new List<string>();
        }

        var records = PrepareValues(limit, searchDomain);
        bool isSample = withSample && records.Count == 0;
        if (isSample) {
            records = PrepareSample(limit);
        }

        var content = Env.GetModel("Ir.Qweb").WithContext(new { inheritBranding = false }).Render(templateKey, new { records = records, isSample = isSample });
        return content.Select(el => el.ToString()).ToList();
    }

    public List<object> PrepareValues(int limit, List<object> searchDomain) {
        int maxLimit = Math.Max(this.Limit, 16);
        limit = limit != 0 ? Math.Min(limit, maxLimit) : maxLimit;

        if (this.FilterId != null) {
            var filterSudo = Env.GetModel("Ir.Filters").Get(this.FilterId.ToString()).Sudo();
            var domain = filterSudo.GetEvalDomain();

            if (Env.GetModel(filterSudo.ModelId.ToString()).HasField("WebsiteId")) {
                domain = Expression.AND(domain, Env.GetModel("Website").GetCurrentWebsite().WebsiteDomain);
            }

            if (Env.GetModel(filterSudo.ModelId.ToString()).HasField("CompanyId")) {
                var website = Env.GetModel("Website").GetCurrentWebsite();
                domain = Expression.AND(domain, new List<object> { "CompanyId", "in", new List<object> { null, website.CompanyId } });
            }

            if (Env.GetModel(filterSudo.ModelId.ToString()).HasField("IsPublished")) {
                domain = Expression.AND(domain, new List<object> { "IsPublished", "=", true });
            }

            if (searchDomain != null) {
                domain = Expression.AND(domain, searchDomain);
            }

            try {
                var records = Env.GetModel(filterSudo.ModelId.ToString()).WithContext(filterSudo.Context).Search(domain, filterSudo.Sort, limit);
                return FilterRecordsToValues(records);
            } catch (MissingError) {
                _logger.Warning($"The provided domain {domain} in 'ir.filters' generated a MissingError in '{this.Name}'");
                return new List<object>();
            }
        } else if (this.ActionServerId != null) {
            try {
                return Env.GetModel("Ir.ActionsServer").Get(this.ActionServerId.ToString()).WithContext(new { dynamicFilter = this, limit = limit, searchDomain = searchDomain }).Sudo().Run();
            } catch (MissingError) {
                _logger.Warning($"The provided domain {searchDomain} in 'ir.actions.server' generated a MissingError in '{this.Name}'");
                return new List<object>();
            }
        }

        return new List<object>();
    }

    public Tuple<string, string> GetFieldNameAndType(string model, string fieldName) {
        var parts = fieldName.Split(':');
        string fieldWidget = parts.Length > 1 ? parts[1] : null;
        string fieldType = "";

        if (fieldWidget == null) {
            var field = Env.GetModel(model).GetField(parts[0]);
            if (field != null) {
                fieldType = field.Type;
            } else if (fieldName.Contains("image")) {
                fieldType = "image";
            } else if (fieldName.Contains("price")) {
                fieldType = "monetary";
            } else {
                fieldType = "text";
            }
        }

        return new Tuple<string, string>(parts[0], fieldWidget ?? fieldType);
    }

    public Dictionary<string, string> GetFilterMetaData() {
        var model = Env.GetModel(this.ModelName);
        var metaData = new Dictionary<string, string>();
        foreach (var fieldName in this.FieldNames.Split(',')) {
            var fieldInfo = GetFieldNameAndType(this.ModelName, fieldName);
            metaData.Add(fieldInfo.Item1, fieldInfo.Item2);
        }

        return metaData;
    }

    public List<object> PrepareSample(int length) {
        if (length == 0) {
            return new List<object>();
        }

        var records = PrepareSampleRecords(length);
        return FilterRecordsToValues(records, true);
    }

    public List<object> PrepareSampleRecords(int length) {
        if (length == 0) {
            return new List<object>();
        }

        var sample = new List<object>();
        var model = Env.GetModel(this.ModelName);
        var sampleData = GetHardcodedSample(model);
        if (sampleData != null) {
            for (int index = 0; index < length; index++) {
                var singleSampleData = sampleData[index % sampleData.Count].Clone();
                FillSample(singleSampleData, index);
                sample.Add(model.New(singleSampleData));
            }
        }

        return sample;
    }

    public void FillSample(object sample, int index) {
        var metaData = GetFilterMetaData();
        var model = Env.GetModel(this.ModelName);
        foreach (var kvp in metaData) {
            var fieldName = kvp.Key;
            var fieldWidget = kvp.Value;
            if (!sample.ContainsField(fieldName) && model.HasField(fieldName)) {
                if (fieldWidget == "image" || fieldWidget == "binary") {
                    sample.SetField(fieldName, null);
                } else if (fieldWidget == "monetary") {
                    sample.SetField(fieldName, (decimal)new Random().Next(100, 10000) / 10);
                } else if (fieldWidget == "integer" || fieldWidget == "float") {
                    sample.SetField(fieldName, index);
                } else {
                    sample.SetField(fieldName, $"Sample {index + 1}");
                }
            }
        }
    }

    public List<object> GetHardcodedSample(object model) {
        return new List<object> { new Dictionary<string, object>() };
    }

    public List<object> FilterRecordsToValues(List<object> records, bool isSample = false) {
        var metaData = GetFilterMetaData();
        var model = Env.GetModel(this.ModelName);
        var website = Env.GetModel("Website").GetCurrentWebsite();

        var values = new List<object>();
        foreach (var record in records) {
            var data = new Dictionary<string, object>();
            foreach (var kvp in metaData) {
                var fieldName = kvp.Key;
                var fieldWidget = kvp.Value;
                var field = model.GetField(fieldName);
                if (field != null && (field.Type == "binary" || field.Type == "image")) {
                    if (isSample) {
                        data.Add(fieldName, record.GetField(fieldName).ToString());
                    } else {
                        data.Add(fieldName, website.ImageUrl(record, fieldName));
                    }
                } else if (fieldWidget == "monetary") {
                    object modelCurrency = null;
                    if (field != null && field.Type == "monetary") {
                        modelCurrency = record.GetField(field.GetCurrencyField(record));
                    } else if (model.HasField("CurrencyId")) {
                        modelCurrency = record.GetField("CurrencyId");
                    }

                    if (modelCurrency != null) {
                        var websiteCurrency = GetWebsiteCurrency();
                        data.Add(fieldName, modelCurrency.Convert(record.GetField(fieldName), websiteCurrency, website.CompanyId, DateTime.Now));
                    } else {
                        data.Add(fieldName, record.GetField(fieldName));
                    }
                } else {
                    data.Add(fieldName, record.GetField(fieldName));
                }
            }

            data.Add("CallToActionUrl", record.GetField("website_url"));
            data.Add("_Record", record);
            values.Add(data);
        }

        return values;
    }

    public object GetWebsiteCurrency() {
        var company = Env.GetModel("Website").GetCurrentWebsite().CompanyId;
        return Env.GetModel("Res.Currency").Get(company.CurrencyId.ToString());
    }
}
