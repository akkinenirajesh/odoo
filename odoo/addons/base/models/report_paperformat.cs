csharp
public partial class ReportPaperFormat 
{
    public void CheckFormatOrPage()
    {
        if (Env.GetRecords<ReportPaperFormat>().Where(x => x.Format != "custom" && (x.PageWidth != null || x.PageHeight != null)).Any())
        {
            throw new Exception("You can select either a format or a specific page width/height, but not both.");
        }
    }

    public void ComputePrintPageSize()
    {
        if (this.Format != null)
        {
            if (this.Format == "custom")
            {
                this.PrintPageWidth = this.PageWidth;
                this.PrintPageHeight = this.PageHeight;
            }
            else
            {
                var paperSize = Env.GetRecords<PaperSize>().FirstOrDefault(ps => ps.Key == this.Format);
                if (paperSize != null)
                {
                    this.PrintPageWidth = paperSize.Width;
                    this.PrintPageHeight = paperSize.Height;
                }
            }
        }

        if (this.Orientation == "Landscape")
        {
            var temp = this.PrintPageWidth;
            this.PrintPageWidth = this.PrintPageHeight;
            this.PrintPageHeight = temp;
        }
    }
}
