csharp
public partial class WebsiteFormConfig {
    public WebsiteFormConfig() {
    }

    public WebsiteFormConfig(int id) {
        this.Id = id;
    }

    public int Id { get; set; }

    public WebsiteFormConfig _WebsiteFormLastRecord() {
        if (Env.Context.ContainsKey("FormBuilderModelModel") && Env.Context.ContainsKey("FormBuilderId")) {
            return Env.GetModel(Env.Context["FormBuilderModelModel"].ToString()).GetById(Convert.ToInt32(Env.Context["FormBuilderId"]));
        }
        return null;
    }
}

public partial class IrModel {
    public IrModel() {
    }

    public IrModel(int id) {
        this.Id = id;
    }

    public int Id { get; set; }

    public bool WebsiteFormAccess { get; set; }

    public IrModelFields WebsiteFormDefaultFieldId { get; set; }

    public string WebsiteFormLabel { get; set; }

    public string WebsiteformKey { get; set; }

    public Dictionary<string, object> GetFormWritableFields(Dictionary<string, object> propertyOrigins) {
        Dictionary<string, object> included = new Dictionary<string, object>();
        if (this.Model == "mail.mail") {
            included.Add("EmailFrom", null);
            included.Add("EmailTo", null);
            included.Add("EmailCc", null);
            included.Add("EmailBcc", null);
            included.Add("Body", null);
            included.Add("ReplyTo", null);
            included.Add("Subject", null);
        } else {
            List<IrModelFields> fields = Env.GetModel("Ir.ModelFields").Search(new Dictionary<string, object>() {
                { "ModelId", this.Id },
                { "WebsiteFormBlacklisted", false }
            });
            foreach (IrModelFields field in fields) {
                included.Add(field.Name, null);
            }
        }

        return GetAuthorizedFields(this.Model, propertyOrigins).Where(x => included.ContainsKey(x.Key) || (x.Value is Dictionary<string, object> && ((Dictionary<string, object>)x.Value).ContainsKey("_property") && included.ContainsKey(((Dictionary<string, object>)x.Value)["_property"]["field"].ToString()))).ToDictionary(x => x.Key, x => x.Value);
    }

    public Dictionary<string, object> GetAuthorizedFields(string modelName, Dictionary<string, object> propertyOrigins) {
        var model = Env.GetModel(modelName);
        var fieldsGet = model.FieldsGet();

        foreach (var key in model.Inherits.Keys) {
            fieldsGet.Remove(key, out _);
        }

        var defaultValues = model.WithUser(Env.SuperUserId).DefaultGet(fieldsGet.Keys.ToList());
        foreach (var field in fieldsGet.Keys.Where(x => defaultValues.ContainsKey(x)).ToList()) {
            fieldsGet[field]["required"] = false;
        }

        foreach (var field in fieldsGet.Keys.ToList()) {
            if (fieldsGet[field].ContainsKey("domain") && fieldsGet[field]["domain"] is string) {
                fieldsGet[field].Remove("domain");
            }

            if (fieldsGet[field].ContainsKey("readonly") && (bool)fieldsGet[field]["readonly"] || field == "create_uid" || field == "create_date" || field == "write_uid" || field == "write_date" || fieldsGet[field]["type"].ToString() == "many2one_reference" || fieldsGet[field]["type"].ToString() == "json") {
                fieldsGet.Remove(field, out _);
            } else if (fieldsGet[field]["type"].ToString() == "properties") {
                var propertyField = fieldsGet[field];
                fieldsGet.Remove(field, out _);
                if (propertyOrigins != null) {
                    var definitionRecord = propertyField["definition_record"].ToString();
                    if (propertyOrigins.ContainsKey(definitionRecord)) {
                        var definitionRecordField = propertyField["definition_record_field"].ToString();
                        var relationField = fieldsGet[definitionRecord];
                        var definitionModel = Env.GetModel(relationField["relation"].ToString());
                        if (!propertyOrigins[definitionRecord].ToString().Contains(".")) {
                            continue;
                        }
                        var definitionRecordObj = definitionModel.GetById(Convert.ToInt32(propertyOrigins[definitionRecord].ToString().Substring(propertyOrigins[definitionRecord].ToString().LastIndexOf(".") + 1)));
                        var propertiesDefinitions = definitionRecordObj[definitionRecordField];
                        foreach (var propertyDefinition in propertiesDefinitions) {
                            if ((propertyDefinition["type"].ToString() == "many2one" || propertyDefinition["type"].ToString() == "many2many") && !propertyDefinition.ContainsKey("comodel") || propertyDefinition["type"].ToString() == "selection" && !propertyDefinition.ContainsKey("selection") || propertyDefinition["type"].ToString() == "tags" && !propertyDefinition.ContainsKey("tags") || propertyDefinition["type"].ToString() == "separator") {
                                continue;
                            }
                            var propertyDefinitionDict = (Dictionary<string, object>)propertyDefinition;
                            propertyDefinitionDict.Add("_property", new Dictionary<string, object>() {
                                { "field", field }
                            });
                            propertyDefinitionDict["required"] = false;
                            if (propertyDefinitionDict.ContainsKey("domain") && propertyDefinitionDict["domain"] is string) {
                                propertyDefinitionDict["domain"] = literal_eval(propertyDefinitionDict["domain"].ToString());
                                try {
                                    propertyDefinitionDict["domain"] = expression.normalize_domain(propertyDefinitionDict["domain"]);
                                } catch (Exception) {
                                    continue;
                                }
                            }
                            fieldsGet.Add(propertyDefinition.get("name").ToString(), propertyDefinitionDict);
                        }
                    }
                }
            }
        }
        return fieldsGet;
    }

    public List<Dictionary<string, object>> GetCompatibleFormModels() {
        if (!Env.User.HasGroup("website.group_website_restricted_editor")) {
            return new List<Dictionary<string, object>>();
        }
        return this.SearchRead(new Dictionary<string, object>() {
            { "WebsiteFormAccess", true }
        }, new List<string>() {
            "Id",
            "Model",
            "Name",
            "WebsiteFormLabel",
            "WebsiteformKey"
        });
    }
}

public partial class IrModelFields {
    public IrModelFields() {
    }

    public IrModelFields(int id) {
        this.Id = id;
    }

    public int Id { get; set; }

    public bool WebsiteFormBlacklisted { get; set; }

    public bool FormbuilderWhitelist(string model, List<string> fields) {
        if (fields.Count == 0) {
            return false;
        }

        if (!Env.User.HasGroup("website.group_website_designer")) {
            return false;
        }

        List<string> unexistingFields = fields.Where(x => !Env.GetModel(model)._Fields.ContainsKey(x)).ToList();
        if (unexistingFields.Count > 0) {
            throw new Exception($"Unable to whitelist field(s) {string.Join(",", unexistingFields)} for model {model}.");
        }

        Env.Cr.Execute(
            "UPDATE ir_model_fields"
            + " SET website_form_blacklisted=false"
            + " WHERE model=%s AND name in %s", (model, tuple(fields)));
        return true;
    }
}
