csharp
public partial class WebsiteEvent_EventMenu
{
    public virtual void Write(ref Dictionary<string, object> values)
    {
        if (values.ContainsKey("MenuType") && values["MenuType"] == "meeting_room")
        {
            values["MenuType"] = "meeting_room";
        }
    }
}
