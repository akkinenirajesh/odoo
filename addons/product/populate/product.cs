csharp
public partial class ProductCategory {
    // all the model methods are written here.
    public void Populate(int size) {
        // Implementation of _populate method
        var categories = base.Populate(size);
        // Set parent/child relation
        PopulateSetParents(categories, size);
    }

    public void PopulateSetParents(List<ProductCategory> categories, int size) {
        // Implementation of _populate_set_parents method
        List<int> parentIds = new List<int>();
        var rand = Env.GetRandom("product.category+parent_generator");

        foreach (var category in categories) {
            if (rand.Random() < 0.25) {
                parentIds.Add(category.Id);
            }
        }

        categories.RemoveAll(c => parentIds.Contains(c.Id)); // Avoid recursion in parent-child relations.
        var parentChilds = new Dictionary<int, List<ProductCategory>>();
        foreach (var category in categories) {
            if (rand.Random() < 0.25) { // 1/4 of remaining categories have a parent.
                var parentId = rand.Choice(parentIds);
                if (!parentChilds.ContainsKey(parentId)) {
                    parentChilds.Add(parentId, new List<ProductCategory>());
                }
                parentChilds[parentId].Add(category);
            }
        }

        foreach (var pair in parentChilds) {
            var parent = Env.Get("Product.ProductCategory").Browse(pair.Key);
            pair.Value.ForEach(child => child.Parent = parent);
        }
    }
}

public partial class ProductProduct {
    // all the model methods are written here.
    public List<ProductProduct> PopulateGetProductFactories() {
        // Implementation of _populate_get_product_factories method
        var categoryIds = Env.GetPopulatedModel("Product.ProductCategory");
        var result = new List<ProductProduct>();

        for (int i = 0; i < 100; i++) {
            var product = new ProductProduct();
            product.Sequence = Env.GetRandom().Randrange(0, 100);
            product.Active = Env.GetRandom().RandomizeBool(0.8);
            product.Type = Env.GetRandom().RandomizeString(["consu", "service"], new double[] { 0.7, 0.3 });
            product.Category = Env.Get("Product.ProductCategory").Browse(Env.GetRandom().RandomizeInt(categoryIds));
            product.ListPrice = Env.GetRandom().Randrange(0, 1500) * Env.GetRandom().Random();
            product.StandardPrice = Env.GetRandom().Randrange(0, 1500) * Env.GetRandom().Random();
            result.Add(product);
        }

        return result;
    }

    public void Populate(int size) {
        // Implementation of _populate method
        var products = base.Populate(size);
        // Add additional fields from PopulateGetProductFactories method
        products.AddRange(PopulateGetProductFactories());
    }
}

public partial class SupplierInfo {
    // all the model methods are written here.
    public void Populate(int size) {
        // Implementation of _populate method
        var supplierInfos = base.Populate(size);
        // Set Company, ProductTemplate and other fields
        // ...
    }
}
