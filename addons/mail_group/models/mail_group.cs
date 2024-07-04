C#
public partial class MailGroup 
{
    public bool Active { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public byte[] Image128 { get; set; }
    public ICollection<MailGroupMessage> MailGroupMessageIds { get; set; }
    public int MailGroupMessageLastMonthCount { get; set; }
    public int MailGroupMessageCount { get; set; }
    public int MailGroupMessageModerationCount { get; set; }
    public bool IsMember { get; set; }
    public ICollection<MailGroupMember> MemberIds { get; set; }
    public ICollection<ResPartner> MemberPartnerIds { get; set; }
    public int MemberCount { get; set; }
    public bool IsModerator { get; set; }
    public bool Moderation { get; set; }
    public int ModerationRuleCount { get; set; }
    public ICollection<MailGroupModeration> ModerationRuleIds { get; set; }
    public ICollection<ResUsers> ModeratorIds { get; set; }
    public bool ModerationNotify { get; set; }
    public string ModerationNotifyMsg { get; set; }
    public bool ModerationGuidelines { get; set; }
    public string ModerationGuidelinesMsg { get; set; }
    public MailGroupAccessMode AccessMode { get; set; }
    public ResGroups AccessGroupId { get; set; }
    public bool CanManageGroup { get; set; }
    public MailAliasContact AliasContact { get; set; }
    public string AliasDefaults { get; set; }
    public IrModel AliasModelId { get; set; }
    public int AliasForceThreadId { get; set; }

    public void ComputeMailGroupMessageLastMonthCount() 
    {
        // implement logic to compute MailGroupMessageLastMonthCount
    }

    public void ComputeMailGroupMessageCount() 
    {
        // implement logic to compute MailGroupMessageCount
    }

    public void ComputeMailGroupMessageModerationCount() 
    {
        // implement logic to compute MailGroupMessageModerationCount
    }

    public void ComputeIsMember() 
    {
        // implement logic to compute IsMember
    }

    public void ComputeMemberPartnerIds() 
    {
        // implement logic to compute MemberPartnerIds
    }

    public void SearchMemberPartnerIds(string operator, object operand) 
    {
        // implement logic to search MemberPartnerIds
    }

    public void ComputeMemberCount() 
    {
        // implement logic to compute MemberCount
    }

    public void ComputeIsModerator() 
    {
        // implement logic to compute IsModerator
    }

    public void ComputeModerationRuleCount() 
    {
        // implement logic to compute ModerationRuleCount
    }

    public void ComputeCanManageGroup() 
    {
        // implement logic to compute CanManageGroup
    }

    public void OnChangeAccessMode() 
    {
        // implement logic for OnChangeAccessMode
    }

    public void OnChangeModeration() 
    {
        // implement logic for OnChangeModeration
    }

    public void CheckModeratorEmail() 
    {
        // implement logic for CheckModeratorEmail
    }

    public void CheckModerationNotify() 
    {
        // implement logic for CheckModerationNotify
    }

    public void CheckModerationGuidelines() 
    {
        // implement logic for CheckModerationGuidelines
    }

    public void CheckModeratorExistence() 
    {
        // implement logic for CheckModeratorExistence
    }

    public void CheckAccessMode() 
    {
        // implement logic for CheckAccessMode
    }

    public Dictionary<string, object> AliasGetCreationValues() 
    {
        // implement logic for AliasGetCreationValues
        return new Dictionary<string, object>(); 
    }

    public AliasError AliasGetError(string message, Dictionary<string, object> messageDict, MailAlias alias) 
    {
        // implement logic for AliasGetError
        return null;
    }

    public void MessageNew(Dictionary<string, object> msgDict, Dictionary<string, object> customValues = null) 
    {
        // implement logic for MessageNew
    }

    public void MessageUpdate(Dictionary<string, object> msgDict, Dictionary<string, object> updateVals = null) 
    {
        // implement logic for MessageUpdate
    }

    public int MessagePost(string body = "", string subject = null, string emailFrom = null, int authorId = 0, Dictionary<string, object> kwargs = null) 
    {
        // implement logic for MessagePost
        return 0; 
    }

    public void ActionSendGuidelines(ICollection<MailGroupMember> members = null) 
    {
        // implement logic for ActionSendGuidelines
    }

    public void NotifyMembers(MailGroupMessage message) 
    {
        // implement logic for NotifyMembers
    }

    public void CronNotifyModerators() 
    {
        // implement logic for CronNotifyModerators
    }

    public void NotifyModerators() 
    {
        // implement logic for NotifyModerators
    }

    public string CleanEmailBody(string bodyHtml) 
    {
        // implement logic for CleanEmailBody
        return string.Empty; 
    }

    public void ActionJoin() 
    {
        // implement logic for ActionJoin
    }

    public void ActionLeave() 
    {
        // implement logic for ActionLeave
    }

    public void JoinGroup(string email, int partnerId = 0) 
    {
        // implement logic for JoinGroup
    }

    public void LeaveGroup(string email, int partnerId = 0, bool allMembers = false) 
    {
        // implement logic for LeaveGroup
    }

    public void SendSubscribeConfirmationEmail(string email) 
    {
        // implement logic for SendSubscribeConfirmationEmail
    }

    public void SendUnsubscribeConfirmationEmail(string email) 
    {
        // implement logic for SendUnsubscribeConfirmationEmail
    }

    public string GenerateActionUrl(string email, string action) 
    {
        // implement logic for GenerateActionUrl
        return string.Empty; 
    }

    public string GenerateActionToken(string email, string action) 
    {
        // implement logic for GenerateActionToken
        return string.Empty; 
    }

    public string GenerateEmailAccessToken(string email) 
    {
        // implement logic for GenerateEmailAccessToken
        return string.Empty; 
    }

    public string GenerateGroupAccessToken() 
    {
        // implement logic for GenerateGroupAccessToken
        return string.Empty; 
    }

    public string GetEmailUnsubscribeUrl(string emailTo) 
    {
        // implement logic for GetEmailUnsubscribeUrl
        return string.Empty; 
    }

    public MailGroupMember FindMember(string email, int partnerId = 0) 
    {
        // implement logic for FindMember
        return null; 
    }

    public Dictionary<int, MailGroupMember> FindMembers(string email, int partnerId) 
    {
        // implement logic for FindMembers
        return new Dictionary<int, MailGroupMember>(); 
    }
}
