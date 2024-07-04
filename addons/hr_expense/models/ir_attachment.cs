csharp
public partial class IrAttachment
{
    public override IrAttachment Create(IrAttachment attachment)
    {
        var createdAttachment = base.Create(attachment);
        
        if (Env.Context.GetValueOrDefault("sync_attachment", true))
        {
            var expensesAttachments = createdAttachment.Where(att => att.ResModel == "Hr.Expense");
            if (expensesAttachments.Any())
            {
                var expenses = Env.Get<HrExpense>().Browse(expensesAttachments.Select(att => att.ResId));
                foreach (var expense in expenses.Where(e => e.SheetId != null))
                {
                    var checksums = new HashSet<string>(expense.SheetId.AttachmentIds.Select(att => att.Checksum));
                    foreach (var attachment in expense.AttachmentIds.Where(att => !checksums.Contains(att.Checksum)))
                    {
                        attachment.Copy(new IrAttachment
                        {
                            ResModel = "Hr.ExpenseSheet",
                            ResId = expense.SheetId.Id
                        });
                    }
                }
            }
        }
        
        return createdAttachment;
    }

    public override void Unlink()
    {
        if (Env.Context.GetValueOrDefault("sync_attachment", true))
        {
            var attachmentsToUnlink = new List<IrAttachment>();
            var expensesAttachments = this.Where(att => att.ResModel == "Hr.Expense");
            
            if (expensesAttachments.Any())
            {
                var expenses = Env.Get<HrExpense>().Browse(expensesAttachments.Select(att => att.ResId));
                foreach (var expense in expenses.Where(e => e.Exists() && e.SheetId != null))
                {
                    var checksums = new HashSet<string>(expense.AttachmentIds.Select(att => att.Checksum));
                    attachmentsToUnlink.AddRange(expense.SheetId.AttachmentIds.Where(att => checksums.Contains(att.Checksum)));
                }
            }

            var sheetsAttachments = this.Where(att => att.ResModel == "Hr.ExpenseSheet");
            if (sheetsAttachments.Any())
            {
                var sheets = Env.Get<HrExpenseSheet>().Browse(sheetsAttachments.Select(att => att.ResId));
                foreach (var sheet in sheets.Where(s => s.Exists()))
                {
                    var checksums = new HashSet<string>(sheet.AttachmentIds.Intersect(sheetsAttachments).Select(att => att.Checksum));
                    attachmentsToUnlink.AddRange(sheet.ExpenseLineIds.SelectMany(e => e.AttachmentIds).Where(att => checksums.Contains(att.Checksum)));
                }
            }

            foreach (var attachment in attachmentsToUnlink)
            {
                attachment.Unlink();
            }
        }

        base.Unlink();
    }
}
