csharp
public partial class ProductExpiry.StockLot {
    public bool UseExpirationDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public DateTime UseDate { get; set; }
    public DateTime RemovalDate { get; set; }
    public DateTime AlertDate { get; set; }
    public bool ProductExpiryAlert { get; set; }
    public bool ProductExpiryReminded { get; set; }

    public void ComputeExpirationDate() {
        if (Env.Get("ProductId").Get("UseExpirationDate") && ExpirationDate == null) {
            ExpirationDate = DateTime.Now.AddDays(Env.Get("ProductId").Get("ProductTmplId").Get("ExpirationTime"));
        }
    }

    public void ComputeDates() {
        if (!Env.Get("ProductId").Get("UseExpirationDate")) {
            UseDate = null;
            RemovalDate = null;
            AlertDate = null;
        } else if (ExpirationDate != null) {
            if (Env.Get("ProductId").Get("ProductTmplId").Get("UseTime") != null &&
                Env.Get("ProductId").Get("ProductTmplId").Get("RemovalTime") != null &&
                Env.Get("ProductId").Get("ProductTmplId").Get("AlertTime") != null) {
                UseDate = ExpirationDate.AddDays(-Env.Get("ProductId").Get("ProductTmplId").Get("UseTime"));
                RemovalDate = ExpirationDate.AddDays(-Env.Get("ProductId").Get("ProductTmplId").Get("RemovalTime"));
                AlertDate = ExpirationDate.AddDays(-Env.Get("ProductId").Get("ProductTmplId").Get("AlertTime"));
            }
        }
    }

    public void ComputeProductExpiryAlert() {
        if (ExpirationDate != null) {
            ProductExpiryAlert = ExpirationDate <= DateTime.Now;
        } else {
            ProductExpiryAlert = false;
        }
    }
}

public partial class Procurement.ProcurementGroup {
    public List<Procurement.ProcurementGroup> Groups { get; set; }

    public void RunSchedulerTasks() {
        Env.Get("ProductExpiry.StockLot").Search(new List<Tuple<string, object>>{Tuple.Create("AlertDate", "<=", DateTime.Now), Tuple.Create("ProductExpiryReminded", "=", false)}).ForEach(lot => {
            lot.ActivitySchedule("product_expiry.mail_activity_type_alert_date_reached",
                Env.Get("ProductId").WithCompany(lot.Get("CompanyId")).Get("ResponsibleId")?.Get("Id") ?? Env.Get("ProductId").Get("ResponsibleId")?.Get("Id") ?? Env.Get("SuperUserId"),
                "The alert date has been reached for this lot/serial number");
            lot.Write(new Dictionary<string, object>{ { "ProductExpiryReminded", true } });
        });
    }
}
