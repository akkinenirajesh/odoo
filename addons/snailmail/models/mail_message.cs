C#
public partial class Snailmail.MailMessage
{
    public void ComputeSnailmailError()
    {
        if (this.MessageType != "snailmail" || this.LetterIds.Count == 0)
        {
            this.SnailmailError = false;
            return;
        }
        this.SnailmailError = this.LetterIds[0].State == "error";
    }

    public void CancelLetter()
    {
        foreach (var letter in this.LetterIds)
        {
            letter.Cancel();
        }
    }

    public void SendLetter()
    {
        foreach (var letter in this.LetterIds)
        {
            letter.SnailmailPrint();
        }
    }

    public void SearchSnailmailError(string operator, bool operand)
    {
        if (operator == "=" && operand)
        {
            // Implement the search logic for "letter_ids.state" equals 'error' and user_id equals current user
        }
        else
        {
            // Implement the search logic for "letter_ids.state" not equals 'error' or user_id not equals current user
        }
    }
}
