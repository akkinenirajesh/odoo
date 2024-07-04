C#
public partial class Website_ImLivechatChannel
{
    public Website_ImLivechatChannel()
    {
        // This constructor is only for initialization. 
        // You can add your initial values here if required.
    }

    public string ComputeWebsiteUrl()
    {
        return "/livechat/channel/" + Env.Utils.Slugify(this.Name);
    }
}
