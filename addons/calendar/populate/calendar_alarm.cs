csharp
public partial class Alarm
{
    public override string ToString()
    {
        return $"{AlarmType.ToString().Capitalize()} - {Duration} {Interval} (#{Id}).";
    }

    public void Populate(PopulateSize size)
    {
        int count = size switch
        {
            PopulateSize.Small => 3,
            PopulateSize.Medium => 10,
            PopulateSize.Large => 30,
            _ => throw new ArgumentOutOfRangeException(nameof(size))
        };

        var alarmTypes = Env.GetOptionSet<Calendar.AlarmType>();
        var intervals = Env.GetOptionSet<Calendar.AlarmInterval>();

        for (int i = 0; i < count; i++)
        {
            var alarm = Env.New<Calendar.Alarm>();
            alarm.AlarmType = alarmTypes.GetRandom();
            alarm.Duration = Env.Random.Next(1, 60);
            alarm.Interval = intervals.GetRandom();
            alarm.Name = alarm.ToString();
            Env.Add(alarm);
        }
    }
}

public enum PopulateSize
{
    Small,
    Medium,
    Large
}
