csharp
public partial class AccountEdiFormat
{
    public override string ToString()
    {
        return Name;
    }

    public Dictionary<string, Func<object>> GetMoveApplicability(AccountMove move)
    {
        // Core function for the EDI processing: it first checks whether the EDI format is applicable on a given
        // move, if so, it then returns a dictionary containing the functions to call for this move.
        // TO OVERRIDE
        return new Dictionary<string, Func<object>>();
    }

    public bool NeedsWebServices()
    {
        // Indicate if the EDI must be generated asynchronously through to some web services.
        // TO OVERRIDE
        return false;
    }

    public bool IsCompatibleWithJournal(AccountJournal journal)
    {
        // Indicate if the EDI format should appear on the journal passed as parameter to be selected by the user.
        // TO OVERRIDE
        return journal.Type == "Sale";
    }

    public bool IsEnabledByDefaultOnJournal(AccountJournal journal)
    {
        return true;
    }

    public List<string> CheckMoveConfiguration(AccountMove move)
    {
        // Checks the move and relevant records for potential error (missing data, etc).
        // TO OVERRIDE
        return new List<string>();
    }

    public void PrepareInvoiceReport(PdfWriter pdfWriter, AccountEdiDocument ediDocument)
    {
        // Prepare invoice report to be printed.
        // TO OVERRIDE
    }

    public static string FormatErrorMessage(string errorTitle, List<string> errors)
    {
        string bulletListMsg = string.Join("", errors.Select(msg => $"<li>{System.Web.HttpUtility.HtmlEncode(msg)}</li>"));
        return $"{errorTitle}<ul>{bulletListMsg}</ul>";
    }
}
