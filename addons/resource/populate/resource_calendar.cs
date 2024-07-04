C#
public partial class ResourceCalendar {
    public void Populate(int size) {
        var records = base.Populate(size);
        var random = Env.Random.Get("calendar");
        var aLot = records.FilterDomain(new[] { new Tuple<string, object>("Name", $"like 'A lot'") });
        foreach (var record in aLot) {
            var attId = record.AttendanceIds[random.RandInt(0, 9)];
            record.Write(new[] { new Tuple<string, object>("AttendanceIds", new[] { new Tuple<int, int>(3, attId.Id) }) });
        }
        var aLittle = records.Except(aLot);
        foreach (var record in aLittle) {
            var toPop = random.Sample(Enumerable.Range(0, 10), random.RandInt(3, 5));
            record.Write(new[] { new Tuple<string, object>("AttendanceIds", toPop.Select(idx => new Tuple<int, int>(3, record.AttendanceIds[idx].Id))) });
        }
    }
}
