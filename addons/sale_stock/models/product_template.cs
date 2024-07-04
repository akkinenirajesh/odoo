csharp
public partial class Sale_ProductTemplate 
{
    public void ComputeExpensePolicy()
    {
        if (this.Type == "Storable")
        {
            this.ExpensePolicy = "no";
        }
    }

    public void ComputeServiceType()
    {
        if (this.Type == "Storable")
        {
            this.ServiceType = "manual";
        }
    }
}
