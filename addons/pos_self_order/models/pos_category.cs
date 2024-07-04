csharp
public partial class PosCategory
{
    public virtual float HourUntil { get; set; }

    public virtual float HourAfter { get; set; }

    public virtual void CheckHour()
    {
        if (HourUntil != null && !(0.0 <= HourUntil && HourUntil <= 24.0))
        {
            throw new Exception("The Availability Until must be set between 00:00 and 24:00");
        }

        if (HourAfter != null && !(0.0 <= HourAfter && HourAfter <= 24.0))
        {
            throw new Exception("The Availability After must be set between 00:00 and 24:00");
        }

        if (HourUntil != null && HourAfter != null && HourUntil < HourAfter)
        {
            throw new Exception("The Availability Until must be greater than Availability After.");
        }
    }

    public virtual List<string> LoadPosDataFields(int configId)
    {
        List<string> fields = Env.Model("Pos.PosCategory").Call("LoadPosDataFields", configId).Cast<string>().ToList();
        fields.AddRange(new List<string>() { "HourUntil", "HourAfter" });
        return fields;
    }
}
