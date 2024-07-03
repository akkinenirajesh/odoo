csharp
public partial class ResCompany
{
    public BarcodeNomenclature GetDefaultNomenclature()
    {
        return Env.Ref("Barcodes.DefaultBarcodeNomenclature", raiseIfNotFound: false);
    }

    public override void OnCreate()
    {
        base.OnCreate();
        if (NomenclatureId == null)
        {
            NomenclatureId = GetDefaultNomenclature();
        }
    }
}
