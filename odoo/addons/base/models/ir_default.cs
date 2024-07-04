csharp
public partial class BaseIrDefault 
{
    public void Set(string modelName, string fieldName, object value, int? userId = null, int? companyId = null, string condition = null)
    {
        // check consistency of model_name, field_name, and value
        try
        {
            var model = Env.GetModel(modelName);
            var field = model.GetField(fieldName);
            var parsed = field.ConvertToCache(value, model);
            var jsonValue = Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }
        catch (KeyNotFoundException)
        {
            throw new Exception($"Invalid field {modelName}.{fieldName}");
        }
        catch (Exception)
        {
            throw new Exception($"Invalid value for {modelName}.{fieldName}: {value}");
        }

        if (field.Type == "integer" && !(Math.Pow(-2, 31) < parsed && parsed < Math.Pow(2, 31) - 1))
        {
            throw new Exception($"Invalid value for {modelName}.{fieldName}: {value} is out of bounds (integers should be between -2,147,483,648 and 2,147,483,647)");
        }

        // update existing default for the same scope, or create one
        var field = Env.GetModel("Base.IrModelFields").Get(modelName, fieldName);
        var defaultRecord = this.Search(
            new Dictionary<string, object>()
            {
                {"FieldId", field.Id},
                {"UserId", userId},
                {"CompanyId", companyId},
                {"Condition", condition},
            },
            1
        );

        if (defaultRecord != null)
        {
            if (defaultRecord.JsonValue != jsonValue)
            {
                defaultRecord.JsonValue = jsonValue;
                defaultRecord.Update();
            }
        }
        else
        {
            this.Create(new Dictionary<string, object>()
            {
                {"FieldId", field.Id},
                {"UserId", userId},
                {"CompanyId", companyId},
                {"Condition", condition},
                {"JsonValue", jsonValue},
            });
        }
    }
}
