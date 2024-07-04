csharp
public partial class Base_ResBank 
{
    public string Name { get; set; }
    public string Street { get; set; }
    public string Street2 { get; set; }
    public string Zip { get; set; }
    public string City { get; set; }
    public Core_CountryState State { get; set; }
    public Core_Country Country { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public bool Active { get; set; }
    public string Bic { get; set; }

    public void ComputeDisplayName()
    {
        this.DisplayName = (this.Name ?? "") + (this.Bic != null ? (" - " + this.Bic) : "");
    }

    public Base_ResBank NameSearch(string name, List<object> domain = null, string operator = "ilike", int? limit = null, string order = null)
    {
        domain = domain ?? new List<object>();
        if (!string.IsNullOrEmpty(name))
        {
            List<object> nameDomain = new List<object> { "OR", new List<object> { "Bic", "=", name + "%" }, new List<object> { "Name", operator, name } };
            if (operator.StartsWith("!"))
            {
                nameDomain = new List<object> { "AND", "NOT" }.Concat(nameDomain.Skip(1)).ToList();
            }
            domain.AddRange(nameDomain);
        }

        return Env.Model<Base_ResBank>().Search(domain, limit, order);
    }

    public void OnChangeCountry()
    {
        if (this.Country != null && this.Country != this.State.Country)
        {
            this.State = null;
        }
    }

    public void OnChangeState()
    {
        if (this.State.Country != null)
        {
            this.Country = this.State.Country;
        }
    }
}

public partial class Base_ResPartnerBank
{
    public bool Active { get; set; }
    public Base_ResPartnerBankAccountType AccType { get; set; }
    public string AccNumber { get; set; }
    public string SanitizedAccNumber { get; set; }
    public string AccHolderName { get; set; }
    public Base_ResPartner Partner { get; set; }
    public bool AllowOutPayment { get; set; }
    public Base_ResBank Bank { get; set; }
    public string BankName { get; set; }
    public string BankBic { get; set; }
    public int Sequence { get; set; }
    public Core_Currency Currency { get; set; }
    public Base_ResCompany Company { get; set; }
    public string CountryCode { get; set; }

    public void ComputeSanitizedAccNumber()
    {
        this.SanitizedAccNumber = SanitizeAccountNumber(this.AccNumber);
    }

    public void ComputeAccType()
    {
        this.AccType = RetrieveAccType(this.AccNumber);
    }

    public void ComputeAccountHolderName()
    {
        this.AccHolderName = this.Partner.Name;
    }

    public Base_ResPartnerBankAccountType RetrieveAccType(string accNumber)
    {
        return Base_ResPartnerBankAccountType.Bank;
    }

    public void ComputeDisplayName()
    {
        this.DisplayName = this.Bank != null ? $"{this.AccNumber} - {this.Bank.Name}" : this.AccNumber;
    }

    public Base_ResPartnerBank Search(List<object> domain = null, int? offset = null, int? limit = null, string order = null)
    {
        domain = domain ?? new List<object>();
        for (int i = 0; i < domain.Count; i++)
        {
            if (domain[i] is List<object> && (string)domain[i][0] == "AccNumber")
            {
                object value = domain[i][2];
                if (value is string)
                {
                    domain[i][2] = SanitizeAccountNumber((string)value);
                }
                else if (value is IEnumerable<string>)
                {
                    domain[i][2] = ((IEnumerable<string>)value).Select(SanitizeAccountNumber).ToList();
                }
            }
        }
        return Env.Model<Base_ResPartnerBank>().Search(domain, offset, limit, order);
    }

    private string SanitizeAccountNumber(string accNumber)
    {
        return accNumber != null ? System.Text.RegularExpressions.Regex.Replace(accNumber, @"\W+", "").ToUpper() : null;
    }
}
