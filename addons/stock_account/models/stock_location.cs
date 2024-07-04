csharp
public partial class StockLocation {
    public bool ShouldBeValued() {
        return this.Usage == "internal" || (this.Usage == "transit" && Env.Company.Id != null);
    }
}
