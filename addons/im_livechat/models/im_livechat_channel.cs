csharp
public partial class ImLivechatChannel
{
    public override string ToString()
    {
        return Name;
    }

    public void ActionJoin()
    {
        Operators = Operators.Append(Env.User);
        Env.Bus.SendOne(Env.User.Partner, "mail.record/insert", new
        {
            LivechatChannel = new
            {
                Id = Id,
                Name = Name,
                HasSelfAsMember = true
            }
        });
    }

    public void ActionQuit()
    {
        Operators = Operators.Where(u => u != Env.User).ToList();
        Env.Bus.SendOne(Env.User.Partner, "mail.record/insert", new
        {
            LivechatChannel = new
            {
                Id = Id,
                Name = Name,
                HasSelfAsMember = false
            }
        });
    }

    public ActionResult ActionViewRating()
    {
        var action = Env.Ref<IrActionsActWindow>("im_livechat.rating_rating_action_livechat");
        action.Context["search_default_parent_res_name"] = Name;
        return action;
    }

    public ActionResult ActionViewChatbotScripts()
    {
        var action = Env.Ref<IrActionsActWindow>("im_livechat.chatbot_script_action");
        var chatbotScriptIds = Rules.Select(r => r.ChatbotScript).Where(s => s != null).ToList();

        if (chatbotScriptIds.Count == 1)
        {
            action.ResId = chatbotScriptIds[0].Id;
            action.ViewMode = "form";
            action.Views = new List<(int?, string)> { (null, "form") };
        }
        else
        {
            action.Domain = new List<object> { new List<object> { "id", "in", chatbotScriptIds.Select(s => s.Id).ToList() } };
        }

        return action;
    }

    // Add other methods as needed...
}
