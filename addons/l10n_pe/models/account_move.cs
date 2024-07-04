C#
public partial class AccountMove {
    public virtual void InverseL10nLatamDocumentNumber() {
        if (Env.Is(this.JournalId.Type, "purchase")
            && Env.Is(this.L10nLatamDocumentTypeId.Code, "01", "03", "07", "08")
            && !string.IsNullOrEmpty(this.L10nLatamDocumentNumber)
            && this.L10nLatamDocumentNumber.Contains('-')
            && Env.Is(this.L10nLatamDocumentTypeId.CountryId.Code, "PE")) {
            string[] number = this.L10nLatamDocumentNumber.Split('-');
            this.L10nLatamDocumentNumber = $"{number[0]}-{number[1].PadLeft(8, '0')}";
        }
    }

    public virtual List<object> GetL10nLatamDocumentsDomain() {
        List<object> result = base.GetL10nLatamDocumentsDomain();
        if (Env.Is(this.CompanyId.CountryId.Code, "PE")
            && !Env.Is(this.JournalId.L10nLatamUseDocuments, true)
            && Env.Is(this.JournalId.Type, "sale")) {
            return result;
        }
        result.Add(new { Code = new List<string> { "01", "03", "07", "08", "20", "40" } });
        if (Env.Is(this.PartnerId.L10nLatamIdentificationTypeId.L10nPeVatCode, "6")
            && Env.Is(this.MoveType, "out_invoice")) {
            result.Add(new { Id = new List<int> {
                Env.Ref("l10n_pe.document_type08b").Id,
                Env.Ref("l10n_pe.document_type02").Id,
                Env.Ref("l10n_pe.document_type07b").Id
            } });
        }
        return result;
    }
}

public partial class AccountMoveLine {
    public virtual void AutoInit() {
        if (Env.ColumnExists("account_move_line", "l10n_pe_group_id")) {
            Env.ExecuteNonQuery("UPDATE account_move_line SET l10n_pe_group_id = account.group_id FROM account_account account WHERE account.id = line.account_id");
        }
    }
}
