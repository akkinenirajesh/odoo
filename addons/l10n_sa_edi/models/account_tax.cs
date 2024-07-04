csharp
public partial class AccountTax {
    public void OnChangeAmount() {
        // super().OnChangeAmount(); // Not applicable in C#
        this.L10nSaIsRetention = false;
    }

    public void ConstrainL10nSaIsRetention() {
        if (this.Amount >= 0 && this.L10nSaIsRetention && this.TypeTaxUse == "Sale") {
            throw new Exception("Cannot set a tax to Retention if the amount is greater than or equal 0");
        }
    }
}
