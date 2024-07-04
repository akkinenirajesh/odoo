csharp
public partial class ContractType
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeCode()
    {
        if (string.IsNullOrEmpty(Code))
        {
            Code = Name;
        }
    }
}
