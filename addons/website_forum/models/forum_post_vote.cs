C#
public partial class WebsiteForumPostVote
{
    public int GetKarmaValue(string oldVote, string newVote, int upKarma, int downKarma)
    {
        var karmaValues = new Dictionary<string, int> { { "-1", downKarma }, { "0", 0 }, { "1", upKarma } };
        var karma = karmaValues[newVote] - karmaValues[oldVote];
        return karma;
    }

    public void Create(Dictionary<string, object> vals)
    {
        if (!Env.IsAdmin)
        {
            vals.Remove("UserId");
        }

        var vote = Env.Create<WebsiteForumPostVote>(vals);
        vote.CheckGeneralRights();
        vote.CheckKarmaRights(vote.Vote == "1");

        // karma update
        vote.VoteUpdateKarma("0", vote.Vote);
    }

    public void Write(Dictionary<string, object> values)
    {
        if (!Env.IsAdmin)
        {
            values.Remove("UserId");
        }

        var vote = this;
        vote.CheckGeneralRights(values);
        var voteValue = values.GetValueOrDefault("Vote");
        if (voteValue != null)
        {
            var upvote = vote.Vote == "-1" ? voteValue == "0" : voteValue == "1";
            vote.CheckKarmaRights(upvote);

            // karma update
            vote.VoteUpdateKarma(vote.Vote, voteValue);
        }

        Env.Write(this, values);
    }

    public void CheckGeneralRights(Dictionary<string, object> vals = null)
    {
        var post = this.PostId;
        if (vals != null && vals.ContainsKey("PostId"))
        {
            post = Env.Get<WebsiteForumPost>(vals["PostId"]);
        }

        if (!Env.IsAdmin)
        {
            // own post check
            if (Env.UserId == post.CreateUserId)
            {
                throw new Exception("It is not allowed to vote for its own post.");
            }
            // own vote check
            if (Env.UserId != this.UserId)
            {
                throw new Exception("It is not allowed to modify someone else's vote.");
            }
        }
    }

    public void CheckKarmaRights(bool upvote)
    {
        if (upvote && !this.PostId.CanUpvote)
        {
            throw new AccessError(string.Format("{0} karma required to upvote.", this.ForumId.KarmaUpvote));
        }
        else if (!upvote && !this.PostId.CanDownvote)
        {
            throw new AccessError(string.Format("{0} karma required to downvote.", this.ForumId.KarmaDownvote));
        }
    }

    public void VoteUpdateKarma(string oldVote, string newVote)
    {
        var karma = GetKarmaValue(oldVote, newVote, this.ForumId.KarmaGenAnswerUpvote, this.ForumId.KarmaGenAnswerDownvote);
        var reason = "";
        var source = "";

        if (this.PostId.ParentId != null)
        {
            if (oldVote == newVote)
            {
                reason = "no changes";
            }
            else if (newVote == "1")
            {
                reason = "upvoted";
            }
            else if (newVote == "-1")
            {
                reason = "downvoted";
            }
            else if (oldVote == "1")
            {
                reason = "no more upvoted";
            }
            else
            {
                reason = "no more downvoted";
            }

            source = string.Format("Answer {0}", reason);
            this.RecipientId.AddKarma(karma, this.PostId, source);
        }
        else
        {
            if (oldVote == newVote)
            {
                reason = "no changes";
            }
            else if (newVote == "1")
            {
                reason = "upvoted";
            }
            else if (newVote == "-1")
            {
                reason = "downvoted";
            }
            else if (oldVote == "1")
            {
                reason = "no more upvoted";
            }
            else
            {
                reason = "no more downvoted";
            }

            source = string.Format("Question {0}", reason);
            this.RecipientId.AddKarma(karma, this.PostId, source);
        }
    }
}
