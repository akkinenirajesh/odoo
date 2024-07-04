C#
public partial class L10nPeResCityDistrict {
    public virtual Core.City CityId { get; set; }

    public virtual Core.Country CountryId { get; set; }

    public virtual Core.State StateId { get; set; }

    public List<string> _LoadPosDataFields(Core.Config configId) {
        return new List<string>() { "Name", "CityId", "CountryId", "StateId" };
    }
}
