csharp
public partial class ResPartner {
    public int PosOrderCount { get; set; }

    public List<PosOrder> PosOrderIds { get; set; }

    public void ComputePosOrder() {
        // Retrieve all children partners and prefetch 'Parent' on them
        var allPartners = Env.Model("ResPartner").SearchFetch(new List<object> {
            new List<object> { "Id", "ChildOf", this.Id }
        }, new List<string> { "Parent" });

        var posOrderData = Env.Model("PosOrder").ReadGroup(
            new List<object> {
                new List<object> { "PartnerId", "In", allPartners.Select(p => p.Id).ToList() }
            },
            new List<string> { "PartnerId" },
            new List<string> { "__Count" }
        );

        var selfIds = new HashSet<int> { this.Id };

        this.PosOrderCount = 0;

        foreach (var partnerCount in posOrderData) {
            var partner = Env.Model("ResPartner").Browse(partnerCount["PartnerId"]);
            while (partner != null) {
                if (selfIds.Contains(partner.Id)) {
                    partner.PosOrderCount += Convert.ToInt32(partnerCount["__Count"]);
                }
                partner = partner.Parent;
            }
        }
    }

    public Dictionary<string, object> ActionViewPosOrder() {
        var action = Env.Model("IrActionsActWindow")._ForXmlId("PointOfSale.ActionPosPosForm");
        if (this.IsCompany) {
            action["Domain"] = new List<object> {
                new List<object> { "PartnerId.CommercialPartnerId", "=", this.Id }
            };
        } else {
            action["Domain"] = new List<object> {
                new List<object> { "PartnerId", "=", this.Id }
            };
        }
        return action;
    }

    public Dictionary<string, object> OpenCommercialEntity() {
        var result = new Dictionary<string, object> {
            { "Target", Env.Context.Get("Target") == "new" ? "new" : null }
        };
        return result.Merge(base.OpenCommercialEntity());
    }

    private List<object> _LoadPosDataDomain(Dictionary<string, object> data) {
        var configId = Env.Model("PosConfig").Browse(data["PosConfig"]["Data"][0]["Id"]);
        return new List<object> {
            new List<object> { "Id", "In", configId.GetLimitedPartnersLoading().Concat(new List<int> { Env.User.PartnerId.Id }).ToList() }
        };
    }

    private List<string> _LoadPosDataFields(int configId) {
        return new List<string> {
            "Id", "Name", "Street", "City", "StateId", "CountryId", "Vat", "Lang", "Phone", "Zip", "Mobile", "Email",
            "Barcode", "WriteDate", "PropertyAccountPositionId", "PropertyProductPricelist", "ParentName"
        };
    }
}
