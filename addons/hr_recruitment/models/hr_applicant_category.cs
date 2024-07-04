csharp
public partial class ApplicantCategory
{
    public override string ToString()
    {
        return Name;
    }

    public int GetDefaultColor()
    {
        return new Random().Next(1, 12);
    }
}
