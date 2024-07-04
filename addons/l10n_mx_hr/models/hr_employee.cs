csharp
public partial class Employee
{
    public override string ToString()
    {
        // You might want to return a meaningful string representation of the Employee
        // For example, you could use the employee's name if it's available
        return Env.GetFieldValue<string>(this, "Name") ?? base.ToString();
    }

    // You can add any additional methods or properties specific to the Employee model here
}
