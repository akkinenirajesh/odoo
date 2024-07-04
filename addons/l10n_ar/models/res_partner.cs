csharp
public partial class ResPartner
{
    public string ComputeL10nArVat()
    {
        if (L10nLatamIdentificationType?.L10nArAfipCode == "80" && !string.IsNullOrEmpty(Vat))
        {
            return StdNum.Ar.Cuit.Compact(Vat);
        }
        return null;
    }

    public string ComputeL10nArFormattedVat()
    {
        if (!string.IsNullOrEmpty(L10nArVat))
        {
            try
            {
                return StdNum.Ar.Cuit.Format(L10nArVat);
            }
            catch (Exception error)
            {
                Env.Logger.LogWarning($"Argentinean VAT was not formatted: {error}");
                return L10nArVat;
            }
        }
        return null;
    }

    public void CheckVat()
    {
        if (L10nLatamIdentificationType?.L10nArAfipCode != null || Country?.Code == "AR")
        {
            L10nArIdentificationValidation();
        }
        else
        {
            base.CheckVat();
        }
    }

    public string EnsureVat()
    {
        if (string.IsNullOrEmpty(L10nArVat))
        {
            throw new UserException($"No VAT configured for partner [{Id}] {Name}");
        }
        return L10nArVat;
    }

    public void L10nArIdentificationValidation()
    {
        if (string.IsNullOrEmpty(Vat))
        {
            return;
        }

        IModule module = null;
        try
        {
            module = GetValidationModule();
        }
        catch (Exception error)
        {
            Env.Logger.LogWarning($"Argentinean document was not validated: {error}");
        }

        if (module == null)
        {
            return;
        }

        try
        {
            module.Validate(Vat);
        }
        catch (InvalidChecksumException)
        {
            throw new ValidationException($"The validation digit is not valid for \"{L10nLatamIdentificationType?.Name}\"");
        }
        catch (InvalidLengthException)
        {
            throw new ValidationException($"Invalid length for \"{L10nLatamIdentificationType?.Name}\"");
        }
        catch (InvalidFormatException)
        {
            throw new ValidationException($"Only numbers allowed for \"{L10nLatamIdentificationType?.Name}\"");
        }
        catch (InvalidComponentException)
        {
            var validCuit = new[] { "20", "23", "24", "27", "30", "33", "34", "50", "51", "55" };
            throw new ValidationException($"CUIT number must be prefixed with one of the following: {string.Join(", ", validCuit)}");
        }
        catch (Exception error)
        {
            throw new ValidationException(error.ToString());
        }
    }

    public int GetIdNumberSanitize()
    {
        if (string.IsNullOrEmpty(Vat))
        {
            return 0;
        }

        if (L10nLatamIdentificationType?.L10nArAfipCode == "80" || L10nLatamIdentificationType?.L10nArAfipCode == "86")
        {
            return int.Parse(StdNum.Ar.Cuit.Compact(Vat));
        }
        else
        {
            var idNumber = System.Text.RegularExpressions.Regex.Replace(Vat, "[^0-9]", "");
            return int.Parse(idNumber);
        }
    }

    private IModule GetValidationModule()
    {
        if (L10nLatamIdentificationType?.L10nArAfipCode == "80" || L10nLatamIdentificationType?.L10nArAfipCode == "86")
        {
            return StdNum.Ar.Cuit;
        }
        else if (L10nLatamIdentificationType?.L10nArAfipCode == "96")
        {
            return StdNum.Ar.Dni;
        }
        return null;
    }
}
