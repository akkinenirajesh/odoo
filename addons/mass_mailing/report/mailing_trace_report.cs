csharp
public partial class MailingTraceReport {
  public void Init() {
    // Mass Mail Statistical Report: based on mailing.trace that models the various
    // statistics collected for each mailing, and mailing.mailing model that models the
    // various mailing performed. 
    Env.DropViewIfExists("mailing_trace_report");
    Env.Execute(GetReportRequest());
  }

  private string GetReportRequest() {
    string sqlSelect = string.Join(", ", GetReportRequestSelectItems());
    string sqlFrom = string.Join(" ", GetReportRequestFromItems());
    List<string> sqlWhereItems = GetReportRequestWhereItems();
    string sqlWhere = sqlWhereItems.Count == 1 ? $"WHERE {sqlWhereItems[0]}" : sqlWhereItems.Count > 0 ? $"WHERE {string.Join(" AND ", sqlWhereItems)}" : "";
    string sqlGroupBy = string.Join(", ", GetReportRequestGroupByItems());
    return $"CREATE OR REPLACE VIEW mailing_trace_report AS ({sqlSelect} {sqlFrom} {sqlWhere} {sqlGroupBy} )";
  }

  private List<string> GetReportRequestSelectItems() {
    return new List<string> {
      "min(trace.id) as id",
      "utm_source.name as name",
      "mailing.MailingType",
      "utm_campaign.name as campaign",
      "trace.create_date as scheduled_date",
      "mailing.State",
      "mailing.email_from",
      "COUNT(trace.id) as scheduled",
      "COUNT(trace.sent_datetime) as sent",
      "(COUNT(trace.id) - COUNT(trace.trace_status) FILTER (WHERE trace.trace_status IN ('outgoing', 'pending', 'process', 'error', 'bounce', 'cancel'))) as delivered",
      "COUNT(trace.trace_status) FILTER (WHERE trace.trace_status = 'process') as processing",
      "COUNT(trace.trace_status) FILTER (WHERE trace.trace_status = 'pending') as pending",
      "COUNT(trace.trace_status) FILTER (WHERE trace.trace_status = 'error') as error",
      "COUNT(trace.trace_status) FILTER (WHERE trace.trace_status = 'bounce') as bounced",
      "COUNT(trace.trace_status) FILTER (WHERE trace.trace_status = 'cancel') as canceled",
      "COUNT(trace.trace_status) FILTER (WHERE trace.trace_status = 'open') as opened",
      "COUNT(trace.trace_status) FILTER (WHERE trace.trace_status = 'reply') as replied",
      "COUNT(trace.links_click_datetime) as clicked",
    };
  }

  private List<string> GetReportRequestFromItems() {
    return new List<string> {
      "mailing_trace as trace",
      "LEFT JOIN mailing_mailing as mailing ON (trace.mass_mailing_id=mailing.id)",
      "LEFT JOIN utm_campaign as utm_campaign ON (mailing.campaign_id = utm_campaign.id)",
      "LEFT JOIN utm_source as utm_source ON (mailing.source_id = utm_source.id)"
    };
  }

  private List<string> GetReportRequestWhereItems() {
    return new List<string>();
  }

  private List<string> GetReportRequestGroupByItems() {
    return new List<string> {
      "trace.create_date",
      "utm_source.name",
      "utm_campaign.name",
      "mailing.MailingType",
      "mailing.State",
      "mailing.email_from"
    };
  }
}
