csharp
public partial class LeaveMandatoryDay
{
    public override string ToString()
    {
        return Name;
    }

    public override void OnCreate()
    {
        base.OnCreate();
        if (Color == 0)
        {
            Color = new Random().Next(1, 12);
        }
        if (Company == null)
        {
            Company = Env.Company;
        }
    }

    public override bool OnValidate()
    {
        if (StartDate > EndDate)
        {
            throw new ValidationException("The start date must be anterior than the end date.");
        }
        return base.OnValidate();
    }
}
