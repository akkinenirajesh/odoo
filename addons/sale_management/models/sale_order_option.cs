C#
public partial class SaleOrderOption {
    public void ComputeName() {
        if (this.ProductId == null) {
            return;
        }
        // Implement the logic to compute the name
    }

    public void ComputeUomId() {
        if (this.ProductId == null || this.UomId != null) {
            return;
        }
        // Implement the logic to compute the UomId
    }

    public void ComputePriceUnit() {
        if (this.ProductId == null) {
            return;
        }
        // Implement the logic to compute the PriceUnit
    }

    public void ComputeDiscount() {
        if (this.ProductId == null) {
            return;
        }
        // Implement the logic to compute the Discount
    }

    public void ComputeIsPresent() {
        // Implement the logic to compute the IsPresent
    }

    public void SearchIsPresent(string operator, bool value) {
        // Implement the logic to search the IsPresent
    }

    public void ButtonAddToOrder() {
        AddOptionToOrder();
    }

    public void AddOptionToOrder() {
        if (!Env.Call<SaleOrder>("_CanToBeEditedOnPortal", this.OrderId)) {
            throw new Exception("You cannot add options to a confirmed order.");
        }
        // Implement the logic to add the option to the order
    }

    public Dictionary<string, object> GetValuesToAddToOrder() {
        return new Dictionary<string, object>() {
            { "OrderId", this.OrderId },
            { "PriceUnit", this.PriceUnit },
            { "Name", this.Name },
            { "ProductId", this.ProductId },
            { "ProductUomQty", this.Quantity },
            { "ProductUom", this.UomId },
            { "Discount", this.Discount },
            { "Sequence", Env.Call<SaleOrderLine>("Max", this.OrderId, "Sequence", default(int)) + 1 }
        };
    }
}
