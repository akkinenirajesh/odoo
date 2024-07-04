csharp
public partial class User
{
    public override string ToString()
    {
        // Assuming there's a Name field in the base User class
        return Name;
    }

    public IEnumerable<string> SelfReadableFields()
    {
        var baseFields = base.SelfReadableFields();
        return baseFields.Concat(new[]
        {
            nameof(ResumeLine),
            nameof(EmployeeSkill)
        });
    }

    public IEnumerable<string> SelfWriteableFields()
    {
        var baseFields = base.SelfWriteableFields();
        return baseFields.Concat(new[]
        {
            nameof(ResumeLine),
            nameof(EmployeeSkill)
        });
    }
}
