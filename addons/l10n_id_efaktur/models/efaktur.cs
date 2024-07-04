csharp
public partial class EfakturRange
{
    public int PopNumber(int companyId)
    {
        var range = Env.EfakturRange.Search(r => r.Company.Id == companyId)
            .OrderBy(r => r.Min)
            .FirstOrDefault();

        if (range == null)
            return -1;

        int popped = int.Parse(range.Min);
        if (int.Parse(range.Min) >= int.Parse(range.Max))
        {
            Env.EfakturRange.Remove(range);
        }
        else
        {
            range.Min = $"{popped + 1:D13}";
        }
        return popped;
    }

    public void PushNumber(int companyId, int number)
    {
        PushNumbers(companyId, number, number);
    }

    public void PushNumbers(int companyId, int min, int max)
    {
        var rangeSup = Env.EfakturRange.FirstOrDefault(r => r.Min == $"{max + 1:D13}");
        if (rangeSup != null)
        {
            rangeSup.Min = $"{min:D13}";
            max = int.Parse(rangeSup.Max);
        }

        var rangeLow = Env.EfakturRange.FirstOrDefault(r => r.Max == $"{max - 1:D13}");
        if (rangeLow != null)
        {
            Env.EfakturRange.Remove(rangeSup);
            rangeLow.Max = $"{max:D13}";
        }

        if (rangeSup == null && rangeLow == null)
        {
            Env.EfakturRange.Add(new EfakturRange
            {
                Company = Env.Company.GetById(companyId),
                Max = $"{max:D13}",
                Min = $"{min:D13}"
            });
        }
    }

    public void _ComputeDefault()
    {
        // Implement the logic to compute default values for Min and Max
    }

    public void _ComputeAvailable()
    {
        Available = 1 + int.Parse(Max) - int.Parse(Min);
    }

    public void OnChangeMin()
    {
        Min = $"{int.Parse(Min):D13}";
        if (string.IsNullOrEmpty(Max) || int.Parse(Min) > int.Parse(Max))
        {
            Max = Min;
        }
    }

    public void OnChangeMax()
    {
        Max = $"{int.Parse(Max):D13}";
        if (string.IsNullOrEmpty(Min) || int.Parse(Min) > int.Parse(Max))
        {
            Min = Max;
        }
    }
}
