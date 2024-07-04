csharp
public partial class DocumentType
{
    public string FormatDocumentNumber(string documentNumber)
    {
        if (this.Country != Env.Ref("Base.Ec"))
        {
            return base.FormatDocumentNumber(documentNumber);
        }

        if (string.IsNullOrEmpty(documentNumber))
        {
            return null;
        }

        if (this.L10nEcCheckFormat)
        {
            documentNumber = System.Text.RegularExpressions.Regex.Replace(documentNumber, @"\s+", "");
            var match = System.Text.RegularExpressions.Regex.Match(documentNumber, @"(\d{1,3})-(\d{1,3})-(\d{1,9})");

            if (match.Success)
            {
                documentNumber = string.Join("-", match.Groups.Cast<System.Text.RegularExpressions.Group>()
                    .Skip(1)
                    .Select((g, i) => g.Value.PadLeft(i < 2 ? 3 : 9, '0')));
            }
            else
            {
                throw new UserErrorException($"Ecuadorian Document {this.DisplayName} must be like 001-001-123456789");
            }
        }

        return documentNumber;
    }
}
