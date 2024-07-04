csharp
public partial class MailIrModel
{
    public void Unlink()
    {
        if (this == null)
        {
            return;
        }

        // Delete followers, messages and attachments for models that will be unlinked.
        var mailModels = Env.Search<MailIrModel>("model IN ('mail.activity', 'mail.activity.type', 'mail.followers', 'mail.message')");

        if (!(this & mailModels))
        {
            var models = this.Get<string>("model");
            var modelIds = this.Get<int>("id");

            Env.Execute("DELETE FROM mail_activity WHERE res_model_id IN " + modelIds);
            Env.Execute("DELETE FROM mail_activity_type WHERE res_model IN " + models);
            Env.Execute("DELETE FROM mail_followers WHERE res_model IN " + models);
            Env.Execute("DELETE FROM mail_message WHERE model IN " + models);
        }

        // Get files attached solely to the models being deleted (and none other)
        var models = this.Get<string>("model");
        var fnames = Env.Execute("SELECT DISTINCT store_fname FROM ir_attachment WHERE res_model IN " + models + " EXCEPT SELECT store_fname FROM ir_attachment WHERE res_model NOT IN " + models);

        Env.Execute("DELETE FROM ir_attachment WHERE res_model IN " + models);

        foreach (var fname in fnames)
        {
            Env.Call("IrAttachment", "_FileDelete", new { fname });
        }

        base.Unlink();
    }

    public void Write(object vals)
    {
        if (this != null && (vals.ContainsKey("IsMailThread") || vals.ContainsKey("IsMailActivity") || vals.ContainsKey("IsMailBlacklist")))
        {
            if (this.Any(x => x.Get<string>("state") != "manual"))
            {
                throw new Exception("Only custom models can be modified.");
            }

            if (vals.ContainsKey("IsMailThread") && this.Any(x => x.Get<bool>("IsMailThread") > (bool)vals["IsMailThread"]))
            {
                throw new Exception("Field \"Mail Thread\" cannot be changed to \"False\".");
            }

            if (vals.ContainsKey("IsMailActivity") && this.Any(x => x.Get<bool>("IsMailActivity") > (bool)vals["IsMailActivity"]))
            {
                throw new Exception("Field \"Mail Activity\" cannot be changed to \"False\".");
            }

            if (vals.ContainsKey("IsMailBlacklist") && this.Any(x => x.Get<bool>("IsMailBlacklist") > (bool)vals["IsMailBlacklist"]))
            {
                throw new Exception("Field \"Mail Blacklist\" cannot be changed to \"False\".");
            }

            base.Write(vals);
            Env.FlushAll();
            Env.SetupModels(this.Get<string>("model"));
            var models = Env.Descendants(this.Get<string>("model"), "_inherits");
            Env.InitModels(models, new { update_custom_fields = true });
        }
        else
        {
            base.Write(vals);
        }
    }

    public object ReflectModelParams(string model)
    {
        var vals = base.ReflectModelParams(model);
        vals["IsMailThread"] = Env.IsInstanceOf(model, "mail.thread");
        vals["IsMailActivity"] = Env.IsInstanceOf(model, "mail.activity.mixin");
        vals["IsMailBlacklist"] = Env.IsInstanceOf(model, "mail.thread.blacklist");
        return vals;
    }

    public object Instanciate(object modelData)
    {
        var modelClass = base.Instanciate(modelData);
        if (modelData.ContainsKey("IsMailBlacklist") && modelClass.Get<string>("name") != "mail.thread.blacklist")
        {
            var parents = modelClass.Get<string>("_inherit");
            var parentsList = parents.Split(',').ToList();
            parentsList.Add("mail.thread.blacklist");
            modelClass.Set("_inherit", parentsList);
            if (modelClass.Get<bool>("_custom"))
            {
                modelClass.Set("_primary_email", "x_email");
            }
        }
        else if (modelData.ContainsKey("IsMailThread") && modelClass.Get<string>("name") != "mail.thread")
        {
            var parents = modelClass.Get<string>("_inherit");
            var parentsList = parents.Split(',').ToList();
            parentsList.Add("mail.thread");
            modelClass.Set("_inherit", parentsList);
        }

        if (modelData.ContainsKey("IsMailActivity") && modelClass.Get<string>("name") != "mail.activity.mixin")
        {
            var parents = modelClass.Get<string>("_inherit");
            var parentsList = parents.Split(',').ToList();
            parentsList.Add("mail.activity.mixin");
            modelClass.Set("_inherit", parentsList);
        }

        return modelClass;
    }

    public object GetDefinitions(List<string> modelNames)
    {
        var modelDefinitions = base.GetDefinitions(modelNames);
        foreach (var modelName in modelNames)
        {
            var model = Env.GetModel(modelName);
            var trackedFieldNames = model.Get<List<string>>("_track_get_fields");
            foreach (var fname in trackedFieldNames)
            {
                if (modelDefinitions[modelName]["fields"].ContainsKey(fname))
                {
                    modelDefinitions[modelName]["fields"][fname]["tracking"] = true;
                }
            }

            if (Env.IsInstanceOf(modelName, "mail.activity.mixin"))
            {
                modelDefinitions[modelName]["has_activities"] = true;
            }
        }

        return modelDefinitions;
    }

    public object GetModelDefinitions(List<string> modelNamesToFetch)
    {
        var modelDefinitions = base.GetModelDefinitions(modelNamesToFetch);
        foreach (var modelName in modelNamesToFetch)
        {
            var model = Env.GetModel(modelName);
            var trackedFieldNames = model.Get<List<string>>("_track_get_fields");
            foreach (var fname in trackedFieldNames)
            {
                if (modelDefinitions[modelName]["fields"].ContainsKey(fname))
                {
                    modelDefinitions[modelName]["fields"][fname]["tracking"] = true;
                }
            }

            if (Env.IsInstanceOf(modelName, "mail.activity.mixin"))
            {
                modelDefinitions[modelName]["has_activities"] = true;
            }
        }

        return modelDefinitions;
    }
}
