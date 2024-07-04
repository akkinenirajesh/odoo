csharp
public partial class WebsiteVisitor {
    public void ComputeDisplayName() {
        // If there is an event registration for an anonymous visitor, use that
        // registered attendee name as visitor name.
        if (!this.PartnerId && this.EventRegistrationIds.Count > 0) {
            this.DisplayName = this.EventRegistrationIds[this.EventRegistrationIds.Count - 1].Name;
        }
    }

    public void ComputeEventRegistrationCount() {
        this.EventRegistrationCount = Env.GetModel("Event.Registration").ReadGroup(new[] {
            new SearchField("VisitorId", SearchOperator.In, new[] { this.Id }),
        }, new[] { "VisitorId" }, new[] { "__count" }).Count;
    }

    public void ComputeEmailPhone() {
        if (string.IsNullOrEmpty(this.Email) || string.IsNullOrEmpty(this.Mobile)) {
            var linkedRegistrations = this.EventRegistrationIds.OrderBy(reg => (reg.CreateDate, reg.Id)).ToList();
            if (string.IsNullOrEmpty(this.Email)) {
                this.Email = linkedRegistrations.FirstOrDefault(reg => !string.IsNullOrEmpty(reg.Email))?.Email;
            }
            if (string.IsNullOrEmpty(this.Mobile)) {
                this.Mobile = linkedRegistrations.FirstOrDefault(reg => !string.IsNullOrEmpty(reg.Phone))?.Phone;
            }
        }
    }

    public void ComputeEventRegisteredIds() {
        // include parent's registrations in a visitor o2m field. We don't add
        // child one as child should not have registrations (moved to the parent)
        this.EventRegisteredIds = this.EventRegistrationIds.Select(reg => reg.EventId).ToList();
    }

    public List<int> SearchEventRegisteredIds(SearchOperator operator, List<int> operand) {
        if (operator == SearchOperator.NotIn) {
            throw new NotImplementedException("Unsupported 'Not In' operation on visitors registrations");
        }

        var allRegistrations = Env.GetModel("Event.Registration").Search(new[] {
            new SearchField("EventId", operator, operand),
        });
        if (allRegistrations.Count > 0) {
            var visitorIds = allRegistrations.Select(reg => reg.VisitorId).ToList();
            return visitorIds;
        } else {
            return new List<int>();
        }
    }

    public List<SearchField> InactiveVisitorsDomain() {
        var domain = base.InactiveVisitorsDomain();
        domain.Add(new SearchField("EventRegistrationIds", SearchOperator.Equals, new List<int>()));
        return domain;
    }

    public void MergeVisitor(WebsiteVisitor target) {
        this.EventRegistrationIds.ForEach(reg => reg.VisitorId = target.Id);
        var registrationWoPartner = this.EventRegistrationIds.Where(registration => registration.PartnerId == null).ToList();
        if (registrationWoPartner.Count > 0) {
            registrationWoPartner.ForEach(registration => registration.PartnerId = target.PartnerId);
        }
        base.MergeVisitor(target);
    }
}
