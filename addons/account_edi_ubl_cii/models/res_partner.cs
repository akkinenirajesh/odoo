csharp
public partial class ResPartner
{
    public override string ToString()
    {
        // Assuming there's a Name field in the partner model
        return Name;
    }

    public void CheckPeppolFields()
    {
        if (!string.IsNullOrEmpty(PeppolEndpoint) && PeppolEas != null)
        {
            string error = BuildErrorPeppolEndpoint(PeppolEas, PeppolEndpoint);
            if (!string.IsNullOrEmpty(error))
            {
                throw new ValidationException(error);
            }
        }
    }

    public void ComputeUblCiiFormat()
    {
        string countryCode = DeduceCountryCode();
        var formatMapping = GetUblCiiFormats();

        if (formatMapping.ContainsKey(countryCode))
        {
            UblCiiFormat = formatMapping[countryCode];
        }
        else if (EasMapping.ContainsKey(countryCode))
        {
            UblCiiFormat = UblCiiFormat.UblBis3;
        }
        // else: keep the existing value
    }

    public void ComputePeppolEndpoint()
    {
        string countryCode = DeduceCountryCode();
        if (EasMapping.ContainsKey(countryCode))
        {
            var fieldName = EasMapping[countryCode].GetValueOrDefault(PeppolEas);
            if (!string.IsNullOrEmpty(fieldName) && 
                GetType().GetProperty(fieldName) != null)
            {
                var value = GetType().GetProperty(fieldName).GetValue(this) as string;
                if (!string.IsNullOrEmpty(value) && 
                    string.IsNullOrEmpty(BuildErrorPeppolEndpoint(PeppolEas, value)))
                {
                    PeppolEndpoint = value;
                }
            }
        }
    }

    public void ComputePeppolEas()
    {
        string countryCode = DeduceCountryCode();
        if (EasMapping.ContainsKey(countryCode))
        {
            var easToField = EasMapping[countryCode];
            if (!easToField.ContainsKey(PeppolEas))
            {
                PeppolEas newEas = easToField.Keys.FirstOrDefault();
                foreach (var kvp in easToField)
                {
                    var fieldName = kvp.Value;
                    if (!string.IsNullOrEmpty(fieldName) && 
                        GetType().GetProperty(fieldName) != null)
                    {
                        var value = GetType().GetProperty(fieldName).GetValue(this) as string;
                        if (!string.IsNullOrEmpty(value) && 
                            string.IsNullOrEmpty(BuildErrorPeppolEndpoint(kvp.Key, value)))
                        {
                            newEas = kvp.Key;
                            break;
                        }
                    }
                }
                PeppolEas = newEas;
            }
        }
    }

    private string BuildErrorPeppolEndpoint(PeppolEas eas, string endpoint)
    {
        if (eas == PeppolEas.Value0208 && !Regex.IsMatch(endpoint, @"^\d{10}$"))
        {
            return "The Peppol endpoint is not valid. The expected format is: 0239843188";
        }
        if (eas == PeppolEas.Value0009 && !IsSiretValid(endpoint))
        {
            return "The Peppol endpoint is not valid. The expected format is: 73282932000074";
        }
        return null;
    }

    private bool IsSiretValid(string siret)
    {
        // Implement SIRET validation logic here
        throw new NotImplementedException();
    }

    private string DeduceCountryCode()
    {
        // Implement country code deduction logic here
        throw new NotImplementedException();
    }

    private Dictionary<string, UblCiiFormat> GetUblCiiFormats()
    {
        return new Dictionary<string, UblCiiFormat>
        {
            { "DE", UblCiiFormat.Xrechnung },
            { "AU", UblCiiFormat.UblANz },
            { "NZ", UblCiiFormat.UblANz },
            { "NL", UblCiiFormat.Nlcius },
            { "FR", UblCiiFormat.Facturx },
            { "SG", UblCiiFormat.UblSg },
        };
    }

    // EasMapping and other necessary methods/properties should be implemented
}
