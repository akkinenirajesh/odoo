csharp
public partial class Employee
{
    public override string ToString()
    {
        return Name;
    }

    public void GenerateRandomBarcode()
    {
        Random random = new Random();
        Barcode = "041" + string.Join("", Enumerable.Range(0, 9).Select(_ => random.Next(10).ToString()));
    }

    public string GetTz()
    {
        return Tz ?? ResourceCalendarId?.Tz ?? CompanyId?.ResourceCalendarId?.Tz ?? "UTC";
    }

    public Dictionary<long, string> GetTzBatch()
    {
        return new Dictionary<long, string> { { Id, GetTz() } };
    }

    public int GetAge(DateTime? targetDate = null)
    {
        if (!Birthday.HasValue)
            return 0;

        targetDate ??= DateTime.Today;
        int age = targetDate.Value.Year - Birthday.Value.Year;
        if (targetDate.Value.Month < Birthday.Value.Month || (targetDate.Value.Month == Birthday.Value.Month && targetDate.Value.Day < Birthday.Value.Day))
            age--;

        return age;
    }

    public List<string> GetPhoneNumberFields()
    {
        return new List<string> { "MobilePhone" };
    }

    public List<string> GetMailPartnerFields(bool introspectFields = false)
    {
        return new List<string> { "UserPartnerId" };
    }
}
