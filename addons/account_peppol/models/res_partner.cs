csharp
public partial class ResPartner
{
    public override string ToString()
    {
        // Assuming Name is a property of ResPartner
        return Name;
    }

    public ParticipantInfo GetParticipantInfo(string ediIdentification)
    {
        // Implementation of _get_participant_info method
        // This is a simplified version and may need adjustment
        var hashParticipant = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(ediIdentification.ToLower()));
        var endpointParticipant = Uri.EscapeDataString($"iso6523-actorid-upis::{ediIdentification}");
        var peppolUser = Env.Company.AccountEdiProxyClientIds.FirstOrDefault(user => user.ProxyType == "peppol");
        var ediMode = peppolUser?.EdiMode ?? Env.GetParameter<string>("account_peppol.edi.mode");
        var smlZone = ediMode == "test" ? "acc.edelivery" : "edelivery";
        var smpUrl = $"http://B-{BitConverter.ToString(hashParticipant).Replace("-", "").ToLower()}.iso6523-actorid-upis.{smlZone}.tech.ec.europa.eu/{endpointParticipant}";

        // Implement the HTTP request and XML parsing here
        // Return the parsed ParticipantInfo or null if not found
        throw new NotImplementedException();
    }

    public bool CheckPeppolParticipantExists(string ediIdentification, bool checkCompany = false, string ublCiiFormat = null)
    {
        var participantInfo = GetParticipantInfo(ediIdentification);
        if (participantInfo == null)
            return false;

        // Implement the rest of the logic here
        throw new NotImplementedException();
    }

    public void ButtonAccountPeppolCheckPartnerEndpoint()
    {
        if (string.IsNullOrEmpty(PeppolEas) || string.IsNullOrEmpty(PeppolEndpoint) || !IsPeppolEdiFormat)
        {
            AccountPeppolIsEndpointValid = false;
        }
        else
        {
            var ediIdentification = $"{PeppolEas}:{PeppolEndpoint}".ToLower();
            AccountPeppolValidityLastCheck = DateTime.Today;
            AccountPeppolIsEndpointValid = CheckPeppolParticipantExists(ediIdentification, ublCiiFormat: UblCiiFormat);
        }
    }

    // Implement other methods as needed
}
