csharp
public partial class IdentificationType
{
    public IEnumerable<IdentificationType> LoadPosData(int configId)
    {
        if (Env.Company.Country?.Code == "AR")
        {
            return Env.Set<IdentificationType>()
                .Where(t => !string.IsNullOrEmpty(t.L10nArAfipCode) && t.Active)
                .Select(t => new IdentificationType { Name = t.Name });
        }
        else
        {
            // Call the base implementation
            return base.LoadPosData(configId);
        }
    }

    public override string ToString()
    {
        return Name;
    }
}
