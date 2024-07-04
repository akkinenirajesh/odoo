csharp
public partial class PosSelfOrder.ProductTemplate {

    public void OnChangeAvailableInPos() {
        if (!this.AvailableInPos) {
            this.SelfOrderAvailable = false;
        }
    }

    public void Write(Dictionary<string, object> valsList) {
        if (valsList.ContainsKey("AvailableInPos")) {
            if (!(bool)valsList["AvailableInPos"]) {
                valsList["SelfOrderAvailable"] = false;
            }
        }

        // Call base write method
        base.Write(valsList);

        if (valsList.ContainsKey("SelfOrderAvailable")) {
            foreach (var record in this) {
                foreach (var product in record.ProductVariantIds) {
                    product.SendAvailabilityStatus();
                }
            }
        }
    }
}

public partial class PosSelfOrder.ProductProduct {

    public List<string> LoadPosDataFields(int configId) {
        List<string> params = base.LoadPosDataFields(configId);
        params.Add("SelfOrderAvailable");
        return params;
    }

    public List<string> LoadPosSelfDataFields(int configId) {
        List<string> params = base.LoadPosSelfDataFields(configId);
        params.Add("DescriptionSelfOrder");
        return params;
    }

    public string GetName() {
        return this.DisplayName;
    }

    public List<Dictionary<string, object>> FilterApplicableAttributes(Dictionary<int, Dictionary<string, object>> attributesByPtalId) {
        return attributesByPtalId.Where(x => this.AttributeLineIds.Contains(x.Key))
            .Select(x => x.Value).ToList();
    }

    public void Write(Dictionary<string, object> valsList) {
        // Call base write method
        base.Write(valsList);

        if (valsList.ContainsKey("SelfOrderAvailable")) {
            this.SendAvailabilityStatus();
        }
    }

    public void SendAvailabilityStatus() {
        var configSelf = Env.Get("Pos.Config").Search(x => x.SelfOrderingMode != "nothing");
        foreach (var config in configSelf) {
            if (config.CurrentSessionId != null && config.AccessToken != null) {
                config.Notify("PRODUCT_CHANGED", new Dictionary<string, object>() {
                    { "product.product", this.Read(this.LoadPosSelfDataFields(config.Id), false) }
                });
            }
        }
    }
}
