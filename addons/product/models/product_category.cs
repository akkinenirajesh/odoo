csharp
public partial class ProductCategory
{
    public void ComputeCompleteName()
    {
        if (this.Parent != null)
        {
            this.CompleteName = $"{this.Parent.CompleteName} / {this.Name}";
        }
        else
        {
            this.CompleteName = this.Name;
        }
    }

    public void ComputeProductCount()
    {
        var readGroupRes = Env.Model("product.template").ReadGroup(new[] { new Domain("CategId", "child_of", this.Id) }, new[] { "CategId" }, new[] { "__count" });
        var groupData = readGroupRes.ToDictionary(x => x.CategId.Id, x => x.Count);
        var productCount = 0;
        foreach (var subCategId in Env.Model("product.category").Search(new[] { new Domain("Id", "child_of", this.Id) }).Select(x => x.Id))
        {
            productCount += groupData.ContainsKey(subCategId) ? groupData[subCategId] : 0;
        }
        this.ProductCount = productCount;
    }

    public void CheckCategoryRecursion()
    {
        if (this.HasCycle())
        {
            throw new ValidationError("You cannot create recursive categories.");
        }
    }

    public void UnlinkExceptDefaultCategory()
    {
        var mainCategory = Env.Ref("product.product_category_all", false);
        if (mainCategory != null && mainCategory.Id == this.Id)
        {
            throw new UserError("You cannot delete this product category, it is the default generic category.");
        }
        var expenseCategory = Env.Ref("product.cat_expense", false);
        if (expenseCategory != null && expenseCategory.Id == this.Id)
        {
            throw new UserError($"You cannot delete the {expenseCategory.Name} product category.");
        }
    }

    private bool HasCycle()
    {
        // Implement cycle detection logic here
    }
}
