csharp
public partial class MassMailing {
    public bool Active { get; set; }
    public string Subject { get; set; }
    public string Preview { get; set; }
    public string EmailFrom { get; set; }
    public bool Favorite { get; set; }
    public DateTime? FavoriteDate { get; set; }
    public DateTime? SentDate { get; set; }
    public string ScheduleType { get; set; }
    public DateTime? ScheduleDate { get; set; }
    public DateTime? CalendarDate { get; set; }
    public string BodyArch { get; set; }
    public string BodyHtml { get; set; }
    public bool IsBodyEmpty { get; set; }
    public List<Attachment> Attachments { get; set; }
    public bool KeepArchives { get; set; }
    public UtmCampaign CampaignId { get; set; }
    public UtmMedium MediumId { get; set; }
    public string State { get; set; }
    public int Color { get; set; }
    public ResUsers UserId { get; set; }
    public string MailingType { get; set; }
    public string MailingTypeDescription { get; set; }
    public string ReplyToMode { get; set; }
    public string ReplyTo { get; set; }
    public string MailingModelReal { get; set; }
    public IrModel MailingModelId { get; set; }
    public string MailingModelName { get; set; }
    public bool MailingOnMailingList { get; set; }
    public string MailingDomain { get; set; }
    public bool MailServerAvailable { get; set; }
    public IrMailServer MailServerId { get; set; }
    public List<MailingList> ContactListIds { get; set; }
    public MailingFilter MailingFilterId { get; set; }
    public string MailingFilterDomain { get; set; }
    public int MailingFilterCount { get; set; }
    public bool AbTestingCompleted { get; set; }
    public string AbTestingDescription { get; set; }
    public bool AbTestingEnabled { get; set; }
    public bool AbTestingIsWinnerMailing { get; set; }
    public int AbTestingMailingsCount { get; set; }
    public int AbTestingPc { get; set; }
    public DateTime? AbTestingScheduleDatetime { get; set; }
    public string AbTestingWinnerSelection { get; set; }
    public bool IsAbTestSent { get; set; }
    public bool KpiMailRequired { get; set; }
    public List<MailingTrace> MailingTraceIds { get; set; }
    public int Total { get; set; }
    public int Scheduled { get; set; }
    public int Expected { get; set; }
    public int Canceled { get; set; }
    public int Sent { get; set; }
    public int Process { get; set; }
    public int Pending { get; set; }
    public int Delivered { get; set; }
    public int Opened { get; set; }
    public int Clicked { get; set; }
    public int Replied { get; set; }
    public int Bounced { get; set; }
    public int Failed { get; set; }
    public double ReceivedRatio { get; set; }
    public double OpenedRatio { get; set; }
    public double RepliedRatio { get; set; }
    public double BouncedRatio { get; set; }
    public double ClicksRatio { get; set; }
    public int LinkTrackersCount { get; set; }
    public DateTime? NextDeparture { get; set; }
    public bool NextDepartureIsPast { get; set; }
    public string WarningMessage { get; set; }

    public void ActionSetFavorite() {
        // Add the current mailing in the favorites list.
        this.Favorite = true;
        // TODO: Implement this method
    }

    public void ActionRemoveFavorite() {
        // Remove the current mailing from the favorites list.
        this.Favorite = false;
        // TODO: Implement this method
    }

    public void ActionDuplicate() {
        // TODO: Implement this method
    }

    public void ActionTest() {
        // TODO: Implement this method
    }

    public void ActionLaunch() {
        // TODO: Implement this method
    }

    public void ActionReload() {
        // TODO: Implement this method
    }

    public void ActionSchedule() {
        // TODO: Implement this method
    }

    public void ActionPutInQueue() {
        // TODO: Implement this method
    }

    public void ActionCancel() {
        // TODO: Implement this method
    }

    public void ActionRetryFailed() {
        // TODO: Implement this method
    }

    public void ActionViewLinkTrackers() {
        // TODO: Implement this method
    }

    public void ActionViewTracesScheduled() {
        // TODO: Implement this method
    }

    public void ActionViewTracesCanceled() {
        // TODO: Implement this method
    }

    public void ActionViewTracesFailed() {
        // TODO: Implement this method
    }

    public void ActionViewTracesProcess() {
        // TODO: Implement this method
    }

    public void ActionViewTracesSent() {
        // TODO: Implement this method
    }

    public void ActionViewClicked() {
        // TODO: Implement this method
    }

    public void ActionViewOpened() {
        // TODO: Implement this method
    }

    public void ActionViewReplied() {
        // TODO: Implement this method
    }

    public void ActionViewBounced() {
        // TODO: Implement this method
    }

    public void ActionViewDelivered() {
        // TODO: Implement this method
    }

    public void ActionViewMailingContacts() {
        // Show the mailing contacts who are in a mailing list selected for this mailing.
        // TODO: Implement this method
    }

    public void ActionCompareVersions() {
        // TODO: Implement this method
    }

    public void ActionSendWinnerMailing() {
        // TODO: Implement this method
    }

    public void ActionSelectAsWinner() {
        // TODO: Implement this method
    }

    public void _GetAbTestingDescriptionValues() {
        // TODO: Implement this method
    }

    public void _GetAbTestingSiblingsMailings() {
        // TODO: Implement this method
    }

    public void _GetAbTestingWinnerSelection() {
        // TODO: Implement this method
    }

    public void _GetDefaultAbTestingCampaignValues() {
        // TODO: Implement this method
    }

    public void _GetOptOutList() {
        // TODO: Implement this method
    }

    public void _GetLinkTrackerValues() {
        // TODO: Implement this method
    }

    public void _GetSeenList() {
        // TODO: Implement this method
    }

    public void _GetSeenListExtra() {
        // TODO: Implement this method
    }

    public void _GetMassMailingContext() {
        // TODO: Implement this method
    }

    public void _GetRecipients() {
        // TODO: Implement this method
    }

    public void _GetRemainingRecipients() {
        // TODO: Implement this method
    }

    public void _GetUnsubscribeOneclickUrl() {
        // TODO: Implement this method
    }

    public void _GetUnsubscribeUrl() {
        // TODO: Implement this method
    }

    public void _GetViewUrl() {
        // TODO: Implement this method
    }

    public void ActionSendMail() {
        // TODO: Implement this method
    }

    public void ConvertLinks() {
        // TODO: Implement this method
    }

    public void _ProcessMassMailingQueue() {
        // TODO: Implement this method
    }

    public void _ActionSendStatistics() {
        // Send an email to the responsible of each finished mailing with the statistics.
        // TODO: Implement this method
    }

    public void _PrepareStatisticsEmailValues() {
        // TODO: Implement this method
    }

    public void _GetPrettyMailingType() {
        // TODO: Implement this method
    }

    public void _GenerateMailingReportToken() {
        // TODO: Implement this method
    }

    public void _ConvertInlineImagesToUrls() {
        // TODO: Implement this method
    }

    public void _CreateAttachmentsFromInlineImages() {
        // TODO: Implement this method
    }

    public void _GetDefaultMailingDomain() {
        // TODO: Implement this method
    }

    public void _GetImageByUrl() {
        // TODO: Implement this method
    }

    public void _ParseMailingDomain() {
        // TODO: Implement this method
    }

    public void _GenerateMailingRecipientToken() {
        // TODO: Implement this method
    }
}
