csharp
public partial class WebsiteEventMeet.EventType
{
    public bool MeetingRoomAllowCreation { get; set; }

    public WebsiteEventMeet.CommunityMenu CommunityMenu { get; set; }

    public void ComputeMeetingRoomAllowCreation()
    {
        this.MeetingRoomAllowCreation = this.CommunityMenu;
    }
}
