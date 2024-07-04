csharp
public partial class ChatRoom {
    public void ComputeIsFull() {
        if (this.MaxCapacity == "NoLimit") {
            this.IsFull = false;
        } else {
            this.IsFull = this.ParticipantCount >= int.Parse(this.MaxCapacity);
        }
    }

    public void ComputeJitsiServerDomain() {
        this.JitsiServerDomain = Env.GetParam("website_jitsi.jitsi_server_domain", "meet.jit.si");
    }
}
