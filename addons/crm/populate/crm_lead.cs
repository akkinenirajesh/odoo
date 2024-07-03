csharp
public partial class Lead
{
    public override string ToString()
    {
        return Name;
    }

    public void Populate()
    {
        var random = new Random();

        // Populate Partner
        Partner = random.Next(3) == 0 ? null : Env.Registry.PopulatedModels["Core.Partner"].GetRandom();

        // Populate address fields if Partner is null
        if (Partner == null)
        {
            PopulateAddress(random);
        }

        // Populate contact information if Partner is null
        if (Partner == null)
        {
            PopulateContact(random);
        }

        // Populate User
        User = random.Next(2) == 0 ? null : Env.Registry.PopulatedModels["Core.User"].GetRandom();

        // Populate DateOpen
        if (User != null)
        {
            DateOpen = DateTime.Now.AddDays(-random.Next(11));
        }

        // Populate Name
        Name = GenerateName(random);

        // Populate Type
        Type = random.NextDouble() < 0.8 ? LeadType.Lead : LeadType.Opportunity;
    }

    private void PopulateAddress(Random random)
    {
        // Implement address population logic here
        // Use the address generators logic from the original Python code
    }

    private void PopulateContact(Random random)
    {
        // Implement contact information population logic here
        // Use the contact generators logic from the original Python code
    }

    private string GenerateName(Random random)
    {
        // Implement name generation logic here
        // Use the name generation logic from the original Python code
        return $"Generated Lead Name {random.Next(1000)}";
    }
}
