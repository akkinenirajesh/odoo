csharp
public partial class DiscussVoiceMetadata
{
    public override string ToString()
    {
        // Simple representation, you might want to adjust this based on your needs
        return $"Voice Metadata for Attachment {AttachmentId?.Id ?? 0}";
    }
}
