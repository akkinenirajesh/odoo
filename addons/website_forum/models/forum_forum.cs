C#
public partial class Forum {
    public Forum() {
    }

    public string Name { get; set; }

    public int Sequence { get; set; }

    public string Mode { get; set; }

    public string Privacy { get; set; }

    public int? AuthorizedGroup { get; set; }

    public bool Active { get; set; }

    public string Faq { get; set; }

    public string Description { get; set; }

    public string Teaser { get; set; }

    public string WelcomeMessage { get; set; }

    public string DefaultOrder { get; set; }

    public double RelevancyPostVote { get; set; }

    public double RelevancyTimeDecay { get; set; }

    public bool AllowShare { get; set; }

    public List<Post> Posts { get; set; }

    public int? LastPostId { get; set; }

    public int TotalPosts { get; set; }

    public int TotalViews { get; set; }

    public int TotalAnswers { get; set; }

    public int TotalFavorites { get; set; }

    public int CountPostsWaitingValidation { get; set; }

    public int CountFlaggedPosts { get; set; }

    public int KarmaGenQuestionNew { get; set; }

    public int KarmaGenQuestionUpvote { get; set; }

    public int KarmaGenQuestionDownvote { get; set; }

    public int KarmaGenAnswerUpvote { get; set; }

    public int KarmaGenAnswerDownvote { get; set; }

    public int KarmaGenAnswerAccept { get; set; }

    public int KarmaGenAnswerAccepted { get; set; }

    public int KarmaGenAnswerFlagged { get; set; }

    public int KarmaAsk { get; set; }

    public int KarmaAnswer { get; set; }

    public int KarmaEditOwn { get; set; }

    public int KarmaEditAll { get; set; }

    public int KarmaEditRetag { get; set; }

    public int KarmaCloseOwn { get; set; }

    public int KarmaCloseAll { get; set; }

    public int KarmaUnlinkOwn { get; set; }

    public int KarmaUnlinkAll { get; set; }

    public int KarmaTagCreate { get; set; }

    public int KarmaUpvote { get; set; }

    public int KarmaDownvote { get; set; }

    public int KarmaAnswerAcceptOwn { get; set; }

    public int KarmaAnswerAcceptAll { get; set; }

    public int KarmaCommentOwn { get; set; }

    public int KarmaCommentAll { get; set; }

    public int KarmaCommentConvertOwn { get; set; }

    public int KarmaCommentConvertAll { get; set; }

    public int KarmaCommentUnlinkOwn { get; set; }

    public int KarmaCommentUnlinkAll { get; set; }

    public int KarmaFlag { get; set; }

    public int KarmaDofollow { get; set; }

    public int KarmaEditor { get; set; }

    public int KarmaUserBio { get; set; }

    public int KarmaPost { get; set; }

    public int KarmaModerate { get; set; }

    public bool HasPendingPost { get; set; }

    public bool CanModerate { get; set; }

    public List<Tag> Tags { get; set; }

    public List<Tag> TagMostUsedIds { get; set; }

    public List<Tag> TagUnusedIds { get; set; }

    public void ComputeTeaser() {
        // C# code to compute Teaser
    }

    public void ComputeLastPostId() {
        // C# code to compute LastPostId
    }

    public void ComputeForumStatistics() {
        // C# code to compute ForumStatistics
    }

    public void ComputeCountPostsWaitingValidation() {
        // C# code to compute CountPostsWaitingValidation
    }

    public void ComputeCountFlaggedPosts() {
        // C# code to compute CountFlaggedPosts
    }

    public void ComputeHasPendingPost() {
        // C# code to compute HasPendingPost
    }

    public void ComputeCanModerate() {
        // C# code to compute CanModerate
    }

    public void ComputeTagIdsUsage() {
        // C# code to compute TagIdsUsage
    }

    public void GoToWebsite() {
        // C# code to go to website
    }

    // Other action methods
}
