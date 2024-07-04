csharp
public partial class HrEmployeePublic
{
    public override string ToString()
    {
        // Assuming there's a Name property in the base class or in another partial definition
        return Name;
    }

    // You can add any additional methods or properties here
    public float CalculateOvertimeHours()
    {
        // Example method to calculate overtime hours
        // This is just a placeholder and should be implemented according to your business logic
        return TotalOvertime;
    }

    public void CheckIn()
    {
        // Example method to check in an employee
        // This is just a placeholder and should be implemented according to your business logic
        LastCheckIn = DateTime.Now;
        AttendanceState = AttendanceState.Checked_In;
        // You might want to create a new HrAttendance record here
        // Env.Create<HrAttendance.HrAttendance>(new { Employee = this, CheckIn = LastCheckIn });
    }

    public void CheckOut()
    {
        // Example method to check out an employee
        // This is just a placeholder and should be implemented according to your business logic
        LastCheckOut = DateTime.Now;
        AttendanceState = AttendanceState.Checked_Out;
        // You might want to update the last HrAttendance record here
        // var lastAttendance = Env.Search<HrAttendance.HrAttendance>().OrderByDescending(a => a.CheckIn).FirstOrDefault();
        // if (lastAttendance != null) {
        //     lastAttendance.CheckOut = LastCheckOut;
        //     lastAttendance.Save();
        // }
    }
}
