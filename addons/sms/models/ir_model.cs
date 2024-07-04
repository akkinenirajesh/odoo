C#
public partial class IrModel {

    public bool IsMailThreadSms { get; set; }

    public void ComputeIsMailThreadSms() {
        if (this.IsMailThread) {
            var ModelObject = Env.GetModel(this.Model);
            var potentialFields = ModelObject.PhoneGetNumberFields().Concat(ModelObject.MailGetPartnerFields()).ToList();
            if (potentialFields.Any(fname => ModelObject.Fields.ContainsKey(fname))) {
                this.IsMailThreadSms = true;
                return;
            }
        }
        this.IsMailThreadSms = false;
    }

    public IEnumerable<int> SearchIsMailThreadSms(string operator, bool value) {
        var threadModels = Env.GetModel("Sms.IrModel").Search(new List<object> { new Dictionary<string, object> { { "IsMailThread", true } } });
        var validModels = Env.GetModel("Sms.IrModel");
        foreach (var model in threadModels) {
            if (!Env.HasModel(model.Model)) {
                continue;
            }
            var ModelObject = Env.GetModel(model.Model);
            var potentialFields = ModelObject.PhoneGetNumberFields().Concat(ModelObject.MailGetPartnerFields()).ToList();
            if (potentialFields.Any(fname => ModelObject.Fields.ContainsKey(fname))) {
                validModels |= model;
            }
        }
        var searchSms = (operator == "=" && value) || (operator == "!=" && !value);
        if (searchSms) {
            return validModels.Select(m => m.Id);
        }
        return validModels.Select(m => m.Id).Select(id => -id);
    }
}
