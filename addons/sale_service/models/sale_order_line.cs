C#
public partial class SaleOrderLine {
    public bool IsService { get; set; }

    public void ComputeIsService() {
        this.IsService = this.Product.Type == "service";
    }
}
