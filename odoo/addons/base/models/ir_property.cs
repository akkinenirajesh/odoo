C#
public partial class BaseIrProperty {
    public void Init()
    {
        // Ensure there is at most one active variant for each combination.
        Env.Cr.Execute("CREATE UNIQUE INDEX IF NOT EXISTS ir_property_unique_index ON {0} (fields_id, COALESCE(company_id, 0), COALESCE(res_id, ''))", _table);
    }

    public Dictionary<string, object> UpdateValues(Dictionary<string, object> values)
    {
        if (!values.ContainsKey("Value"))
        {
            return values;
        }

        var value = values["Value"];
        values.Remove("Value");

        var type = values.ContainsKey("Type") ? values["Type"].ToString() : this.Type;
        var field = TYPE2FIELD.Get(type);

        if (field == null)
        {
            throw new Exception("Invalid type");
        }

        if (field == "ValueReference")
        {
            if (value == null)
            {
                value = false;
            }
            else if (value is BaseModel)
            {
                value = string.Format("{0},{1}", ((BaseModel)value)._name, ((BaseModel)value).Id);
            }
            else if (value is int)
            {
                var fieldId = values.ContainsKey("FieldsId") ? values["FieldsId"] : this.FieldsId;
                var field = Env.IrModelFields.Browse(fieldId);
                value = string.Format("{0},{1}", field.Sudo().Relation, (int)value);
            }
        }

        values[field] = value;
        return values;
    }

    public void Write(Dictionary<string, object> values)
    {
        values = UpdateValues(values);
        var defaultSet = false;
        defaultSet = (values.ContainsKey("ResId") && values["ResId"] == false && this.ResId != false) ||
                     this.ResId == false && values.Any(x => this[x.Key] != this.Fields[x.Key].ConvertToRecord(x.Value, this));

        base.Write(values);
        if (defaultSet)
        {
            // DLE P44: test `test_27_company_dependent`
            // Easy solution, need to flush write when changing a property.
            // Maybe it would be better to be able to compute all impacted cache value and update those instead
            // Then clear_cache must be removed as well.
            Env.FlushAll();
            Env.Registry.ClearCache();
        }
    }

    public BaseIrProperty Create(Dictionary<string, object> vals)
    {
        vals = UpdateValues(vals);
        var createdDefault = vals.ContainsKey("ResId") && vals["ResId"] == false;
        var result = base.Create(vals);
        if (createdDefault)
        {
            // DLE P44: test `test_27_company_dependent`
            Env.FlushAll();
            Env.Registry.ClearCache();
        }
        return result;
    }

    public void Unlink()
    {
        var defaultDeleted = this.ResId == false;
        base.Unlink();
        if (defaultDeleted)
        {
            Env.Registry.ClearCache();
        }
    }

    public object GetByRecord()
    {
        if (this.Type == "Char" || this.Type == "Text" || this.Type == "Selection")
        {
            return this.ValueText;
        }
        else if (this.Type == "Float")
        {
            return this.ValueFloat;
        }
        else if (this.Type == "Boolean")
        {
            return Convert.ToBoolean(this.ValueInteger);
        }
        else if (this.Type == "Integer")
        {
            return this.ValueInteger;
        }
        else if (this.Type == "Binary")
        {
            return this.ValueBinary;
        }
        else if (this.Type == "Many2One")
        {
            if (string.IsNullOrEmpty(this.ValueReference))
            {
                return false;
            }

            var parts = this.ValueReference.Split(',');
            var model = parts[0];
            var resourceId = int.Parse(parts[1]);
            return Env[model].Browse(resourceId).Exists();
        }
        else if (this.Type == "DateTime")
        {
            return this.ValueDatetime;
        }
        else if (this.Type == "Date")
        {
            if (this.ValueDatetime == null)
            {
                return false;
            }

            return DateTime.Parse(this.ValueDatetime.ToString()).Date;
        }

        return false;
    }

    public void SetDefault(string name, string model, object value, ResCompany company = null)
    {
        var fieldId = Env.IrModelFields.Get(model, name).Id;
        var companyId = company != null ? company.Id : 0;
        var prop = this.Sudo().Search([
            ("FieldsId", "=", fieldId),
            ("CompanyId", "=", companyId),
            ("ResId", "=", false)
        ]);

        if (prop != null)
        {
            prop.Write(new Dictionary<string, object> { { "Value", value } });
        }
        else
        {
            prop.Create(new Dictionary<string, object> {
                { "FieldsId", fieldId },
                { "CompanyId", companyId },
                { "ResId", false },
                { "Name", name },
                { "Value", value },
                { "Type", Env[model].Fields[name].Type }
            });
        }
    }

    public object Get(string name, string model, string resId = null)
    {
        if (string.IsNullOrEmpty(resId))
        {
            var t, v = GetDefaultProperty(name, model);
            if (v == null || t != "Many2One")
            {
                return v;
            }

            return Env[v[0]].Browse(v[1]);
        }

        var p = GetProperty(name, model, resId);
        if (p != null)
        {
            return p.GetByRecord();
        }

        return false;
    }

    public Tuple<string, object> GetDefaultProperty(string name, string model)
    {
        var prop = GetProperty(name, model, resId: false);
        if (prop == null)
        {
            return null;
        }

        var v = prop.GetByRecord();
        if (prop.Type != "Many2One")
        {
            return new Tuple<string, object>(prop.Type, v);
        }

        return new Tuple<string, object>("Many2One", v != null ? new Tuple<string, object>(v._name, v.Id) : null);
    }

    public BaseIrProperty GetProperty(string name, string model, string resId)
    {
        var domain = GetDomain(name, model);
        if (domain != null)
        {
            if (resId != null && int.TryParse(resId, out var intResId))
            {
                resId = string.Format("{0},{1}", model, intResId);
            }

            domain = new List<Tuple<string, object>> { new Tuple<string, object>("ResId", resId) }.Concat(domain).ToList();
            return this.Sudo().Search(domain, limit: 1, order: "CompanyId NULLS FIRST");
        }

        return null;
    }

    public List<Tuple<string, object>> GetDomain(string propName, string model)
    {
        var fieldId = Env.IrModelFields.Get(model, propName).Id;
        if (fieldId == 0)
        {
            return null;
        }

        var companyId = Env.Company.Id;
        return new List<Tuple<string, object>> { new Tuple<string, object>("FieldsId", fieldId), new Tuple<string, object>("CompanyId", "in", new List<object> { companyId, false }) };
    }

    public Dictionary<int, object> GetMulti(string name, string model, List<int> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return new Dictionary<int, object>();
        }

        var field = Env[model].Fields[name];
        var fieldId = Env.IrModelFields.Get(model, name).Id;
        var companyId = Env.Company != null ? Env.Company.Id : 0;

        if (field.Type == "Many2One")
        {
            var comodel = Env[field.CommodelName];
            var modelPos = model.Length + 2;
            var valuePos = comodel._name.Length + 2;

            var query = string.Format(
                "SELECT substr(p.res_id, {0})::integer, r.id FROM ir_property p LEFT JOIN {1} r ON substr(p.value_reference, {2})::integer=r.id WHERE p.fields_id={3} AND (p.company_id={4} OR p.company_id IS NULL) AND (p.res_id IN {5} OR p.res_id IS NULL) ORDER BY p.company_id NULLS FIRST",
                modelPos, comodel._table, valuePos, fieldId, companyId, string.Join(",", ids.Select(x => string.Format("{0},{1}", model, x))));

            var result = new Dictionary<int, object>();
            var refs = ids.Select(x => string.Format("{0},{1}", model, x)).ToList();
            foreach (var subRefs in Env.Cr.SplitForInConditions(refs))
            {
                Env.Cr.Execute(query, new List<object> { subRefs });
                result.Update(Env.Cr.FetchAll());
            }

            return result.ToDictionary(x => x.Key, x => Env[comodel._name].Browse(x.Value));
        }
        else if (TYPE2FIELD.ContainsKey(field.Type))
        {
            var modelPos = model.Length + 2;
            var query = string.Format(
                "SELECT substr(p.res_id, {0})::integer, p.{1} FROM ir_property p WHERE p.fields_id={2} AND (p.company_id={3} OR p.company_id IS NULL) AND (p.res_id IN {4} OR p.res_id IS NULL) ORDER BY p.company_id NULLS FIRST",
                modelPos, TYPE2FIELD.Get(field.Type), fieldId, companyId, string.Join(",", ids.Select(x => string.Format("{0},{1}", model, x))));

            var result = new Dictionary<int, object>();
            var refs = ids.Select(x => string.Format("{0},{1}", model, x)).ToList();
            foreach (var subRefs in Env.Cr.SplitForInConditions(refs))
            {
                Env.Cr.Execute(query, new List<object> { subRefs });
                result.Update(Env.Cr.FetchAll());
            }

            return result.ToDictionary(x => x.Key, x => TYPE2CLEAN.Get(field.Type)(x.Value));
        }

        return ids.ToDictionary(x => x, x => false);
    }

    public void SetMulti(string name, string model, Dictionary<int, object> values, object defaultValue = null)
    {
        if (values == null || values.Count == 0)
        {
            return;
        }

        if (defaultValue == null)
        {
            var domain = GetDomain(name, model);
            if (domain == null)
            {
                throw new Exception();
            }

            defaultValue = Get(name, model);
        }

        var fieldId = Env.IrModelFields.Get(model, name).Id;
        var companyId = Env.Company.Id;
        var refs = values.ToDictionary(x => string.Format("{0},{1}", model, x.Key), x => x.Key);
        var props = this.Sudo().Search([
            ("FieldsId", "=", fieldId),
            ("CompanyId", "=", companyId),
            ("ResId", "in", refs.Keys.ToList())
        ]);

        foreach (var prop in props)
        {
            var id = refs[prop.ResId];
            var value = values[id];
            if (value.Equals(defaultValue))
            {
                Env.Cr.Execute("DELETE FROM ir_property WHERE id={0}", prop.Id);
            }
            else if (!value.Equals(prop.GetByRecord()))
            {
                prop.Write(new Dictionary<string, object> { { "Value", value } });
            }
        }

        var valsList = new List<Dictionary<string, object>>();
        foreach (var refId in refs.Keys)
        {
            var id = refs[refId];
            var value = values[id];
            if (!value.Equals(defaultValue))
            {
                valsList.Add(new Dictionary<string, object> {
                    { "FieldsId", fieldId },
                    { "CompanyId", companyId },
                    { "ResId", refId },
                    { "Name", name },
                    { "Value", value },
                    { "Type", Env[model].Fields[name].Type }
                });
            }
        }

        this.Sudo().Create(valsList);
    }

    public List<Tuple<string, object>> SearchMulti(string name, string model, string operator, object value)
    {
        var defaultMatches = false;
        var negate = false;

        if (operator == "in" && value is List<object> && ((List<object>)value).Contains(false))
        {
            operator = "not in";
            negate = true;
        }
        else if (operator == "not in" && value is List<object> && !((List<object>)value).Contains(false))
        {
            operator = "in";
            negate = true;
        }
        else if (operator == "!=" && value != null)
        {
            operator = TERM_OPERATORS_NEGATION.Get(operator);
            negate = true;
        }
        else if (operator == "=" && value == null)
        {
            operator = "!=";
            negate = true;
        }

        var field = Env[model].Fields[name];

        if (field.Type == "Many2One")
        {
            if (value is BaseModel)
            {
                value = string.Format("{0},{1}", field.CommodelName, ((BaseModel)value).Id);
            }
            else if (value is int)
            {
                value = string.Format("{0},{1}", field.CommodelName, (int)value);
            }
            else if (value is List<object>)
            {
                value = ((List<object>)value).Select(x => x is BaseModel ? string.Format("{0},{1}", field.CommodelName, ((BaseModel)x).Id) : string.Format("{0},{1}", field.CommodelName, (int)x)).ToList();
            }

            if (operator == "=" || operator == "!=" || operator == "<=" || operator == "<" || operator == ">" || operator == ">=")
            {
                // No action required
            }
            else if (operator == "in" || operator == "not in")
            {
                // No action required
            }
            else if (operator == "=like" || operator == "=ilike" || operator == "like" || operator == "not like" || operator == "ilike" || operator == "not ilike")
            {
                var target = Env[field.CommodelName];
                var targetNames = target.NameSearch(value, operator: operator, limit: null);
                var targetIds = targetNames.Select(x => x.Id).ToList();
                operator = "in";
                value = targetIds.Select(x => string.Format("{0},{1}", field.CommodelName, x)).ToList();
            }
            else if (operator == "any" || operator == "not any")
            {
                if (operator == "not any")
                {
                    negate = true;
                }

                operator = "in";
                value = Env[field.CommodelName]._Search(value).Select(x => string.Format("{0},{1}", field.CommodelName, x.Id)).ToList();
            }
        }
        else if (field.Type == "Integer" || field.Type == "Float")
        {
            if (value is double)
            {
                value = (double)value;
            }
            else if (value is int)
            {
                value = (int)value;
            }

            if (operator == ">=" && Convert.ToDouble(value) <= 0)
            {
                operator = "<";
                negate = true;
            }
            else if (operator == ">" && Convert.ToDouble(value) < 0)
            {
                operator = "<=";
                negate = true;
            }
            else if (operator == "<=" && Convert.ToDouble(value) >= 0)
            {
                operator = ">";
                negate = true;
            }
            else if (operator == "<" && Convert.ToDouble(value) > 0)
            {
                operator = ">=";
                negate = true;
            }
        }
        else if (field.Type == "Boolean")
        {
            value = Convert.ToInt32(value);
        }

        var domain = GetDomain(name, model);
        if (domain == null)
        {
            throw new Exception();
        }

        var props = this.Search(domain.Concat(new List<Tuple<string, object>> { new Tuple<string, object>(TYPE2FIELD.Get(field.Type), operator, value) }).ToList());

        var goodIds = new List<int>();
        foreach (var prop in props)
        {
            if (!string.IsNullOrEmpty(prop.ResId))
            {
                var parts = prop.ResId.Split(',');
                goodIds.Add(int.Parse(parts[1]));
            }
            else
            {
                defaultMatches = true;
            }
        }

        if (defaultMatches)
        {
            props = this.Search(domain.Concat(new List<Tuple<string, object>> { new Tuple<string, object>("ResId", "!=", false) }).ToList());
            var allIds = props.Select(x => int.Parse(x.ResId.Split(',')[1])).ToList();
            var badIds = allIds.Except(goodIds).ToList();
            return negate ? new List<Tuple<string, object>> { new Tuple<string, object>("Id", "in", badIds) } : new List<Tuple<string, object>> { new Tuple<string, object>("Id", "not in", badIds) };
        }
        else
        {
            return negate ? new List<Tuple<string, object>> { new Tuple<string, object>("Id", "not in", goodIds) } : new List<Tuple<string, object>> { new Tuple<string, object>("Id", "in", goodIds) };
        }
    }
}
