csharp
public partial class User
{
    private static readonly string[] DAYS = new string[] 
    {
        "MondayLocationId", "TuesdayLocationId", "WednesdayLocationId", 
        "ThursdayLocationId", "FridayLocationId", "SaturdayLocationId", "SundayLocationId"
    };

    public List<string> GetEmployeeFieldsToSync()
    {
        var baseFields = base.GetEmployeeFieldsToSync();
        baseFields.AddRange(DAYS);
        return baseFields;
    }

    public List<string> SelfReadableFields
    {
        get
        {
            var baseFields = base.SelfReadableFields;
            baseFields.AddRange(DAYS);
            return baseFields;
        }
    }

    public List<string> SelfWriteableFields
    {
        get
        {
            var baseFields = base.SelfWriteableFields;
            baseFields.AddRange(DAYS);
            return baseFields;
        }
    }

    public void ComputeImStatus()
    {
        base.ComputeImStatus();
        
        var hrEmployee = Env.Get<HrHomeworking.Employee>();
        string dayField = hrEmployee.GetCurrentDayLocationField();
        
        var location = this.GetType().GetProperty(dayField).GetValue(this) as HrHomeworking.WorkLocation;
        if (location == null || string.IsNullOrEmpty(location.LocationType))
            return;

        if (ImStatus == "online" || ImStatus == "away" || ImStatus == "offline")
        {
            ImStatus = $"presence_{location.LocationType}_{ImStatus}";
        }
    }
}
