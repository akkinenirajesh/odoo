csharp
public partial class AccountJournal
{
    public override bool Write(Dictionary<string, object> vals)
    {
        // OVERRIDE
        // Don't allow the user to deactivate an edi format having at least one document to be processed.
        if (vals.ContainsKey("EdiFormatIds"))
        {
            var oldEdiFormatIds = this.EdiFormatIds;
            var res = base.Write(vals);
            var diffEdiFormatIds = oldEdiFormatIds.Except(this.EdiFormatIds).ToList();
            var documents = Env.Set<Account.AccountEdiDocument>().Search(new[]
            {
                ("MoveId.JournalId", "in", new[] { this.Id }),
                ("EdiFormatId", "in", diffEdiFormatIds.Select(e => e.Id).ToArray()),
                ("State", "in", new[] { "to_cancel", "to_send" })
            });

            // If the formats we are unchecking do not need a webservice, we don't need them to be correctly sent
            if (documents.Any(d => d.EdiFormatId.NeedsWebServices()))
            {
                throw new UserError($"Cannot deactivate ({string.Join(", ", documents.Select(d => d.EdiFormatId.DisplayName))}) on this journal because not all documents are synchronized");
            }

            // remove these documents which: do not need a web service & are linked to the edi formats we are unchecking
            if (documents.Any())
            {
                documents.Unlink();
            }
            return res;
        }
        else
        {
            return base.Write(vals);
        }
    }

    public void ComputeCompatibleEdiIds()
    {
        var ediFormats = Env.Set<Account.AccountEdiFormat>().Search(new object[] { });

        var compatibleEdis = ediFormats.Where(e => e.IsCompatibleWithJournal(this)).ToList();
        this.CompatibleEdiIds = compatibleEdis;
    }

    public void ComputeEdiFormatIds()
    {
        var ediFormats = Env.Set<Account.AccountEdiFormat>().Search(new object[] { });

        var protectedEdiFormatsPerJournal = new Dictionary<int, HashSet<int>>();
        if (this.Id != 0)
        {
            var query = @"
                SELECT
                    move.journal_id,
                    ARRAY_AGG(doc.edi_format_id) AS edi_format_ids
                FROM account_edi_document doc
                JOIN account_move move ON move.id = doc.move_id
                WHERE doc.state IN ('to_cancel', 'to_send')
                AND move.journal_id = @JournalId
                GROUP BY move.journal_id
            ";
            var result = Env.Cr.Query<(int JournalId, int[] EdiFormatIds)>(query, new { JournalId = this.Id });
            protectedEdiFormatsPerJournal = result.ToDictionary(
                r => r.JournalId,
                r => new HashSet<int>(r.EdiFormatIds)
            );
        }

        var enabledEdiFormats = ediFormats.Where(e => 
            e.IsCompatibleWithJournal(this) && 
            (e.IsEnabledByDefaultOnJournal(this) || this.EdiFormatIds.Contains(e))
        ).ToList();

        // The existing edi formats that are already in use so we can't remove it.
        var protectedEdiFormatIds = protectedEdiFormatsPerJournal.TryGetValue(this.Id, out var ids) ? ids : new HashSet<int>();
        var protectedEdiFormats = this.EdiFormatIds.Where(e => protectedEdiFormatIds.Contains(e.Id)).ToList();

        this.EdiFormatIds = enabledEdiFormats.Concat(protectedEdiFormats).Distinct().ToList();
    }
}
