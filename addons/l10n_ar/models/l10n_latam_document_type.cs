csharp
public partial class L10nLatamDocumentType
{
    public string FormatDocumentNumber(string documentNumber)
    {
        if (this.Country.Code != "AR")
        {
            // Call the base implementation
            return base.FormatDocumentNumber(documentNumber);
        }

        if (string.IsNullOrEmpty(documentNumber))
        {
            return null;
        }

        string errorMsg = "'{0}' is not a valid value for '{1}'.<br/>{2}";

        if (string.IsNullOrEmpty(this.Code))
        {
            return documentNumber;
        }

        // Import Dispatch Number Validator
        if (this.Code == "66" || this.Code == "67")
        {
            if (documentNumber.Length != 16)
            {
                throw new UserException(string.Format(errorMsg, documentNumber, this.ToString(), "The number of import Dispatch must be 16 characters"));
            }
            return documentNumber;
        }

        // Invoice Number Validator (For Eg: 123-123)
        bool failed = false;
        string[] args = documentNumber.Split('-');
        if (args.Length != 2)
        {
            failed = true;
        }
        else
        {
            string pos = args[0];
            string number = args[1];
            if (pos.Length > 5 || !int.TryParse(pos, out _))
            {
                failed = true;
            }
            else if (number.Length > 8 || !int.TryParse(number, out _))
            {
                failed = true;
            }
            documentNumber = $"{pos.PadLeft(5, '0')}-{number.PadLeft(8, '0')}";
        }
        if (failed)
        {
            throw new UserException(string.Format(errorMsg, documentNumber, this.ToString(), 
                "The document number must be entered with a dash (-) and a maximum of 5 characters for the first part " +
                "and 8 for the second. The following are examples of valid numbers:\n* 1-1\n* 0001-00000001\n* 00001-00000001"));
        }

        return documentNumber;
    }

    public override string ToString()
    {
        // Implement the string representation of the object
        return this.Name; // Assuming there's a Name field in the model
    }
}
