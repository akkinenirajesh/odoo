csharp
public partial class LunchProductCategory
{
    // All methods are written here.
    public void CreateLunchProductCategories(int counter)
    {
        // Example using Env to access services.
        // var companyService = Env.GetService<ICompanyService>(); 
        // companyService.CreateCompany(name, ...);
        var lunchProductCategory = new LunchProductCategory { Name = $"lunch_product_category_{counter}" };
        // ... add additional logic for company using Env here
        Env.Add(lunchProductCategory);
    }
}
