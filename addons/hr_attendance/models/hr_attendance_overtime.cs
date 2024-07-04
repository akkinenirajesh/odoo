csharp
public partial class AttendanceOvertime
{
    public override string ToString()
    {
        return Employee.ToString();
    }

    public static HR.Employee DefaultEmployee()
    {
        return Env.User.Employee;
    }

    public void Init()
    {
        // Note: This method would typically be handled by the framework or database layer
        // in a C# environment. The following is a conceptual representation:
        
        // Env.Database.ExecuteNonQuery(@"
        //     CREATE UNIQUE INDEX IF NOT EXISTS hr_attendance_overtime_unique_employee_per_day
        //     ON AttendanceOvertime (Employee, Date)
        //     WHERE Adjustment = false");
    }
}
