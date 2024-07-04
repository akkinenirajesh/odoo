csharp
public partial class WebsiteSale.ProductRibbon 
{
    public string GetPositionClass()
    {
        if (this.Position == "left")
        {
            return "o_ribbon_left";
        }
        else
        {
            return "o_ribbon_right";
        }
    }
}
