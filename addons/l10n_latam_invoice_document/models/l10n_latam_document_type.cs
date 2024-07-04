csharp
public partial class DocumentType
{
    public string FormatDocumentNumber(string documentNumber)
    {
        // Method to be inherited by different localizations. The purpose of this method is to allow:
        // * making validations on the documentNumber. If it is wrong it should raise an exception
        // * format the documentNumber against a pattern and return it
        return documentNumber;
    }

    public override string ToString()
    {
        string name = this.Name;
        if (!string.IsNullOrEmpty(this.Code))
        {
            name = $"({this.Code}) {name}";
        }
        return name;
    }
}
