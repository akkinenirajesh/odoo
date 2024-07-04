csharp
public partial class User
{
    public override string ToString()
    {
        // Implement string representation logic here
        return base.ToString();
    }

    public IEnumerable<string> GetSelfReadableFields()
    {
        var baseFields = base.GetSelfReadableFields();
        return baseFields.Concat(new[] { "Vehicle", "BankAccount" });
    }
}
