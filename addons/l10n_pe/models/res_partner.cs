csharp
public partial class l10n_pe.ResPartner {
    public void OnChangeL10nPeDistrict() {
        if (this.L10nPeDistrict != null) {
            this.CityId = this.L10nPeDistrict.CityId;
        }
    }

    public void OnChangeCityId() {
        if (this.CityId != null && this.L10nPeDistrict != null && this.L10nPeDistrict.CityId != this.CityId) {
            this.L10nPeDistrict = null;
        }
    }
}
