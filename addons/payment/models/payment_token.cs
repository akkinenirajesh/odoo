csharp
public partial class PaymentToken {
    public virtual void ComputeDisplayName() {
        this.DisplayName = BuildDisplayName();
    }

    public virtual void Create(Dictionary<string, object> values) {
        if (values.ContainsKey("ProviderId")) {
            var provider = Env.GetModel("Payment.PaymentProvider").Browse(values["ProviderId"]);
            values.AddOrUpdate(GetSpecificCreateValues(provider.Code, values));
        }
        // Let psycopg warn about the missing required field.
    }

    public virtual Dictionary<string, object> GetSpecificCreateValues(string providerCode, Dictionary<string, object> values) {
        return new Dictionary<string, object>();
    }

    public virtual void Write(Dictionary<string, object> values) {
        if (values.ContainsKey("Active")) {
            if (Convert.ToBoolean(values["Active"])) {
                if (this.Active) {
                    // You can't unarchive tokens linked to inactive payment methods or disabled providers.
                    throw new Exception("You can't unarchive tokens linked to inactive payment methods or disabled providers.");
                }
            } else {
                // Call the handlers in sudo mode because this method might have been called by RPC.
                // this.Sudo().HandleArchiving();
            }
        }
        //  return super().write(values);
    }

    public virtual void CheckPartnerIsNeverPublic() {
        if (this.PartnerId.IsPublic) {
            throw new Exception("No token can be assigned to the public partner.");
        }
    }

    public virtual void HandleArchiving() {
    }

    public virtual IEnumerable<PaymentToken> GetAvailableTokens(IEnumerable<long> providersIds, long partnerId, bool isValidation, Dictionary<string, object> kwargs) {
        if (!isValidation) {
            return Env.GetModel("Payment.PaymentToken").Search(new Dictionary<string, object> {
                { "ProviderId", "in", providersIds },
                { "PartnerId", "=", partnerId },
            });
        } else {
            // Get all the tokens of the partner and of their commercial partner, regardless of whether the providers are available.
            var partner = Env.GetModel("Res.Partner").Browse(partnerId);
            return Env.GetModel("Payment.PaymentToken").Search(new Dictionary<string, object> {
                { "PartnerId", "in", new List<long> { partner.Id, partner.CommercialPartnerId.Id } },
            });
        }
    }

    public virtual string BuildDisplayName(int maxLength = 34, bool shouldPad = true, Dictionary<string, object> kwargs) {
        if (this.CreateDate == null) {
            return string.Empty;
        }

        var paddingLength = maxLength - (this.PaymentDetails?.Length ?? 0);
        if (string.IsNullOrEmpty(this.PaymentDetails)) {
            var createDateStr = this.CreateDate.ToString("yyyy/MM/dd");
            return string.Format("Payment details saved on {0}", createDateStr);
        } else if (paddingLength >= 2) {
            var padding = (shouldPad ? new string('•', Math.Min(paddingLength - 1, 4)) + " " : string.Empty);
            return string.Concat(padding, this.PaymentDetails);
        } else if (paddingLength > 0) {
            return this.PaymentDetails;
        } else {
            return string.IsNullOrEmpty(this.PaymentDetails) ? string.Empty : this.PaymentDetails.Substring(Math.Max(0, this.PaymentDetails.Length - maxLength));
        }
    }

    public virtual List<Dictionary<string, object>> GetLinkedRecordsInfo() {
        return new List<Dictionary<string, object>>();
    }
}
