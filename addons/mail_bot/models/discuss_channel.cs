csharp
public partial class DiscussChannel {
    public void ExecuteCommandHelp() {
        Env.Call("DiscussChannel", "ExecuteCommandHelp", this);
        Env.Call("Mail.Bot", "_ApplyLogic", this, new { command = "help" });
    }
}
