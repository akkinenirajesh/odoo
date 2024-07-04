csharp
public partial class Uom
{
    public List<string> UnprotectedUomXmlIds()
    {
        // Override
        // When timesheet App is installed, we also need to protect the hour UoM
        // from deletion (and warn in case of modification)
        return new List<string>
        {
            "product_uom_dozen"
        };
    }

    public override string ToString()
    {
        // Implement the string representation of the UoM
        return Name; // Assuming there's a Name property in the base class
    }
}
