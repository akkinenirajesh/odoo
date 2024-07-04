csharp
public partial class MailBusPresence {

    public void Init()
    {
        Env.Cr.Execute("CREATE UNIQUE INDEX IF NOT EXISTS bus_presence_guest_unique ON %s (guest_id) WHERE guest_id IS NOT NULL", this._Table);
    }
}
