csharp
public partial class ResCompany
{
    public override string ToString()
    {
        return Name;
    }

    public string SanitizePeppolPhoneNumber(string phoneNumber = null)
    {
        string errorMessage = "Please enter the phone number in the correct international format.\n" +
            "For example: +32123456789, where +32 is the country code.\n" +
            "Currently, only European countries are supported.";

        phoneNumber = phoneNumber ?? AccountPeppolPhoneNumber;
        if (string.IsNullOrEmpty(phoneNumber))
            return null;

        if (!phoneNumber.StartsWith("+"))
            phoneNumber = "+" + phoneNumber;

        // Implement phone number validation logic here
        // You might want to use a third-party library for phone number validation

        return phoneNumber;
    }

    public bool CheckPeppolEndpointNumber(bool warning = false)
    {
        // Implement the logic to check PEPPOL endpoint number
        // You might need to convert the Python dictionaries to C# dictionaries or use a different approach

        return true;
    }

    public void ComputePeppolPurchaseJournal()
    {
        if (PeppolPurchaseJournal == null && AccountPeppolProxyState != PeppolProxyState.NotRegistered && AccountPeppolProxyState != PeppolProxyState.Rejected)
        {
            PeppolPurchaseJournal = Env.Set<AccountJournal>()
                .Search(j => j.Company == this && j.Type == "Purchase")
                .FirstOrDefault();

            if (PeppolPurchaseJournal != null)
                PeppolPurchaseJournal.IsPeppolJournal = true;
        }
    }

    public void InversePeppolPurchaseJournal()
    {
        var journalsToReset = Env.Set<AccountJournal>()
            .Search(j => j.Company == this && j.IsPeppolJournal);

        foreach (var journal in journalsToReset)
            journal.IsPeppolJournal = false;

        if (PeppolPurchaseJournal != null)
            PeppolPurchaseJournal.IsPeppolJournal = true;
    }

    public Dictionary<string, Dictionary<string, string>> PeppolModulesDocumentTypes()
    {
        // Implement the logic to return supported document types
        // You might need to adjust this based on how you want to handle module-specific document types in C#

        return new Dictionary<string, Dictionary<string, string>>
        {
            ["default"] = new Dictionary<string, string>
            {
                ["urn:oasis:names:specification:ubl:schema:xsd:Invoice-2::Invoice##urn:cen.eu:en16931:2017#compliant#urn:fdc:peppol.eu:2017:poacc:billing:3.0::2.1"] = "Peppol BIS Billing UBL Invoice V3",
                // Add other document types here
            }
        };
    }

    public Dictionary<string, string> PeppolSupportedDocumentTypes()
    {
        var result = new Dictionary<string, string>();
        foreach (var module in PeppolModulesDocumentTypes())
        {
            foreach (var item in module.Value)
            {
                result[item.Key] = item.Value;
            }
        }
        return result;
    }
}
