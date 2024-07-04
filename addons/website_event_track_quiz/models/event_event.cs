csharp
public partial class WebsiteEventTrackQuiz.Event {

    public void ComputeCommunityMenu()
    {
        if (this.EventTypeID != null && this.EventTypeID != this._origin.EventTypeID)
        {
            this.CommunityMenu = this.EventTypeID.CommunityMenu;
        }
        else if (this.WebsiteMenu && (this.WebsiteMenu != this._origin.WebsiteMenu || !this.CommunityMenu))
        {
            this.CommunityMenu = true;
        }
        else if (!this.WebsiteMenu)
        {
            this.CommunityMenu = false;
        }
    }
}
