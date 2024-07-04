csharp
public partial class WebsiteForumIrAttachment 
{
    public bool CanBypassRightsOnMediaDialog(Dictionary<string, object> attachmentData)
    {
        string resModel = attachmentData["ResModel"].ToString();
        int resId = (int)attachmentData["ResId"];
        if (resModel == "Website.Forum.Post" && resId > 0 && Env.Ref<WebsiteForumPost>(resId).CanUseFullEditor)
        {
            return true;
        }

        return Env.Ref<IrAttachment>().CanBypassRightsOnMediaDialog(attachmentData);
    }
}
