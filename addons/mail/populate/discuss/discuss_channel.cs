csharp
public partial class Mail.DiscussChannel
{
    public void Populate(int size)
    {
        // install_mode to prevent from automatically adding system as member
        var context = Env.Context;
        context.Set("install_mode", true);
        var self = new Mail.DiscussChannel();
        self.WithContext(context).Populate(size);
    }
}
