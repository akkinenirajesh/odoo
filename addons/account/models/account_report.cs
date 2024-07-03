csharp
public partial class AccountReport
{
    public override string ToString()
    {
        return Name + (Country != null ? $" ({Country.Code})" : "");
    }

    public string GetCopiedName()
    {
        string name = Name + " (copy)";
        while (Env.Find<AccountReport>(r => r.Name == name).Any())
        {
            name += " (copy)";
        }
        return name;
    }

    public void OnChangeAvailabilityCondition()
    {
        if (AvailabilityCondition != AccountReportAvailability.Country)
        {
            Country = null;
        }
    }

    public void ValidateRootReportId()
    {
        if (RootReport?.RootReport != null)
        {
            throw new ValidationException("Only a report without a root report of its own can be selected as root report.");
        }
    }

    public void ValidateSectionReportIds()
    {
        if (SectionReports.Any(section => section.SectionReports.Any()))
        {
            throw new ValidationException("The sections defined on a report cannot have sections themselves.");
        }
    }

    public AccountReport Copy(IDictionary<string, object> defaultValues = null)
    {
        var newReport = (AccountReport)MemberwiseClone();
        
        if (defaultValues != null)
        {
            foreach (var kvp in defaultValues)
            {
                newReport.GetType().GetProperty(kvp.Key)?.SetValue(newReport, kvp.Value);
            }
        }

        newReport.Name = GetCopiedName();

        var codeMapping = new Dictionary<string, string>();
        foreach (var line in Lines.Where(l => l.Parent == null))
        {
            line.CopyHierarchy(newReport, codeMapping: codeMapping);
        }

        foreach (var column in Columns)
        {
            column.Copy(new { ReportId = newReport.Id });
        }

        return newReport;
    }
}
