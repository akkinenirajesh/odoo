csharp
public partial class Employee
{
    public void ComputeSubordinates()
    {
        // Logic to compute subordinates
        // Use Env to access other models or services if needed
    }

    public void ComputeIsSubordinate()
    {
        // Logic to compute if the employee is a subordinate
    }

    public static IEnumerable<Employee> SearchIsSubordinate(bool isSubordinate)
    {
        // Logic to search for employees based on subordinate status
        // Return an IEnumerable<Employee> based on the search criteria
    }
}

public partial class EmployeePublic
{
    public void ComputeSubordinates()
    {
        // Logic to compute subordinates for public employee
        // Use Env to access other models or services if needed
    }

    public void ComputeIsSubordinate()
    {
        // Logic to compute if the public employee is a subordinate
    }

    public static IEnumerable<EmployeePublic> SearchIsSubordinate(bool isSubordinate)
    {
        // Logic to search for public employees based on subordinate status
        // Return an IEnumerable<EmployeePublic> based on the search criteria
    }
}
