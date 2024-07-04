C#
public partial class SmsSms 
{
    public void ActionSetCanceled()
    {
        UpdateSmsStateAndTrackers(Sms.SmsState.Canceled);
    }

    public void ActionSetError(Sms.SmsFailureType failureType)
    {
        UpdateSmsStateAndTrackers(Sms.SmsState.Error, failureType);
    }

    public void ActionSetOutgoing()
    {
        UpdateSmsStateAndTrackers(Sms.SmsState.Outgoing);
    }

    public void Send(bool unlinkFailed = false, bool unlinkSent = true, bool autoCommit = false, bool raiseException = false)
    {
        if (this.State != Sms.SmsState.Outgoing || this.ToDelete)
        {
            return;
        }

        var batchSize = int.Parse(Env.GetParameter("sms.session.batch.size", "500"));
        var batchIds = SplitBatch(batchSize);

        foreach (var batchId in batchIds)
        {
            var smsBatch = Env.GetModel("Sms.Sms").Search(batchId).WithContext(new Dictionary<string, object>() { { "sms_skip_msg_notification", true } });
            smsBatch._Send(unlinkFailed, unlinkSent, raiseException);

            if (autoCommit && !Env.IsTesting())
            {
                Env.Commit();
            }
        }
    }

    public void ResendFailed()
    {
        var smsToSend = Env.GetModel("Sms.Sms").Search(new Dictionary<string, object>()
        {
            { "State", Sms.SmsState.Error },
            { "ToDelete", false }
        });

        if (smsToSend.Count > 0)
        {
            smsToSend.State = Sms.SmsState.Outgoing;
            smsToSend.Send();

            var successSms = smsToSend.Count - smsToSend.Search(new Dictionary<string, object>()
            {
                { "State", Sms.SmsState.Error }
            }).Count;

            if (successSms > 0)
            {
                Env.Notify("Success", _("%(count)s out of the %(total)s selected SMS Text Messages have successfully been resent.", count: successSms, total: this.Count), "success");
            }
            else
            {
                Env.Notify("Warning", _("The SMS Text Messages could not be resent."), "danger");
            }
        }
        else
        {
            Env.Notify("Warning", _("There are no SMS Text Messages to resend."), "danger");
        }
    }

    public void ProcessQueue(List<int> ids = null)
    {
        var domain = new Dictionary<string, object>()
        {
            { "State", Sms.SmsState.Outgoing },
            { "ToDelete", false }
        };

        var filteredIds = Env.GetModel("Sms.Sms").Search(domain, 10000);

        if (ids != null)
        {
            filteredIds = filteredIds.Where(id => ids.Contains(id)).ToList();
        }

        filteredIds.Sort();

        try
        {
            var autoCommit = !Env.IsTesting();
            Send(unlinkFailed: false, unlinkSent: true, autoCommit: autoCommit, raiseException: false);
        }
        catch (Exception ex)
        {
            Env.LogException("Failed processing SMS queue", ex);
        }
    }

    private List<List<int>> SplitBatch(int batchSize)
    {
        var batchIds = new List<List<int>>();
        var smsIds = this.Ids;
        for (int i = 0; i < smsIds.Count; i += batchSize)
        {
            batchIds.Add(smsIds.Skip(i).Take(batchSize).ToList());
        }

        return batchIds;
    }

    private void _Send(bool unlinkFailed, bool unlinkSent, bool raiseException)
    {
        var messages = this.GroupBy(sms => sms.Body).Select(group => new
        {
            Content = group.Key,
            Numbers = group.Select(sms => new
            {
                Number = sms.Number,
                Uuid = sms.Uuid
            }).ToList()
        }).ToList();

        var deliveryReportsUrl = Env.GetBaseURL() + "/sms/status";
        try
        {
            var results = Env.GetModel("Sms.Api")._SendSmsBatch(messages, deliveryReportsUrl);
        }
        catch (Exception ex)
        {
            Env.LogInfo("Sent batch %s SMS: %s: failed with exception %s", this.Ids.Count, this.Ids, ex);
            if (raiseException)
            {
                throw;
            }

            var results = this.Select(sms => new { Uuid = sms.Uuid, State = "server_error" }).ToList();
        }
        finally
        {
            Env.LogInfo("Send batch %s SMS: %s: gave %s", this.Ids.Count, this.Ids, results);
        }

        var resultsUuids = results.Select(result => result.Uuid).ToList();
        var allSms = Env.GetModel("Sms.Sms").Search(new Dictionary<string, object>() { { "Uuid", resultsUuids } }).WithContext(new Dictionary<string, object>() { { "sms_skip_msg_notification", true } });

        foreach (var iapState in results.GroupBy(result => result.State))
        {
            var sms = allSms.Where(s => iapState.Select(r => r.Uuid).Contains(s.Uuid)).ToList();
            if (Sms.Sms.IAPToSmsStateSuccess.ContainsKey(iapState.Key))
            {
                var successState = Sms.Sms.IAPToSmsStateSuccess[iapState.Key];
                sms.SmsTrackerId._ActionUpdateFromSmsState(successState);
                var toDelete = new Dictionary<string, object>() { { "ToDelete", true } };
                if (!unlinkSent)
                {
                    toDelete = new Dictionary<string, object>();
                }
                sms.Write(new Dictionary<string, object>() { { "State", successState }, { "FailureType", null }, toDelete });
            }
            else
            {
                var failureType = Sms.Sms.IAPToSmsFailureType.ContainsKey(iapState.Key) ? Sms.Sms.IAPToSmsFailureType[iapState.Key] : Sms.SmsFailureType.Unknown;
                if (failureType != Sms.SmsFailureType.Unknown)
                {
                    sms.SmsTrackerId._ActionUpdateFromSmsState(Sms.SmsState.Error, failureType);
                }
                else
                {
                    sms.SmsTrackerId._ActionUpdateFromProviderError(iapState.Key);
                }
                var toDelete = new Dictionary<string, object>() { { "ToDelete", true } };
                if (!unlinkFailed)
                {
                    toDelete = new Dictionary<string, object>();
                }
                sms.Write(new Dictionary<string, object>() { { "State", Sms.SmsState.Error }, { "FailureType", failureType }, toDelete });
            }
        }

        allSms.MailMessageId._NotifyMessageNotificationUpdate();
    }

    private void UpdateSmsStateAndTrackers(Sms.SmsState newState, Sms.SmsFailureType failureType = null)
    {
        this.Write(new Dictionary<string, object>() { { "State", newState }, { "FailureType", failureType } });
        this.SmsTrackerId._ActionUpdateFromSmsState(newState, failureType);
    }

    private void ComputeSmsTrackerId()
    {
        var existingTrackers = Env.GetModel("Sms.Tracker").Search(new Dictionary<string, object>() { { "SmsUuid", this.Filtered(sms => !string.IsNullOrEmpty(sms.Uuid)).Select(sms => sms.Uuid).ToList() } });
        var trackerIdsBySmsUuid = existingTrackers.ToDictionary(tracker => tracker.SmsUuid, tracker => tracker.Id);
        foreach (var sms in this.Filtered(sms => trackerIdsBySmsUuid.ContainsKey(sms.Uuid)))
        {
            sms.SmsTrackerId = trackerIdsBySmsUuid[sms.Uuid];
        }
    }
}
