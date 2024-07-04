csharp
public partial class WebsiteSaleProductImage {
    public void ComputeEmbedCode() {
        this.EmbedCode = Env.GetService<WebEditorTools>().GetVideoEmbedCode(this.VideoUrl);
    }

    public void ComputeCanImage1024BeZoomed() {
        this.CanImage1024BeZoomed = this.Image1920 != null && Env.GetService<Tools>().IsImageSizeAbove(this.Image1920, this.Image1024);
    }

    public void OnChangeVideoUrl() {
        if (this.Image1920 == null) {
            var thumbnail = Env.GetService<WebEditorTools>().GetVideoThumbnail(this.VideoUrl);
            this.Image1920 = thumbnail != null ? Convert.ToBase64String(thumbnail) : null;
        }
    }

    public void CheckValidVideoUrl() {
        if (this.VideoUrl != null && this.EmbedCode == null) {
            throw new Exception("Provided video URL for '" + this.Name + "' is not valid. Please enter a valid video URL.");
        }
    }

    public WebsiteSaleProductImage Create(Dictionary<string, object> values) {
        var contextWithoutTemplate = Env.Context.Where(kvp => kvp.Key != "default_product_tmpl_id").ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var normalVals = new List<Dictionary<string, object>>();
        var variantValsList = new List<Dictionary<string, object>>();

        foreach (var val in values) {
            if (val.ContainsKey("ProductVariantId") && Env.Context.ContainsKey("default_product_tmpl_id")) {
                variantValsList.Add(val);
            } else {
                normalVals.Add(val);
            }
        }

        return Env.Create<WebsiteSaleProductImage>(normalVals).Concat(Env.WithContext(contextWithoutTemplate).Create<WebsiteSaleProductImage>(variantValsList)).ToList().FirstOrDefault();
    }
}
