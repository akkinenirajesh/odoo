csharp
public partial class WebsiteEventBoothExhibitor.EventBooth {
    public virtual Action ActionViewSponsor() {
        var action = Env.Ref<IrActionsActWindow>("website_event_exhibitor.event_sponsor_action");
        action.Views = new List<Tuple<object, string>> { new Tuple<object, string>(false, "form") };
        action.ResId = SponsorId.Id;
        return action;
    }

    public virtual int GetOrCreateSponsor(Dictionary<string, object> vals) {
        var sponsorId = Env.Search<WebsiteEventExhibitor.Sponsor>(new[] {
            new Tuple<string, object>("PartnerId", this.PartnerId.Id),
            new Tuple<string, object>("SponsorTypeId", this.SponsorTypeId.Id),
            new Tuple<string, object>("ExhibitorType", this.BoothCategory.ExhibitorType),
            new Tuple<string, object>("EventId", this.EventId.Id)
        }, limit: 1);

        if (sponsorId == null) {
            var values = new Dictionary<string, object> {
                { "EventId", this.EventId.Id },
                { "SponsorTypeId", this.SponsorTypeId.Id },
                { "ExhibitorType", this.BoothCategory.ExhibitorType },
                { "PartnerId", this.PartnerId.Id },
            };

            foreach (var item in vals) {
                if (item.Key.StartsWith("Sponsor")) {
                    values.Add(item.Key.Substring("Sponsor".Length), item.Value);
                }
            }

            if (!values.ContainsKey("Name")) {
                values["Name"] = this.PartnerId.Name;
            }

            if (this.BoothCategory.ExhibitorType == "online") {
                values["RoomName"] = "odoo-exhibitor-" + this.PartnerId.Name;
            }

            sponsorId = Env.Create<WebsiteEventExhibitor.Sponsor>(values);
        }

        return sponsorId.Id;
    }

    public virtual void ActionPostConfirm(Dictionary<string, object> writeVals) {
        if (this.UseSponsor && this.PartnerId != null) {
            this.SponsorId = GetOrCreateSponsor(writeVals);
        }

        base.ActionPostConfirm(writeVals);
    }
}
