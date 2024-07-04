csharp
public partial class WebsiteEventMeet.Event {
    public int MeetingRoomCount { get; set; }
    public bool MeetingRoomAllowCreation { get; set; }

    public void ComputeCommunityMenu() {
        if (this.EventType != null && this.EventType != this._origin.EventType) {
            this.CommunityMenu = this.EventType.CommunityMenu;
        }
        else if (this.WebsiteMenu && (this.WebsiteMenu != this._origin.WebsiteMenu || !this.CommunityMenu)) {
            this.CommunityMenu = true;
        }
        else if (!this.WebsiteMenu) {
            this.CommunityMenu = false;
        }
    }

    public void ComputeMeetingRoomCount() {
        var meetingRoomCount = Env.Ref<WebsiteEventMeet.MeetingRoom>()._readGroup(
            new List<string>() { "Event" },
            new List<string>() { "__count" },
            new List<string>() { "Event", this.Id.ToString() }
        );

        this.MeetingRoomCount = meetingRoomCount.Count();
    }

    public void ComputeMeetingRoomAllowCreation() {
        if (this.EventType != null && this.EventType != this._origin.EventType) {
            this.MeetingRoomAllowCreation = this.EventType.MeetingRoomAllowCreation;
        }
        else if (this.CommunityMenu && this.CommunityMenu != this._origin.CommunityMenu) {
            this.MeetingRoomAllowCreation = true;
        }
        else if (!this.CommunityMenu || !this.MeetingRoomAllowCreation) {
            this.MeetingRoomAllowCreation = false;
        }
    }
}
