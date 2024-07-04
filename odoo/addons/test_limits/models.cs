csharp
public partial class TestLimitsModel
{
    public bool ConsumeNothing()
    {
        return true;
    }

    public bool ConsumeMemory(int size)
    {
        var l = new int[size];
        return true;
    }

    public bool LeakMemory(int size)
    {
        if (!Env.HasAttribute("l"))
        {
            Env.Registry["TestLimits.Model"].l = new List<int[]>();
        }
        Env.Registry["TestLimits.Model"].l.Add(new int[size]);
        return true;
    }

    public bool ConsumeTime(int seconds)
    {
        System.Threading.Thread.Sleep(seconds * 1000);
        return true;
    }

    public bool ConsumeCpuTime(int seconds)
    {
        var t0 = DateTime.Now;
        var t1 = DateTime.Now;
        while (t1 - t0 < TimeSpan.FromSeconds(seconds))
        {
            for (var i = 0; i < 10000000; i++)
            {
                var x = i * i;
            }
            t1 = DateTime.Now;
        }
        return true;
    }
}
