csharp
public partial class Attendance
{
    public override string ToString()
    {
        if (CheckOut == null)
        {
            return $"From {CheckIn:HH:mm}";
        }
        else
        {
            return $"{WorkedHours:hh\\:mm} ({CheckIn:HH:mm}-{CheckOut:HH:mm})";
        }
    }

    public void ComputeColor()
    {
        if (CheckOut != null)
        {
            Color = WorkedHours > 16 ? 1 : 0;
        }
        else
        {
            Color = CheckIn < DateTime.Today.AddDays(-1) ? 1 : 10;
        }
    }

    public void ComputeOvertimeHours()
    {
        // This method would need to be implemented based on the complex logic in the original Python code
        // It would involve querying other records and performing calculations
    }

    public void ComputeWorkedHours()
    {
        if (CheckOut != null && CheckIn != null && Employee != null)
        {
            var calendar = GetEmployeeCalendar();
            var resource = Employee.Resource;
            var tz = TimeZoneInfo.FindSystemTimeZoneById(calendar.Tz);
            var checkInTz = TimeZoneInfo.ConvertTimeToUtc(CheckIn.Value, tz);
            var checkOutTz = TimeZoneInfo.ConvertTimeToUtc(CheckOut.Value, tz);

            // The rest of the calculation would need to be implemented
            // This would involve querying lunch intervals and performing time calculations
        }
        else
        {
            WorkedHours = null;
        }
    }

    public void CheckValidityCheckInCheckOut()
    {
        if (CheckIn != null && CheckOut != null)
        {
            if (CheckOut < CheckIn)
            {
                throw new ValidationException("\"Check Out\" time cannot be earlier than \"Check In\" time.");
            }
        }
    }

    public void CheckValidity()
    {
        // This method would need to be implemented to check the validity of the attendance record
        // It would involve querying other attendance records and performing checks
    }

    public string ActionInAttendanceMaps()
    {
        return GetGoogleMapsUrl(InLatitude, InLongitude);
    }

    public string ActionOutAttendanceMaps()
    {
        return GetGoogleMapsUrl(OutLatitude, OutLongitude);
    }

    private string GetGoogleMapsUrl(float latitude, float longitude)
    {
        return $"https://maps.google.com?q={latitude},{longitude}";
    }

    // Other methods would need to be implemented similarly
}
