C#
public partial class MassMailingIrModel {
    public void ComputeIsMailingEnabled() {
        this.IsMailingEnabled = (bool)Env.Get(this.Model).GetProperty("_mailing_enabled");
    }

    public List<object> SearchIsMailingEnabled(string operator, object value) {
        if (operator != "=" && operator != "!=") {
            throw new Exception("Searching Mailing Enabled models supports only direct search using '=' or '!='.");
        }
        List<object> validModels = new List<object>();
        var allModels = Env.Get("ir.model").Search([]);
        foreach (object model in allModels) {
            if (!Env.Contains(model.Get("Model")) || (bool)model.Get("IsTransient")) {
                continue;
            }
            if ((bool)Env.Get(model.Get("Model")).GetProperty("_mailing_enabled")) {
                validModels.Add(model);
            }
        }
        bool searchIsMailingEnabled = (operator == "=" && (bool)value) || (operator == "!=" && !(bool)value);
        if (searchIsMailingEnabled) {
            return new List<object> { new { id = new List<object>(validModels.Select(x => x.Get("Id"))) } };
        }
        return new List<object> { new { id = new List<object>(validModels.Select(x => x.Get("Id"))) } };
    }
}
