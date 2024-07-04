csharp
public partial class AccountMove {
  public void PreviewInvoice() {
    var action = Env.Call("preview_invoice");
    if (action["url"].ToString().StartsWith("/")) {
      action["url"] = $"/@{action["url"]}";
    }
    Env.Return(action);
  }

  public void ComputeWebsiteId() {
    foreach (var move in this) {
      var sourceWebsites = move.LineIds.SaleLineIds.OrderId.WebsiteId;
      if (sourceWebsites.Count == 1) {
        move.WebsiteId = sourceWebsites;
      } else {
        move.WebsiteId = null;
      }
    }
  }
}
