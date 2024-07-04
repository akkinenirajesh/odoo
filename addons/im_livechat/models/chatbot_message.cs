csharp
public partial class ChatbotMessage
{
    public override string ToString()
    {
        return DiscussChannel?.ToString() ?? base.ToString();
    }

    protected override void OnDeleting()
    {
        base.OnDeleting();
        // Add any additional logic for cascade deletion if needed
    }
}
