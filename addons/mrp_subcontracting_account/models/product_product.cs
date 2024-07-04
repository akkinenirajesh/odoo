csharp
public partial class MrpSubcontractingAccount.ProductProduct 
{
    public virtual decimal ComputeBomPrice(MrpSubcontractingAccount.MrpBom bom, bool bomsToRecompute = false, bool byproductBom = false)
    {
        decimal price = Env.Call("MrpSubcontractingAccount.ProductProduct", "ComputeBomPrice", this, bom, bomsToRecompute, byproductBom);
        if (bom != null && bom.Type == "subcontract")
        {
            var seller = Env.Call<MrpSubcontractingAccount.ProductProduct, MrpSubcontractingAccount.ProductSupplier>("_SelectSeller", this, bom.ProductQty, bom.ProductUom, new Dictionary<string, object> { { "SubcontractorIds", bom.SubcontractorIds } });
            if (seller != null)
            {
                decimal sellerPrice = seller.Currency.Convert(seller.Price, Env.Company.Currency, (bom.Company ?? Env.Company), DateTime.Now);
                price += seller.ProductUom.ComputePrice(sellerPrice, this.Uom);
            }
        }
        return price;
    }
}
