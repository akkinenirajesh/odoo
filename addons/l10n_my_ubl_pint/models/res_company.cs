csharp
public partial class ResCompany {
    public string SSTRegistrationNumber { get; set; }
    public string TTXRegistrationNumber { get; set; }

    public override string ToString() {
        // Logic to compute the string representation of the object
        return SSTRegistrationNumber;
    }
}

public partial class BaseDocumentLayout {
    public Core.Country AccountFiscalCountryId { get; set; }
    public string SSTRegistrationNumber { get; set; }
    public string TTXRegistrationNumber { get; set; }

    public override string ToString() {
        // Logic to compute the string representation of the object
        return SSTRegistrationNumber;
    }
}
