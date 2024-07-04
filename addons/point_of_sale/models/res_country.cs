C#
public partial class ResCountry
{
    public ResCountry(Env env)
    {
        this.Env = env;
    }

    public Env Env { get; }

    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string VatLabel { get; set; }

    public List<State> States { get; set; }

    public string PhoneCode { get; set; }
    public string ZipFormat { get; set; }
    public string ZipCodeRegexp { get; set; }
    public string CodeFormat { get; set; }
    public string VatLabelFormat { get; set; }
    public string NameFormat { get; set; }
    public string AddressFormat { get; set; }
    public string CompanyFormat { get; set; }
    public string StreetFormat { get; set; }
    public string ZipFormatRegexp { get; set; }
    public string CityFormat { get; set; }
    public string StateFormat { get; set; }
    public string AddressFormatRegexp { get; set; }
    public string PhoneFormat { get; set; }
    public Currency Currency { get; set; }
    public Lang Language { get; set; }
    public CountryGroup CountryGroup { get; set; }
    public string Website { get; set; }

    public List<object> LoadPosDataFields(int configId)
    {
        return new List<object>() { Id, Name, Code, VatLabel };
    }
}
