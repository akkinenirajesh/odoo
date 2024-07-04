C#
public partial class WebIrModel
{
    public virtual List<WebIrModel> DisplayNameFor(List<string> models)
    {
        List<WebIrModel> accessibleModels = new List<WebIrModel>();
        List<WebIrModel> notAccessibleModels = new List<WebIrModel>();
        foreach (string model in models)
        {
            if (IsValidForModelSelector(model))
            {
                accessibleModels.Add(Env.GetModel(model));
            }
            else
            {
                notAccessibleModels.Add(new WebIrModel { Name = model, Model = model });
            }
        }
        return DisplayNameFor(accessibleModels).Concat(notAccessibleModels).ToList();
    }

    public virtual List<WebIrModel> DisplayNameFor(List<WebIrModel> models)
    {
        List<WebIrModel> records = Env.SearchRead<WebIrModel>(new List<object[]> { new object[] { "Model", "in", models.Select(m => m.Model).ToList() } }, new List<string> { "Name", "Model" });
        return records.Select(model => new WebIrModel
        {
            Name = model.Name,
            Model = model.Model
        }).ToList();
    }

    public virtual bool IsValidForModelSelector(string model)
    {
        WebIrModel modelInstance = Env.GetModel(model);
        return Env.User.IsInternal()
            && modelInstance != null
            && modelInstance.CheckAccessRights("read", raiseException: false)
            && !modelInstance.IsTransient
            && !modelInstance.IsAbstract;
    }

    public virtual List<WebIrModel> GetAvailableModels()
    {
        List<string> accessibleModels = Env.Pool.Models.Where(model => IsValidForModelSelector(model)).ToList();
        return DisplayNameFor(accessibleModels);
    }

    public virtual Dictionary<string, object> GetDefinitions(List<string> modelNames)
    {
        Dictionary<string, object> modelDefinitions = new Dictionary<string, object>();
        foreach (string modelName in modelNames)
        {
            WebIrModel model = Env.GetModel(modelName);
            Dictionary<string, object> fieldsDataByFName = new Dictionary<string, object>();
            foreach (var fieldData in model.FieldsGet(new List<string>
            {
                "DefinitionRecordField", "DefinitionRecord", "Aggregator",
                "Name", "Readonly", "Related", "Relation", "Required", "Searchable",
                "Selection", "Sortable", "Store", "String", "Tracking", "Type"
            }))
            {
                if (fieldData.Value.Get("Selectable", true) as bool && (
                    !fieldData.Value.ContainsKey("Relation") || fieldData.Value.Get("Relation") as string == modelName
                ))
                {
                    fieldsDataByFName.Add(fieldData.Key, fieldData.Value);
                }
            }
            fieldsDataByFName = fieldsDataByFName.Where(field => !field.Value.ContainsKey("Related") || (field.Value.Get("Related") as string).Split('.')[0] == modelName).ToDictionary(x => x.Key, x => x.Value);
            foreach (var fieldData in fieldsDataByFName)
            {
                if (model.Fields.ContainsKey(fieldData.Key))
                {
                    List<WebIrModelField> inverseFields = model.Pool.FieldInverses[model.Fields[fieldData.Key]].Where(field => field.ModelName == modelName).ToList();
                    if (inverseFields.Count > 0)
                    {
                        fieldData.Value.Add("InverseFNameByModelName", inverseFields.ToDictionary(field => field.ModelName, field => field.Name));
                    }
                    if (fieldData.Value.Get("Type") as string == "many2one_reference")
                    {
                        fieldData.Value.Add("ModelNameRefFName", model.Fields[fieldData.Key].ModelField);
                    }
                }
            }
            modelDefinitions.Add(modelName, new Dictionary<string, object>
            {
                { "Description", model.Description },
                { "Fields", fieldsDataByFName },
                { "Inherit", model.InheritModule.Where(modelName => modelNames.Contains(modelName)).ToList() },
                { "Order", model.Order },
                { "ParentName", model.ParentName },
                { "RecName", model.RecName },
            });
        }
        return modelDefinitions;
    }
}
