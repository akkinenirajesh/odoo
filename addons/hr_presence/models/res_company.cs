csharp
public partial class ResCompany
{
    public override string ToString()
    {
        // Assuming there's a Name field in the base ResCompany model
        return Name;
    }

    // You can add any additional methods or properties here
    public void ComputePresence()
    {
        // Logic to compute presence
        HrPresenceLastComputeDate = DateTime.Now;
        // Save the changes
        Env.Save(this);
    }
}
