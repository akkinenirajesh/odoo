csharp
public partial class WebsiteSlidesForum.SlideChannel {

    public virtual void ActionRedirectToForum() {
        var action = Env.Get("ir.actions.actions")._ForXmlId("website_forum.forum_post_action");
        action.ViewMode = "tree";
        action.Context = new { Create = false };
        action.Domain = new { ForumId = this.ForumId.Id };
        return action;
    }

    public virtual void Create(List<object> valsList) {
        var channels = base.Create(valsList);
        channels.ForumId.Privacy = false;
        return channels;
    }

    public virtual void Write(object vals) {
        var oldForum = this.ForumId;
        base.Write(vals);
        if (vals.ContainsKey("ForumId")) {
            this.ForumId.Privacy = false;
            if (oldForum != this.ForumId) {
                oldForum.Write(new {
                    Privacy = "private",
                    AuthorizedGroupId = Env.Ref("website_slides.group_website_slides_officer").Id
                });
            }
        }
    }
}
