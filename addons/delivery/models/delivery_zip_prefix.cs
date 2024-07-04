csharp
public partial class DeliveryZipPrefix
{
    public override string ToString()
    {
        return Name;
    }

    public override void OnCreate()
    {
        Name = Name.ToUpper();
        base.OnCreate();
    }

    public override void OnWrite()
    {
        if (IsFieldChanged(nameof(Name)))
        {
            Name = Name.ToUpper();
        }
        base.OnWrite();
    }
}
