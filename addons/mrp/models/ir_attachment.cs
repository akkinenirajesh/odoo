csharp
public partial class Mrp.IrAttachment
{
    public void PostAddCreate(params object[] kwargs)
    {
        var self = this;
        var bom = Env.Model<Mrp.Bom>().Browse(self.ResId);

        if (self.ResModel == "mrp.bom")
        {
            self.ResModel = bom.ProductId != null ? bom.ProductId.Name : bom.ProductTmplId.Name;
            self.ResId = bom.ProductId != null ? bom.ProductId.Id : bom.ProductTmplId.Id;
            Env.Model<Product.Document>().Create(new Dictionary<string, object> {
                {"IrAttachmentId", self.Id},
                {"AttachedOnMrp", "bom"}
            });
        }
    }
}
