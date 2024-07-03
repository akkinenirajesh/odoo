csharp
public partial class Partner
{
    public void InverseStreetData()
    {
        string street = ((this.StreetName ?? "") + " " + (this.StreetNumber ?? "")).Trim();
        if (!string.IsNullOrEmpty(this.StreetNumber2))
        {
            street = street + " - " + this.StreetNumber2;
        }
        this.Street = street;
    }

    public void ComputeStreetData()
    {
        var splitStreet = Env.Tools.StreetSplit(this.Street);
        this.StreetName = splitStreet.StreetName;
        this.StreetNumber = splitStreet.StreetNumber;
        this.StreetNumber2 = splitStreet.StreetNumber2;
    }

    public Dictionary<string, string> GetStreetSplit()
    {
        return new Dictionary<string, string>
        {
            { "StreetName", this.StreetName },
            { "StreetNumber", this.StreetNumber },
            { "StreetNumber2", this.StreetNumber2 }
        };
    }

    public void OnChangeCityId()
    {
        if (this.CityId != null)
        {
            this.City = this.CityId.Name;
            this.Zip = this.CityId.Zipcode;
            this.StateId = this.CityId.StateId;
        }
        else if (this.Id != 0) // Assuming Id 0 means new record
        {
            this.City = null;
            this.Zip = null;
            this.StateId = null;
        }
    }
}
