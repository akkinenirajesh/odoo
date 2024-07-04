csharp
public partial class ResPartner
{
    private const string TEST_GST_NUMBER = "36AABCT1332L011";

    public bool DisplayPanWarning
    {
        get
        {
            return Vat != null && L10nInPan != null && L10nInPan != Vat.Substring(2, 10);
        }
    }

    public void OnChangeCompanyType()
    {
        if (Country?.Code == "IN")
        {
            L10nInGstTreatment = CompanyType == CompanyType.Company ? GstTreatment.Regular : GstTreatment.Consumer;
        }
    }

    public void OnChangeCountry()
    {
        if (Country?.Code != "IN")
        {
            L10nInGstTreatment = GstTreatment.Overseas;
        }
        else if (Country?.Code == "IN")
        {
            L10nInGstTreatment = CompanyType == CompanyType.Company ? GstTreatment.Regular : GstTreatment.Consumer;
        }
    }

    public void OnChangeVat()
    {
        if (Vat != null && CheckVatIn(Vat))
        {
            State = Env.States.FirstOrDefault(s => s.L10nInTin == Vat.Substring(0, 2));
            if (char.IsLetter(Vat[2]))
            {
                L10nInPan = Vat.Substring(2, 10);
            }
        }
    }

    public bool CheckVatIn(string vat)
    {
        if (vat == TEST_GST_NUMBER)
        {
            return true;
        }
        // Implement the regular VAT check logic here
        return false;
    }
}
