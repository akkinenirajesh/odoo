csharp
public partial class IrModel
{
    public Dictionary<string, Dictionary<string, object>> GetModelDefinitions(List<string> modelNamesToFetch)
    {
        var modelDefinitions = new Dictionary<string, Dictionary<string, object>>();

        foreach (var modelName in modelNamesToFetch)
        {
            var model = Env.Get(modelName);
            var fieldsDataByFname = new Dictionary<string, Dictionary<string, object>>();

            var fieldsData = model.FieldsGet(new List<string> 
            { 
                "Name", "Type", "Relation", "Required", "Readonly", "Selection",
                "String", "DefinitionRecord", "DefinitionRecordField", "ModelField"
            });

            foreach (var fieldData in fieldsData)
            {
                if (!fieldData.Value.ContainsKey("Relation") || modelNamesToFetch.Contains((string)fieldData.Value["Relation"]))
                {
                    fieldsDataByFname[fieldData.Key] = fieldData.Value;
                }
            }

            foreach (var fieldName in fieldsDataByFname.Keys)
            {
                if (model.Fields.ContainsKey(fieldName))
                {
                    var inverseFields = model.Pool.FieldInverses[model.Fields[fieldName]]
                        .Where(field => modelNamesToFetch.Contains(field.ModelName))
                        .ToList();

                    if (inverseFields.Any())
                    {
                        fieldsDataByFname[fieldName]["InverseFnameByModelName"] = 
                            inverseFields.ToDictionary(field => field.ModelName, field => field.Name);
                    }

                    if ((string)fieldsDataByFname[fieldName]["Type"] == "Many2OneReference")
                    {
                        fieldsDataByFname[fieldName]["ModelNameRefFname"] = 
                            model.Fields[fieldName].ModelField;
                    }
                }
            }

            modelDefinitions[modelName] = new Dictionary<string, object>
            {
                { "Fields", fieldsDataByFname }
            };
        }

        return modelDefinitions;
    }
}
