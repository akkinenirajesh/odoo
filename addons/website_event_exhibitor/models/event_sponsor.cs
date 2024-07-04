C#
public partial class WebsiteEventExhibitor_EventSponsor {

    public void ComputeUrl() {
        if (Env.Ref("Core.Website").GetWebsite(this) != null && this.Url == null) {
            this.Url = Env.Ref("Core.Partner").GetPartner(this.PartnerId).Website;
        }
    }

    public void ComputeName() {
        SynchronizeWithPartner("Name");
    }

    public void ComputeEmail() {
        SynchronizeWithPartner("Email");
    }

    public void ComputePhone() {
        SynchronizeWithPartner("Phone");
    }

    public void ComputeMobile() {
        SynchronizeWithPartner("Mobile");
    }

    public void ComputeImage512() {
        SynchronizeWithPartner("Image512");
    }

    public void ComputeWebsiteImageUrl() {
        if (this.Image512 != null) {
            this.WebsiteImageUrl = Env.Ref("Core.Website").GetImageUrl(this, "Image256", 256);
        } else if (Env.Ref("Core.Partner").GetPartner(this.PartnerId).Image256 != null) {
            this.WebsiteImageUrl = Env.Ref("Core.Website").GetImageUrl(Env.Ref("Core.Partner").GetPartner(this.PartnerId), "Image256", 256);
        } else {
            this.WebsiteImageUrl = "/website_event_exhibitor/static/src/img/event_sponsor_default.svg";
        }
    }

    private void SynchronizeWithPartner(string fieldName) {
        if (this.GetType().GetProperty(fieldName).GetValue(this, null) == null) {
            this.GetType().GetProperty(fieldName).SetValue(this, Env.Ref("Core.Partner").GetPartner(this.PartnerId).GetType().GetProperty(fieldName).GetValue(Env.Ref("Core.Partner").GetPartner(this.PartnerId), null));
        }
    }

    public void OnchangeExhibitorType() {
        if (this.ExhibitorType == "online" && this.RoomName == null) {
            if (this.Name != null) {
                this.RoomName = "odoo-exhibitor-" + this.Name;
            } else {
                this.RoomName = Env.Ref("Core.ChatRoom").GetDefaultName("exhibitor");
            }
            this.RoomName = JitSiSanitizeName(this.RoomName);
        }
        if (this.ExhibitorType == "online" && this.GetType().GetProperty("RoomMaxCapacity").GetValue(this, null) == null) {
            this.GetType().GetProperty("RoomMaxCapacity").SetValue(this, "8");
        }
    }

    public void ComputeWebsiteDescription() {
        if (Env.Ref("Core.Utils").IsHtmlEmpty(this.WebsiteDescription)) {
            this.WebsiteDescription = Env.Ref("Core.Partner").GetPartner(this.PartnerId).WebsiteDescription;
        }
    }

    public void ComputeIsInOpeningHours() {
        var event = Env.Ref("WebsiteEvent.Event").GetEvent(this.EventId);
        if (!event.IsOngoing) {
            this.IsInOpeningHours = false;
        } else if (this.HourFrom == null || this.HourTo == null) {
            this.IsInOpeningHours = true;
        } else {
            var eventTz = TimeZoneInfo.FindSystemTimeZoneById(event.DateTz);
            var dtBegin = DateTime.SpecifyKind(event.DateBegin, DateTimeKind.Utc).ToTimeZone(eventTz);
            var dtEnd = DateTime.SpecifyKind(event.DateEnd, DateTimeKind.Utc).ToTimeZone(eventTz);
            var nowUtc = DateTime.UtcNow;
            var nowTz = TimeZoneInfo.ConvertTime(nowUtc, eventTz);

            var openingFromTz = TimeZoneInfo.ConvertTime(DateTime.Parse(nowTz.ToShortDateString() + " " + Env.Ref("Core.Utils").FloatToTime(this.HourFrom)), eventTz);
            var openingToTz = TimeZoneInfo.ConvertTime(DateTime.Parse(nowTz.ToShortDateString() + " " + Env.Ref("Core.Utils").FloatToTime(this.HourTo)), eventTz);
            if (this.HourTo == 0) {
                openingToTz = openingToTz.AddDays(1);
            }

            var openingFrom = DateTime.Compare(dtBegin, openingFromTz) > 0 ? dtBegin : openingFromTz;
            var openingTo = DateTime.Compare(dtEnd, openingToTz) < 0 ? dtEnd : openingToTz;

            this.IsInOpeningHours = DateTime.Compare(openingFrom, nowTz) <= 0 && DateTime.Compare(nowTz, openingTo) < 0;
        }
    }

    public void ComputeCountryFlagUrl() {
        if (Env.Ref("Core.Partner").GetPartner(this.PartnerId).CountryId != null) {
            this.CountryFlagUrl = Env.Ref("Core.Country").GetCountry(Env.Ref("Core.Partner").GetPartner(this.PartnerId).CountryId).ImageUrl;
        } else {
            this.CountryFlagUrl = null;
        }
    }

    public void ComputeWebsiteUrl() {
        if (this.Id != null) {
            var baseUrl = Env.Ref("WebsiteEvent.Event").GetEvent(this.EventId).GetBaseUrl();
            this.WebsiteUrl = string.Format("{0}/event/{1}/exhibitor/{2}", baseUrl, Env.Ref("Core.Utils").Slug(Env.Ref("WebsiteEvent.Event").GetEvent(this.EventId)), Env.Ref("Core.Utils").Slug(this));
        }
    }

    public long GetBackendMenuId() {
        return Env.Ref("WebsiteEvent.EventMainMenu").Id;
    }

    public void OpenWebsiteUrl() {
        if (Env.Ref("WebsiteEvent.Event").GetEvent(this.EventId).WebsiteId != null) {
            base.OpenWebsiteUrl();
        } else {
            Env.Ref("Core.Website").GetClientAction(string.Format("/event/{0}/exhibitor/{1}", Env.Ref("Core.Utils").Slug(Env.Ref("WebsiteEvent.Event").GetEvent(this.EventId)), Env.Ref("Core.Utils").Slug(this)));
        }
    }

    public List<Core.Partner> _MessageGetSuggestedRecipients() {
        var recipients = base._MessageGetSuggestedRecipients();
        if (this.PartnerId != null) {
            _MessageAddSuggestedRecipient(recipients, Env.Ref("Core.Partner").GetPartner(this.PartnerId), "Sponsor");
        }
        return recipients;
    }

    private string JitSiSanitizeName(string name) {
        return name;
    }

    private void _MessageAddSuggestedRecipient(List<Core.Partner> recipients, Core.Partner partner, string reason) {
    }
}
