csharp
public partial class CalendarEvent
{
    public bool NeedVideoCall()
    {
        if (this.ResModel == "Hr.Leave")
        {
            return false;
        }
        return base.NeedVideoCall();
    }
}
