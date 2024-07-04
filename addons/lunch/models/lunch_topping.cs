csharp
public partial class LunchTopping {

    public void ComputeDisplayName() {
        var currencyId = this.Env.Company.CurrencyId;
        this.DisplayName = $"{this.Name} {this.Env.FormatLang(this.Price, currencyId)}";
    }
}
