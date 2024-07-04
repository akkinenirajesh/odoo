csharp
public partial class UoM
{
    public override string ToString()
    {
        // Assuming there's a Name field in the base UoM class
        return Name;
    }

    // You can add any additional methods or properties specific to this extension here
    public string GetHungarianEdiCode()
    {
        return L10nHuEdiCode?.ToString() ?? "Not specified";
    }
}
