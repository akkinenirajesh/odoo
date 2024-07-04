csharp
public partial class MailResPartner
{
    public object SearchForChannelInvite(string searchTerm, int? channelId = null, int limit = 30)
    {
        // Implement logic here
    }
    
    public void SearchForChannelInviteToStore(Store store, DiscussChannel channel)
    {
        // Implement logic here
    }

    public object GetMentionSuggestionsFromChannel(int channelId, string search, int limit = 8)
    {
        // Implement logic here
    }
    
    private object _GetMentionSuggestionsDomain(string search)
    {
        // Implement logic here
    }

    private object _SearchMentionSuggestions(object domain, int limit)
    {
        // Implement logic here
    }
}
