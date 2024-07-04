csharp
public partial class WebsiteEventMeet.EventMeetingRoom 
{
    public void ComputeWebsiteUrl()
    {
        if (this.Id != 0)
        {
            var baseUrl = this.EventId.GetBaseUrl();
            this.WebsiteUrl = $"{baseUrl}/event/{this.EventId.Slug}/meeting_room/{this.Slug}";
        }
    }

    public void Create(List<Dictionary<string, object>> valuesList)
    {
        foreach (var values in valuesList)
        {
            if (!values.ContainsKey("ChatRoomId") && !values.ContainsKey("RoomName"))
            {
                values["RoomName"] = $"odoo-room-{values["Name"]}";
            }
        }
        // Call the base create method.
        // ... 
    }

    public void ArchiveMeetingRooms()
    {
        Env.Search<WebsiteEventMeet.EventMeetingRoom>(x => !x.IsPinned && x.Active && x.RoomParticipantCount == 0 && x.RoomLastActivity < DateTime.Now.AddHours(-4)).Active = false;
    }

    public void OpenWebsiteUrl()
    {
        if (this.EventId.WebsiteId != null)
        {
            // Call the base open_website_url method.
            // ... 
        }
        else
        {
            var website = Env.Get<Website.Website>();
            var action = website.GetClientAction($"/event/{this.EventId.Slug}/meeting_room/{this.Slug}");
        }
    }
}
