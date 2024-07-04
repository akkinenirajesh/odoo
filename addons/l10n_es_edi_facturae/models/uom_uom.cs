csharp
public partial class UoM
{
    public override string ToString()
    {
        // Logic to compute the string representation of the object
        return Env.GetString("UoM.L10nEsEdiFacturaeUomCode", L10nEsEdiFacturaeUomCode);
    }
}
