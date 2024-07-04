csharp
public partial class SpreadsheetDashboardShare
{
    public string ComputeFullUrl()
    {
        return string.Format("{0}/dashboard/share/{1}/{2}", Env.GetBaseUrl(), this.Id, this.AccessToken);
    }

    public string ActionGetShareUrl(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("ExcelFiles"))
        {
            var excelZip = ZipXslxFiles(vals["ExcelFiles"]);
            vals.Remove("ExcelFiles");
            vals["ExcelExport"] = Convert.ToBase64String(excelZip);
        }
        var share = Env.Create("Spreadsheet.SpreadsheetDashboardShare", vals);
        return share.FullUrl;
    }

    private byte[] ZipXslxFiles(object excelFiles)
    {
        // Implement your logic to zip excel files here
        throw new NotImplementedException();
    }

    public bool CheckToken(string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            return false;
        }
        return accessToken == this.AccessToken;
    }

    public void CheckDashboardAccess(string accessToken)
    {
        if (!CheckToken(accessToken) || !Env.IsAccessAllowed("read", "Spreadsheet.SpreadsheetDashboard", this.DashboardId.Id))
        {
            throw new ForbiddenException("You don't have access to this dashboard.");
        }
    }
}
