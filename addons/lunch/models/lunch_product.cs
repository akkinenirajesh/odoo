C#
public partial class LunchProduct {
    public void ComputeIsNew() {
        DateTime today = Env.Context.Today;
        this.IsNew = this.NewUntil != null && today <= this.NewUntil;
    }

    public void ComputeIsFavorite() {
        this.IsFavorite = Env.User.FavoriteLunchProductIDs.Contains(this.ID);
    }

    public void InverseIsFavorite() {
        if (this.IsFavorite) {
            Env.User.FavoriteLunchProductIDs.Add(this.ID);
        } else {
            Env.User.FavoriteLunchProductIDs.Remove(this.ID);
        }
    }

    public void ComputeLastOrderDate() {
        var allOrders = Env.Search<LunchOrder>(
            x => x.UserID == Env.User.ID && x.ProductID == this.ID
        );
        this.LastOrderDate = allOrders.Max(x => x.Date);
    }

    public void ComputeProductImage() {
        this.ProductImage = this.Image128 ?? this.CategoryID.Image128;
    }

    public void ComputeIsAvailableAt() {
        this.IsAvailableAt = null;
    }

    public Domain SearchIsAvailableAt(string operator, object value) {
        if (operator == "in" || operator == "not in" || operator == "=" || operator == "!=") {
            if (value is int) {
                value = new List<int>() { (int)value };
            }
            if (operator == "in" || operator == "=") {
                return new Domain(new[] {
                    new DomainPart("SupplierID.AvailableLocationIDs", "in", value),
                    new DomainPart("SupplierID.AvailableLocationIDs", "=", null)
                });
            } else {
                return new Domain(new[] {
                    new DomainPart("SupplierID.AvailableLocationIDs", "not in", value),
                    new DomainPart("SupplierID.AvailableLocationIDs", "!=", null)
                });
            }
        }
        return Domain.True;
    }

    public void SyncActiveFromRelated() {
        if ((this.CategoryID.Active && this.SupplierID.Active) != this.Active) {
            this.ToggleActive();
        }
    }

    public void ToggleActive() {
        if (!this.Active && !this.CategoryID.Active) {
            throw new UserError($"The following product categories are archived. You should either unarchive the categories or change the category of the product.\n{string.Join("\n", this.CategoryID.Name)}");
        }
        if (!this.Active && !this.SupplierID.Active) {
            throw new UserError($"The following suppliers are archived. You should either unarchive the suppliers or change the supplier of the product.\n{string.Join("\n", this.SupplierID.Name)}");
        }
        this.Active = !this.Active;
    }

    public void Write(Dictionary<string, object> vals) {
        if (vals.ContainsKey("IsFavorite")) {
            if ((bool)vals["IsFavorite"]) {
                Env.User.FavoriteLunchProductIDs.Add(this.ID);
            } else {
                Env.User.FavoriteLunchProductIDs.Remove(this.ID);
            }
            vals.Remove("IsFavorite");
        }
        if (vals.Count > 0) {
            base.Write(vals);
        }
    }
}
