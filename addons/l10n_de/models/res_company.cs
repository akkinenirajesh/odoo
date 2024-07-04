csharp
public partial class ResCompany
{
    public string GetL10nDeStnrNational()
    {
        string nationalSteuerNummer = null;

        if (!string.IsNullOrEmpty(L10nDeStnr) && CountryCode == "DE")
        {
            try
            {
                // Note: You'll need to implement or find an equivalent for the stdnum.de.stnr functions
                nationalSteuerNummer = StdNum.De.Stnr.ToCountryNumber(L10nDeStnr, State.Name);
            }
            catch (InvalidComponentException)
            {
                throw new ValidationException("Your company's SteuerNummer is not compatible with your state");
            }
            catch (InvalidFormatException)
            {
                if (StdNum.De.Stnr.IsValid(L10nDeStnr, State.Name))
                {
                    nationalSteuerNummer = L10nDeStnr;
                }
                else
                {
                    throw new ValidationException("Your company's SteuerNummer is not valid");
                }
            }
        }
        else if (!string.IsNullOrEmpty(L10nDeStnr))
        {
            nationalSteuerNummer = L10nDeStnr;
        }

        return nationalSteuerNummer;
    }

    public void ValidateL10nDeStnr()
    {
        GetL10nDeStnrNational();
    }
}
