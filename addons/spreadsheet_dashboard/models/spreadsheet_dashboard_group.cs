csharp
public partial class SpreadsheetDashboardGroup
{
    public void UnlinkExceptSpreadsheetData()
    {
        var externalIds = this.Env.GetExternalId(this);
        foreach (var group in this)
        {
            var externalId = externalIds[group.Id];
            if (externalId != null && !externalId.StartsWith("__export__"))
            {
                throw new UserError($"You cannot delete {group.Name} as it is used in another module.");
            }
        }
    }
}
