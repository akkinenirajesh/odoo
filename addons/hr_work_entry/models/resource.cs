csharp
public partial class ResourceCalendarAttendance
{
    public Hr.WorkEntryType DefaultWorkEntryType()
    {
        return Env.Ref("hr_work_entry.work_entry_type_attendance", raiseIfNotFound: false);
    }

    public Dictionary<string, object> CopyAttendanceVals()
    {
        var res = base.CopyAttendanceVals();
        res["WorkEntryType"] = this.WorkEntryType?.Id;
        return res;
    }
}

public partial class ResourceCalendarLeave
{
    public Dictionary<string, object> CopyLeaveVals()
    {
        var res = base.CopyLeaveVals();
        res["WorkEntryType"] = this.WorkEntryType?.Id;
        return res;
    }
}
