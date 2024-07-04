csharp
public partial class Mrp.ProductDocument {
    public Mrp.AttachedOnMrp AttachedOnMrp { get; set; }

    public Mrp.AttachedOnMrp DefaultAttachedOnMrp() {
        if (Env.Context.ContainsKey("AttachedOnBom") && Env.Context["AttachedOnBom"] is bool && (bool)Env.Context["AttachedOnBom"]) {
            return Mrp.AttachedOnMrp.Bom;
        }
        return Mrp.AttachedOnMrp.Hidden;
    }
}
