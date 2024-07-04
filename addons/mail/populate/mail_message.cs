csharp
public partial class MailMessage {
    public MailMessage(Buvi.Env env) {
        this.Env = env;
    }

    public Buvi.Env Env { get; private set; }

    public void _Populate(string size) {
        // Logic to implement
        _PopulateThreads(size, "Res.Partner");
    }

    private void _PopulateThreads(string size, string modelName) {
        // Logic to implement
    }
}
