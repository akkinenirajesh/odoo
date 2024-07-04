C#
public partial class Product.IrAttachment
{
    public IrAttachment Create(List<Dictionary<string, object>> valsList)
    {
        var attachments = base.Create(valsList);
        if (!Env.Context.ContainsKey("disable_product_documents_creation"))
        {
            var productAttachments = attachments.Where(attachment =>
                attachment.ResModel == "product.product" || attachment.ResModel == "product.template" &&
                string.IsNullOrEmpty(attachment.ResField)).ToList();
            if (productAttachments.Any())
            {
                Env.Model("product.document").Create(productAttachments.Select(attachment => new Dictionary<string, object> { { "IrAttachmentId", attachment.Id } }).ToList());
            }
        }
        return attachments;
    }
}
