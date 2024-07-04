csharp
public partial class WebsiteResUsers {

    public void CheckLogin() {
        // Check if two users with the same login exist
        // Using Env to access other objects
        var users = Env.GetModel("Website.ResUsers").Search(
            new[] {
                new SearchCondition("Login", "in", this.Login),
                new SearchCondition("WebsiteId", "=", null)
            }
        );
        if (users.Count > 1) {
            throw new ValidationError("You can not have two users with the same login!");
        }
    }

    public Domain GetLoginDomain(string login) {
        // Get the current website
        var website = Env.GetModel("Website.Website").GetCurrentWebsite();
        return new Domain(base.GetLoginDomain(login).Conditions.Concat(website.WebsiteDomain().Conditions));
    }

    public Domain GetEmailDomain(string email) {
        // Get the current website
        var website = Env.GetModel("Website.Website").GetCurrentWebsite();
        return new Domain(base.GetEmailDomain(email).Conditions.Concat(website.WebsiteDomain().Conditions));
    }

    public string GetLoginOrder() {
        return "WebsiteId, " + base.GetLoginOrder();
    }

    public WebsiteResUsers SignupCreateUser(Dictionary<string, object> values) {
        // Get the current website
        var currentWebsite = Env.GetModel("Website.Website").GetCurrentWebsite();
        values["CompanyId"] = currentWebsite.CompanyId;
        values["CompanyIds"] = new List<Command>() { Command.Link(currentWebsite.CompanyId) };
        if (Env.Request != null && currentWebsite.SpecificUserAccount) {
            values["WebsiteId"] = currentWebsite.Id;
        }
        return base.SignupCreateUser(values);
    }

    public Domain GetSignupInvitationScope() {
        // Get the current website
        var currentWebsite = Env.GetModel("Website.Website").Search(new[] { new SearchCondition("Id", "=", Env.User.Id) });
        return currentWebsite.AuthSignupUninvited || base.GetSignupInvitationScope();
    }

    public int Authenticate(string db, string login, string password, string userAgentEnv) {
        // Check if there is an anonymous visitor
        var visitorPreAuthenticateSudo = Env.Request != null ? Env.GetModel("Website.Visitor")._GetVisitorFromRequest() : null;
        int uid = base.Authenticate(db, login, password, userAgentEnv);
        if (uid > 0 && visitorPreAuthenticateSudo != null) {
            // Get the user's partner
            var userPartner = Env.User.PartnerId;
            // Get the visitor associated with the user
            var visitorCurrentUserSudo = Env.GetModel("Website.Visitor").Search(
                new[] {
                    new SearchCondition("PartnerId", "=", userPartner.Id)
                }, 
                1
            );
            if (visitorCurrentUserSudo != null) {
                // Merge the visitor records
                if (visitorPreAuthenticateSudo != visitorCurrentUserSudo) {
                    visitorPreAuthenticateSudo._MergeVisitor(visitorCurrentUserSudo);
                }
                visitorCurrentUserSudo._UpdateVisitorLastVisit();
            } else {
                visitorPreAuthenticateSudo.AccessToken = userPartner.Id;
                visitorPreAuthenticateSudo._UpdateVisitorLastVisit();
            }
        }
        return uid;
    }

    public void CheckOneUserType() {
        base.CheckOneUserType();
        var internalUsers = Env.Ref("Base.GroupUser").Users & this;
        if (internalUsers.Any(user => user.WebsiteId != null)) {
            throw new ValidationError("Remove website on related partner before they become internal user.");
        }
    }
}
