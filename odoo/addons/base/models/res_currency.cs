csharp
public partial class BaseCurrency {

    public virtual void Create(Dictionary<string, object> valsList) {
        // implement Create
    }

    public virtual void Unlink() {
        // implement Unlink
    }

    public virtual void Write(Dictionary<string, object> vals) {
        // implement Write
    }

    public virtual void ToggleGroupMultiCurrency() {
        // implement ToggleGroupMultiCurrency
    }

    public virtual void ActivateGroupMultiCurrency() {
        // implement ActivateGroupMultiCurrency
    }

    public virtual void DeactivateGroupMultiCurrency() {
        // implement DeactivateGroupMultiCurrency
    }

    public virtual void CheckCompanyCurrencyStaysActive() {
        // implement CheckCompanyCurrencyStaysActive
    }

    public virtual Dictionary<int, decimal> GetRates(ResCompany company, DateTime date) {
        // implement GetRates
    }

    public virtual void ComputeIsCurrentCompanyCurrency() {
        // implement ComputeIsCurrentCompanyCurrency
    }

    public virtual void ComputeCurrentRate() {
        // implement ComputeCurrentRate
    }

    public virtual void ComputeDecimalPlaces() {
        // implement ComputeDecimalPlaces
    }

    public virtual void ComputeDate() {
        // implement ComputeDate
    }

    public virtual string AmountToText(decimal amount) {
        // implement AmountToText
    }

    public virtual string Format(decimal amount) {
        // implement Format
    }

    public virtual decimal Round(decimal amount) {
        // implement Round
    }

    public virtual int CompareAmounts(decimal amount1, decimal amount2) {
        // implement CompareAmounts
    }

    public virtual bool IsZero(decimal amount) {
        // implement IsZero
    }

    public virtual decimal GetConversionRate(BaseCurrency fromCurrency, BaseCurrency toCurrency, ResCompany company = null, DateTime date = null) {
        // implement GetConversionRate
    }

    public virtual decimal Convert(decimal fromAmount, BaseCurrency toCurrency, ResCompany company = null, DateTime date = null, bool round = true) {
        // implement Convert
    }

    public virtual void SelectCompaniesRates() {
        // implement SelectCompaniesRates
    }

    public virtual string GetViewCacheKey(int? viewId = null, string viewType = "form", Dictionary<string, object> options = null) {
        // implement GetViewCacheKey
    }

    public virtual (string, object) GetView(int? viewId = null, string viewType = "form", Dictionary<string, object> options = null) {
        // implement GetView
    }
}

public partial class BaseCurrencyRate {

    public virtual void SanitizeVals(Dictionary<string, object> vals) {
        // implement SanitizeVals
    }

    public virtual void Write(Dictionary<string, object> vals) {
        // implement Write
    }

    public virtual void Create(Dictionary<string, object> valsList) {
        // implement Create
    }

    public virtual BaseCurrencyRate GetLatestRate() {
        // implement GetLatestRate
    }

    public virtual Dictionary<ResCompany, decimal> GetLastRatesForCompanies(List<ResCompany> companies) {
        // implement GetLastRatesForCompanies
    }

    public virtual void ComputeRate() {
        // implement ComputeRate
    }

    public virtual void ComputeCompanyRate() {
        // implement ComputeCompanyRate
    }

    public virtual void InverseCompanyRate() {
        // implement InverseCompanyRate
    }

    public virtual void ComputeInverseCompanyRate() {
        // implement ComputeInverseCompanyRate
    }

    public virtual void InverseInverseCompanyRate() {
        // implement InverseInverseCompanyRate
    }

    public virtual void OnchangeRateWarning() {
        // implement OnchangeRateWarning
    }

    public virtual void CheckCompanyId() {
        // implement CheckCompanyId
    }

    public virtual List<object> NameSearch(string name, List<object> domain = null, string operator = "ilike", int? limit = null, string order = null) {
        // implement NameSearch
    }

    public virtual string GetViewCacheKey(int? viewId = null, string viewType = "form", Dictionary<string, object> options = null) {
        // implement GetViewCacheKey
    }

    public virtual (string, object) GetView(int? viewId = null, string viewType = "form", Dictionary<string, object> options = null) {
        // implement GetView
    }
}
