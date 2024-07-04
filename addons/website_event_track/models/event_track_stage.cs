csharp
public partial class EventTrackStage 
{
    public void ComputeIsVisibleInAgenda() 
    {
        if (this.IsCancel)
        {
            this.IsVisibleInAgenda = false;
        }
        else if (this.IsFullyAccessible)
        {
            this.IsVisibleInAgenda = true;
        }
    }

    public void ComputeIsFullyAccessible()
    {
        if (this.IsCancel || !this.IsVisibleInAgenda)
        {
            this.IsFullyAccessible = false;
        }
    }
}
