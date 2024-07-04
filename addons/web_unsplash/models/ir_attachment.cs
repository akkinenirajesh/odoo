C#
public partial class WebIrAttachment {
  public bool CanBypassRightsOnMediaDialog(Dictionary<string, object> attachmentData) {
    if (attachmentData.ContainsKey("url") && attachmentData["type"].ToString() == "binary" && attachmentData["url"].ToString().StartsWith("/unsplash/")) {
      return true;
    }
    return Env.Context.Call("Web.IrAttachment", "_can_bypass_rights_on_media_dialog", attachmentData);
  }
}
