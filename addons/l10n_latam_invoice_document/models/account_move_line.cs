csharp
public partial class AccountMoveLine
{
    public override void AutoInit()
    {
        // Skip the computation of the field `L10nLatamDocumentTypeId` at the module installation
        if (!Env.Cr.ColumnExists("account_move_line", "l10n_latam_document_type_id"))
        {
            Env.Cr.CreateColumn("account_move_line", "l10n_latam_document_type_id", "int4");
        }
        base.AutoInit();
    }

    public Account.L10nLatamDocumentType L10nLatamDocumentTypeId
    {
        get => MoveId?.L10nLatamDocumentTypeId;
        set => MoveId?.L10nLatamDocumentTypeId = value;
    }
}
