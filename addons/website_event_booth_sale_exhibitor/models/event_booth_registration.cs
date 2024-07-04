C#
public partial class WebsiteEventBoothSaleExhibitor.EventBoothRegistration 
{
    public List<WebsiteEventBoothSaleExhibitor.EventBoothRegistration> _get_fields_for_booth_confirmation()
    {
        var result = Env.Ref<WebsiteEventBoothSaleExhibitor.EventBoothRegistration>("WebsiteEventBoothSaleExhibitor.EventBoothRegistration")._get_fields_for_booth_confirmation();
        result.AddRange(new List<WebsiteEventBoothSaleExhibitor.EventBoothRegistration>
        {
            this,
            Env.Ref<WebsiteEventBoothSaleExhibitor.EventBoothRegistration>("WebsiteEventBoothSaleExhibitor.EventBoothRegistration"),
            Env.Ref<WebsiteEventBoothSaleExhibitor.EventBoothRegistration>("WebsiteEventBoothSaleExhibitor.EventBoothRegistration"),
            Env.Ref<WebsiteEventBoothSaleExhibitor.EventBoothRegistration>("WebsiteEventBoothSaleExhibitor.EventBoothRegistration"),
            Env.Ref<WebsiteEventBoothSaleExhibitor.EventBoothRegistration>("WebsiteEventBoothSaleExhibitor.EventBoothRegistration"),
            Env.Ref<WebsiteEventBoothSaleExhibitor.EventBoothRegistration>("WebsiteEventBoothSaleExhibitor.EventBoothRegistration"),
            Env.Ref<WebsiteEventBoothSaleExhibitor.EventBoothRegistration>("WebsiteEventBoothSaleExhibitor.EventBoothRegistration")
        });
        return result;
    }
}
