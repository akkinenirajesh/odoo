csharp
public partial class MailGroupMessage {
    public void ActionModerateAccept() {
        if (Env.IsAuthenticated && this.ModerationStatus == Mail.ModerationStatus.PendingModeration) {
            this.ModerationStatus = Mail.ModerationStatus.Accepted;
            this.ModeratorId = Env.User.Id;
            // Send the email to the members of the group
            this.MailGroupId.NotifyMembers(this);
        }
    }

    public void ActionModerateRejectWithComment(string rejectSubject, string rejectComment) {
        if (Env.IsAuthenticated && this.ModerationStatus == Mail.ModerationStatus.PendingModeration) {
            if (!string.IsNullOrEmpty(rejectSubject) || !string.IsNullOrEmpty(rejectComment)) {
                ModerateSendRejectEmail(rejectSubject, rejectComment);
            }
            ActionModerateReject();
        }
    }

    public void ActionModerateReject() {
        if (Env.IsAuthenticated && this.ModerationStatus == Mail.ModerationStatus.PendingModeration) {
            this.ModerationStatus = Mail.ModerationStatus.Rejected;
            this.ModeratorId = Env.User.Id;
        }
    }

    public void ActionModerateAllow() {
        CreateModerationRule(Mail.AuthorModerationStatus.Allow);

        // Accept all emails of the same authors
        var sameAuthor = GetPendingSameAuthorSameGroup();
        sameAuthor.ActionModerateAccept();
    }

    public void ActionModerateBan() {
        CreateModerationRule(Mail.AuthorModerationStatus.Ban);

        // Reject all emails of the same author
        var sameAuthor = GetPendingSameAuthorSameGroup();
        sameAuthor.ActionModerateReject();
    }

    public void ActionModerateBanWithComment(string banSubject, string banComment) {
        CreateModerationRule(Mail.AuthorModerationStatus.Ban);

        if (!string.IsNullOrEmpty(banSubject) || !string.IsNullOrEmpty(banComment)) {
            ModerateSendRejectEmail(banSubject, banComment);
        }

        // Reject all emails of the same author
        var sameAuthor = GetPendingSameAuthorSameGroup();
        sameAuthor.ActionModerateReject();
    }

    private MailGroupMessage GetPendingSameAuthorSameGroup() {
        return Env.Model("Mail.MailGroupMessage").Search(new object[] {
            new object[] { "MailGroupId", "=", this.MailGroupId.Id },
            new object[] { "EmailFromNormalized", "=", this.EmailFromNormalized },
            new object[] { "ModerationStatus", "=", Mail.ModerationStatus.PendingModeration }
        });
    }

    private void CreateModerationRule(Mail.AuthorModerationStatus status) {
        if (status != Mail.AuthorModerationStatus.Ban && status != Mail.AuthorModerationStatus.Allow) {
            throw new Exception($"Wrong status ({status})");
        }

        if (string.IsNullOrEmpty(this.EmailFrom)) {
            throw new Exception($"The email \"{this.EmailFrom}\" is not valid.");
        }

        var existingModeration = Env.Model("Mail.MailGroupModeration").Search(new object[] {
            new object[] { "Email", "=", this.EmailFromNormalized },
            new object[] { "MailGroupId", "=", this.MailGroupId.Id }
        });
        existingModeration.Status = status;

        var moderationToCreate = new List<object>();
        if (!existingModeration.Any(m => m.Email == this.EmailFromNormalized)) {
            moderationToCreate.Add(new object[] {
                "Email", this.EmailFromNormalized,
                "MailGroupId", this.MailGroupId.Id,
                "Status", status
            });
        }

        Env.Model("Mail.MailGroupModeration").Create(moderationToCreate);
    }

    private void _assertModerable() {
        if (this.ModerationStatus != Mail.ModerationStatus.PendingModeration) {
            throw new Exception("This message can not be moderated");
        }
    }

    private void ModerateSendRejectEmail(string subject, string comment) {
        if (!string.IsNullOrEmpty(this.EmailFrom)) {
            var bodyHtml = Env.Model("Mail.RenderMixin").ReplaceLocalLinks(
                $"<div>{comment}</div>"
            );

            Env.Model("Mail.Mail").Create(new object[] {
                "AuthorId", Env.User.PartnerId.Id,
                "AutoDelete", true,
                "BodyHtml", bodyHtml,
                "EmailFrom", Env.User.EmailFormatted ?? Env.Company.CatchallFormatted,
                "EmailTo", this.EmailFrom,
                "References", this.MailMessageId.MessageId,
                "Subject", subject,
                "State", Mail.MailState.Outgoing
            });
        }
    }
}
