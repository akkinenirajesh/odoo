C#
public partial class TestPerformanceBaseModel 
{
    public void ValuePc() 
    {
        this.ValuePc = (float)this.Value / 100;
    }

    public void ComputedValue() 
    {
        this.ComputedValue = (float)this.Value / 100;
    }

    public void IndirectComputedValue() 
    {
        this.IndirectComputedValue = this.ComputedValue / 100;
    }

    public void ValueCtx() 
    {
        Env.Execute("SELECT 42");
        this.ValueCtx = Env.Context.Get("key");
    }

    public void Total() 
    {
        this.Total = this.LineIds.Sum(line => line.Value);
    }
}

public partial class TestPerformanceLineModel
{
    public void Init() 
    {
        Env.CreateUniqueIndex("test_performance_line_uniq", "test_performance_line", new string[] { "BaseId", "Value" });
    }
}

public partial class TestPerformanceMozzarella 
{
    public void ValuePlusOne() 
    {
        this.ValuePlusOne = this.Value + 1;
    }
}
