csharp
public partial class Message
{
    public void ComputeAccountAuditLogPreview()
    {
        var moveMessages = this.Filtered(m => m.Model == "account.move" && m.ResId != 0);
        foreach (var message in moveMessages)
        {
            string title = message.Subject ?? message.Preview;
            var trackingValueIds = message.Sudo().TrackingValueIds.FilterHasFieldAccess(Env);
            
            if (string.IsNullOrEmpty(title) && trackingValueIds.Any())
            {
                title = "Updated";
            }
            if (string.IsNullOrEmpty(title) && message.SubtypeId != null && !message.SubtypeId.Internal)
            {
                title = message.SubtypeId.DisplayName;
            }
            
            var auditLogPreview = $"<div>{title ?? ""}</div>";
            auditLogPreview += string.Join("<br>", trackingValueIds.TrackingValueFormat().Select(fmt =>
                $"{fmt.OldValue.Value} <i class='o_TrackingValue_separator fa fa-long-arrow-right mx-1 text-600' title='Changed' role='img' aria-label='Changed'></i>{fmt.NewValue.Value} ({fmt.ChangedField})"
            ));
            
            message.AccountAuditLogPreview = auditLogPreview;
        }
    }

    public void ComputeAccountAuditLogMoveId()
    {
        var moveMessages = this.Filtered(m => m.Model == "account.move" && m.ResId != 0);
        if (moveMessages.Any())
        {
            var moves = Env.GetModel<Account.Move>().Sudo().Search(new[]
            {
                new[] { "Id", "in", moveMessages.Select(m => m.ResId).Distinct().ToList() },
                new[] { "Company.CheckAccountAuditTrail", "=", true }
            });

            foreach (var message in moveMessages)
            {
                message.AccountAuditLogActivated = moves.Any(m => m.Id == message.ResId);
                message.AccountAuditLogMoveId = message.AccountAuditLogActivated ? message.ResId : 0;
            }
        }
    }

    public List<object> SearchAccountAuditLogMoveId(string @operator, object value)
    {
        if (new[] { "=", "like", "ilike", "!=", "not ilike", "not like" }.Contains(@operator) && value is string strValue)
        {
            var resIdDomain = new[] { "ResId", "in", Env.GetModel<Account.Move>().NameSearch(strValue, @operator: @operator) };
            return new List<object> { new[] { "Model", "=", "account.move" }, resIdDomain };
        }
        else if (new[] { "in", "!=", "not in" }.Contains(@operator))
        {
            var resIdDomain = new[] { "ResId", @operator, value };
            return new List<object> { new[] { "Model", "=", "account.move" }, resIdDomain };
        }
        else
        {
            throw new UserException("Operation not supported");
        }
    }

    public List<object> SearchAccountAuditLogActivated(string @operator, object value)
    {
        if (!new[] { "=", "!=" }.Contains(@operator) || !(value is bool))
        {
            throw new UserException("Operation not supported");
        }

        var moveQuery = Env.GetModel<Account.Move>().Search(new[] { new[] { "Company.CheckAccountAuditTrail", @operator, value } });
        return new List<object>
        {
            new[] { "Model", "=", "account.move" },
            new[] { "ResId", "in", moveQuery.Select(m => m.Id).ToList() }
        };
    }
}
