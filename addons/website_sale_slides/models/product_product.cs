csharp
public partial class WebsiteSaleSlides.Product {

    public string GetProductMultilineDescriptionSale()
    {
        var paymentChannels = this.ChannelIds.Where(course => course.Enroll == "payment").ToList();
        if (paymentChannels.Count == 0)
        {
            return base.GetProductMultilineDescriptionSale();
        }

        string newLine = paymentChannels.Count == 1 ? "" : "\n";
        return $"Access to: {newLine}{string.Join("\n", paymentChannels.Select(channel => channel.Name))}";
    }
}
