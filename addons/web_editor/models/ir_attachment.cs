csharp
public partial class Web_IrAttachment {
    public string LocalUrl { get; set; }
    public string ImageSrc { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    public Web_IrAttachment OriginalId { get; set; }
    public void ComputeLocalUrl() {
        if (this.Url != null) {
            this.LocalUrl = this.Url;
        } else {
            this.LocalUrl = "/web/image/" + this.Id + "?unique=" + this.Checksum;
        }
    }

    public void ComputeImageSrc() {
        if (Web.SupportedImageMimeTypes.ContainsKey(this.Mimetype)) {
            if (this.Type == "url") {
                if (this.Url.StartsWith("/")) {
                    this.ImageSrc = this.Url;
                } else {
                    string name = System.Net.WebUtility.UrlEncode(this.Name);
                    this.ImageSrc = "/web/image/" + this.Id + "-redirect/" + name;
                }
            } else {
                string unique = this.Checksum.Substring(0, 8);
                if (this.Url != null) {
                    string separator = this.Url.Contains("?") ? "&" : "?";
                    this.ImageSrc = this.Url + separator + "unique=" + unique;
                } else {
                    string name = System.Net.WebUtility.UrlEncode(this.Name);
                    this.ImageSrc = "/web/image/" + this.Id + "-" + unique + "/" + name;
                }
            }
        } else {
            this.ImageSrc = null;
        }
    }

    public void ComputeImageSize() {
        try {
            System.Drawing.Image image = Odoo.Tools.Base64ToImage(this.Datas);
            this.ImageWidth = image.Width;
            this.ImageHeight = image.Height;
        } catch {
            this.ImageWidth = 0;
            this.ImageHeight = 0;
        }
    }

    public System.Collections.Generic.Dictionary<string, object> GetMediaInfo() {
        return new System.Collections.Generic.Dictionary<string, object>() {
            {"Id", this.Id},
            {"Name", this.Name},
            {"Description", this.Description},
            {"Mimetype", this.Mimetype},
            {"Checksum", this.Checksum},
            {"Url", this.Url},
            {"Type", this.Type},
            {"ResId", this.ResId},
            {"ResModel", this.ResModel},
            {"Public", this.Public},
            {"AccessToken", this.AccessToken},
            {"ImageSrc", this.ImageSrc},
            {"ImageWidth", this.ImageWidth},
            {"ImageHeight", this.ImageHeight},
            {"OriginalId", this.OriginalId}
        };
    }

    public bool CanBypassRightsOnMediaDialog(System.Collections.Generic.Dictionary<string, object> attachmentData) {
        return false;
    }
}
