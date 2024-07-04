C#
public partial class MassMailingSms.SmsTracker {
    public void ActionUpdateFromProviderError(string providerError) {
        var errorStatus = Env.Call("MassMailingSms.SmsTracker", "_ActionUpdateFromProviderError", this, providerError);
    }

    public void ActionUpdateFromSmsState(string smsState, string failureType, string failureReason) {
        Env.Call("MassMailingSms.SmsTracker", "_ActionUpdateFromSmsState", this, smsState, failureType, failureReason);
    }

    private (string, string, string) _ActionUpdateFromProviderError(string providerError) {
        var (errorStatus, failureType, failureReason) = Env.Call<MassMailingSms.SmsTracker>("_ActionUpdateFromProviderError", this, providerError);
        _UpdateSmsTraces(errorStatus ?? "error", failureType, failureReason);
        return (errorStatus, failureType, failureReason);
    }

    private void _ActionUpdateFromSmsState(string smsState, string failureType, string failureReason) {
        Env.Call("MassMailingSms.SmsTracker", "_ActionUpdateFromSmsState", this, smsState, failureType, failureReason);
        var traceStatus = SMSStateToTraceStatus[smsState];
        var traces = _UpdateSmsTraces(traceStatus, failureType, failureReason);
        _UpdateSmsMailings(traceStatus, traces);
    }

    private List<MassMailing.MailingTrace> _UpdateSmsTraces(string traceStatus, string failureType, string failureReason) {
        if (MailingTraceId == null) {
            return Env.Ref<MassMailing.MailingTrace>("MassMailing.MailingTrace");
        }

        var statusesToIgnore = StatusesToIgnore[traceStatus];
        var traces = MailingTraceId.Where(t => !statusesToIgnore.Contains(t.TraceStatus));

        if (traces.Any()) {
            var tracesValues = new Dictionary<string, object> {
                { "TraceStatus", traceStatus },
                { "FailureType", failureType },
                { "FailureReason", failureReason },
            };
            if (traceStatus == "pending") {
                tracesValues["SentDateTime"] = Env.Now;
            }

            traces.Write(tracesValues);
        }

        return traces.ToList();
    }

    private void _UpdateSmsMailings(string traceStatus, List<MassMailing.MailingTrace> traces) {
        traces.Flush(["TraceStatus"]);

        if (traceStatus == "process") {
            traces.First().MassMailingId.Write(new Dictionary<string, object> {
                { "State", "sending" }
            });
            return;
        }

        var mailingsToMarkDone = Env.Ref<MassMailing.Mailing>(
            "MassMailing.Mailing"
        ).Search(new List<object[]> {
            new object[] { "Id", "in", traces.Select(t => t.MassMailingId.Id).ToList() },
            new object[] { "!", ("MailingTraceIds.TraceStatus", "=", "process") },
            new object[] { "State", "!=", "done" },
        });

        if (mailingsToMarkDone.Any()) {
            if (Env.User.IsPublic) {
                mailingsToMarkDone.First()._TrackSetAuthor(Env.Ref<ResPartner.Partner>("base.partner_root"));
            }

            foreach (var mailing in mailingsToMarkDone) {
                mailing.Write(new Dictionary<string, object> {
                    { "State", "done" },
                    { "SentDate", Env.Now },
                    { "KpiMailRequired", !mailing.SentDate },
                });
            }
        }
    }

    private Dictionary<string, string> SMSStateToTraceStatus = new Dictionary<string, string> {
        { "error", "error" },
        { "process", "process" },
        { "outgoing", "outgoing" },
        { "canceled", "cancel" },
        { "pending", "pending" },
        { "sent", "sent" },
    };

    private Dictionary<string, List<string>> StatusesToIgnore = new Dictionary<string, List<string>> {
        { "cancel", new List<string> { "cancel", "process", "pending", "sent" } },
        { "outgoing", new List<string> { "outgoing", "process", "pending", "sent" } },
        { "process", new List<string> { "process", "pending", "sent" } },
        { "pending", new List<string> { "pending", "sent" } },
        { "bounce", new List<string> { "bounce" } },
        { "sent", new List<string> { "sent" } },
        { "error", new List<string> { "error" } },
    };
}
