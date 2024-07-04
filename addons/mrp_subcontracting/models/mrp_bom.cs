csharp
public partial class MrpBom
{
    public MrpBom BomSubcontractFind(Product.Product product, Picking.PickingType pickingType, Res.Company company, string bomType, Res.Partner subcontractor)
    {
        var domain = BomFindDomain(product, pickingType, company, bomType);
        if (subcontractor != null)
        {
            domain = Env.AddAnd(domain, new List<object[]> { new object[] { "Subcontractors", "parent_of", subcontractor.Id } });
            return Env.SearchOne<MrpBom>(domain, new List<object[]> { new object[] { "Sequence", "ASC" }, new object[] { "Product", "ASC" }, new object[] { "Id", "ASC" } });
        }
        else
        {
            return null;
        }
    }

    private List<object[]> BomFindDomain(Product.Product product, Picking.PickingType pickingType, Res.Company company, string bomType)
    {
        var domain = new List<object[]>();
        domain.Add(new object[] { "Product", "=", product.Id });
        if (pickingType != null)
        {
            domain.Add(new object[] { "PickingType", "=", pickingType.Id });
        }
        if (company != null)
        {
            domain.Add(new object[] { "Company", "=", company.Id });
        }
        domain.Add(new object[] { "Type", "=", bomType });
        return domain;
    }

    public void CheckSubcontractingNoOperation()
    {
        if (Env.FilteredDomain(this, new List<object[]> { new object[] { "Type", "=", "subcontract" }, new object[] { "OperationIds", "!=", null }, new object[] { "ByproductIds", "!=", null" } }))
        {
            throw new Exception("You can not set a Bill of Material with operations or by-product line as subcontracting.");
        }
    }
}
