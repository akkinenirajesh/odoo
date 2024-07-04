C#
public partial class MailAliasDomain {
    public void ComputeBounceEmail() {
        if (this.BounceAlias != null) {
            this.BounceEmail = $"{this.BounceAlias}@{this.Name}";
        }
    }

    public void ComputeCatchallEmail() {
        if (this.CatchallAlias != null) {
            this.CatchallEmail = $"{this.CatchallAlias}@{this.Name}";
        }
    }

    public void ComputeDefaultFromEmail() {
        if (this.DefaultFrom != null) {
            if (this.DefaultFrom.Contains("@")) {
                this.DefaultFromEmail = this.DefaultFrom;
            } else {
                this.DefaultFromEmail = $"{this.DefaultFrom}@{this.Name}";
            }
        }
    }

    public void CheckBounceCatchallUniqueness() {
        // TODO: Implement uniqueness validation logic
    }

    public void CheckName() {
        // TODO: Implement name validation logic
    }

    public void SanitizeConfiguration(Dictionary<string, object> configValues) {
        if (configValues.ContainsKey("BounceAlias")) {
            configValues["BounceAlias"] = Env.Ref("Mail.Alias")._SanitizeAliasName(configValues["BounceAlias"].ToString());
        }
        if (configValues.ContainsKey("CatchallAlias")) {
            configValues["CatchallAlias"] = Env.Ref("Mail.Alias")._SanitizeAliasName(configValues["CatchallAlias"].ToString());
        }
        if (configValues.ContainsKey("DefaultFrom")) {
            configValues["DefaultFrom"] = Env.Ref("Mail.Alias")._SanitizeAliasName(configValues["DefaultFrom"].ToString(), true);
        }
    }

    public void MigrateIcpToDomain() {
        // TODO: Implement migration logic
    }
}
