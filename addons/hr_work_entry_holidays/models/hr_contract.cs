csharp
using System;
using System.Linq;
using System.Collections.Generic;

namespace HrWorkEntryHolidays
{
    public partial class HrContract
    {
        public WorkEntryType GetLeaveWorkEntryType(ResourceCalendarLeave leave)
        {
            if (leave.HolidayId != null)
            {
                return leave.HolidayId.HolidayStatusId.WorkEntryTypeId;
            }
            else
            {
                return leave.WorkEntryTypeId;
            }
        }

        public List<Tuple<string, int>> GetMoreValsLeaveInterval(Tuple<DateTime, DateTime> interval, List<Tuple<DateTime, DateTime, ResourceCalendarLeave>> leaves)
        {
            var result = base.GetMoreValsLeaveInterval(interval, leaves);
            foreach (var leave in leaves)
            {
                if (interval.Item1 >= leave.Item1 && interval.Item2 <= leave.Item2)
                {
                    result.Add(Tuple.Create("LeaveId", leave.Item3.HolidayId.Id));
                }
            }
            return result;
        }

        public WorkEntryType GetIntervalLeaveWorkEntryType(
            Tuple<DateTime, DateTime, ResourceCalendarLeave> interval, 
            List<Tuple<DateTime, DateTime, ResourceCalendarLeave>> leaves, 
            List<string> bypassingCodes)
        {
            if (interval.Item3.WorkEntryTypeId != null && bypassingCodes.Contains(interval.Item3.WorkEntryTypeId.Code))
            {
                return interval.Item3.WorkEntryTypeId;
            }

            var intervalStart = interval.Item1.ToUniversalTime();
            var intervalStop = interval.Item2.ToUniversalTime();
            var includingRcleaves = leaves
                .Where(l => l.Item3 != null && intervalStart >= l.Item3.DateFrom && intervalStop <= l.Item3.DateTo)
                .Select(l => l.Item3)
                .ToList();
            var includingGlobalRcleaves = includingRcleaves.Where(l => l.HolidayId == null).ToList();
            var includingHolidayRcleaves = includingRcleaves.Where(l => l.HolidayId != null).ToList();
            ResourceCalendarLeave rcLeave = null;

            var bypassingRcLeave = bypassingCodes.Any()
                ? includingHolidayRcleaves.FirstOrDefault(l => bypassingCodes.Contains(l.HolidayId.HolidayStatusId.WorkEntryTypeId.Code))
                : null;

            if (bypassingRcLeave != null)
            {
                rcLeave = bypassingRcLeave;
            }
            else if (includingGlobalRcleaves.Any())
            {
                rcLeave = includingGlobalRcleaves.First();
            }
            else if (includingHolidayRcleaves.Any())
            {
                rcLeave = includingHolidayRcleaves.First();
            }

            if (rcLeave != null)
            {
                return GetLeaveWorkEntryTypeDates(rcLeave, intervalStart, intervalStop, this.EmployeeId);
            }

            return Env.Ref<WorkEntryType>("hr_work_entry_contract.work_entry_type_leave");
        }

        public List<Tuple<string, object>> GetSubLeaveDomain()
        {
            var domain = base.GetSubLeaveDomain();
            return domain.Concat(new List<Tuple<string, object>>
            {
                Tuple.Create("HolidayId.EmployeeId", (object)this.EmployeeId.Id)
            }).ToList();
        }
    }
}
