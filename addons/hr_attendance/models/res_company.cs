csharp
public partial class ResCompany
{
    public string DefaultCompanyToken()
    {
        return Guid.NewGuid().ToString();
    }

    public void ComputeAttendanceKioskUrl()
    {
        AttendanceKioskUrl = Env.UrlJoin(GetBaseUrl(), $"/hr_attendance/{AttendanceKioskKey}");
    }

    public void Write(Dictionary<string, object> vals)
    {
        List<List<object>> searchDomains = new List<List<object>>();
        List<List<object>> deleteDomains = new List<List<object>>();

        bool isDisablingOvertime = false;
        if (vals.ContainsKey("HrAttendanceOvertime") && !(bool)vals["HrAttendanceOvertime"] && HrAttendanceOvertime)
        {
            deleteDomains.Add(new List<object> { new List<object> { "CompanyId", "=", Id } });
            vals["OvertimeStartDate"] = null;
            isDisablingOvertime = true;
        }

        // ... (Implement the rest of the write logic here)

        base.Write(vals);

        if (deleteDomains.Count > 0)
        {
            Env.Get<HrAttendanceOvertime>().Search(deleteDomains).Unlink();
        }

        if (searchDomains.Count > 0)
        {
            Env.Get<HrAttendance>().Search(searchDomains).UpdateOvertime();
        }
    }

    public void RegenerateAttendanceKioskKey()
    {
        Write(new Dictionary<string, object>
        {
            { "AttendanceKioskKey", Guid.NewGuid().ToString("N") }
        });
    }

    public Dictionary<string, object> ActionOpenKioskMode()
    {
        return new Dictionary<string, object>
        {
            { "type", "ir.actions.act_url" },
            { "target", "self" },
            { "url", $"/hr_attendance/kiosk_mode_menu/{Env.Company.Id}" }
        };
    }
}
