csharp
public partial class WebsiteChatbotScript 
{
    public void ActionTestScript()
    {
        this.EnsureOne();
        Env.Action.ActUrl(string.Format("/chatbot/{0}/test", this.Id), "self");
    }
}
