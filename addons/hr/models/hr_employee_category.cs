csharp
public partial class EmployeeCategory
{
    public override string ToString()
    {
        return Name;
    }

    public int GetDefaultColor()
    {
        return new Random().Next(1, 12); // Generates a random number between 1 and 11
    }
}
