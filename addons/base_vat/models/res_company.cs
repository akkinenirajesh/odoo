csharp
public partial class ResCompany
{
    public override string ToString()
    {
        // Assuming there's a Name field in the base ResCompany model
        return Name;
    }

    // Example method to toggle VatCheckVies
    public void ToggleVatCheckVies()
    {
        VatCheckVies = !VatCheckVies;
        Env.SaveChanges();
    }
}
