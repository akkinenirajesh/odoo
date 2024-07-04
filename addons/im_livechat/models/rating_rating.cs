csharp
public partial class Rating
{
    public string ComputeResName()
    {
        if (this.ResModel == "Discuss.Channel")
        {
            var currentObject = Env.GetModel(this.ResModel).Browse(this.ResId);
            return $"{currentObject.LivechatChannelId.Name} / {currentObject.Id}";
        }
        
        // Call base implementation for other models
        return base.ComputeResName();
    }

    public object ActionOpenRatedObject()
    {
        var action = base.ActionOpenRatedObject();

        if (this.ResModel == "Discuss.Channel")
        {
            var discussChannel = Env.GetModel(this.ResModel).Browse(this.ResId);
            if (discussChannel.IsMember)
            {
                var ctx = new Dictionary<string, object>(Env.Context);
                ctx["active_id"] = this.ResId;
                return new Dictionary<string, object>
                {
                    ["type"] = "ir.actions.client",
                    ["tag"] = "mail.action_discuss",
                    ["context"] = ctx
                };
            }

            var viewId = Env.Ref("im_livechat.discuss_channel_view_form").Id;
            action["views"] = new List<object> { new List<object> { viewId, "form" } };
        }

        return action;
    }
}
