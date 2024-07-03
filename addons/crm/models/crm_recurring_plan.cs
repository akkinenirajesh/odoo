csharp
public partial class RecurringPlan
{
    public override string ToString()
    {
        return Name;
    }

    public override bool OnBeforeCreate()
    {
        if (NumberOfMonths < 0)
        {
            Env.AddError("NumberOfMonths", "The number of months can't be negative.");
            return false;
        }
        return base.OnBeforeCreate();
    }

    public override bool OnBeforeUpdate()
    {
        if (NumberOfMonths < 0)
        {
            Env.AddError("NumberOfMonths", "The number of months can't be negative.");
            return false;
        }
        return base.OnBeforeUpdate();
    }
}
