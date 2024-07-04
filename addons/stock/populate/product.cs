C#
public partial class Stock_ProductProduct
{
    public bool IsStorable { get; set; }
    public string Tracking { get; set; }

    public void ComputeIsStorable()
    {
        if (Env.Get<Stock_ProductTemplate>().Get("type") == "consu")
        {
            this.IsStorable = Env.Random().NextDouble() < 0.8;
        }
        else
        {
            this.IsStorable = false;
        }
    }

    public void ComputeTracking()
    {
        if (this.IsStorable)
        {
            double random = Env.Random().NextDouble();
            if (random < 0.7)
            {
                this.Tracking = "none";
            }
            else if (random < 0.9)
            {
                this.Tracking = "lot";
            }
            else
            {
                this.Tracking = "serial";
            }
        }
        else
        {
            this.Tracking = "none";
        }
    }
}
