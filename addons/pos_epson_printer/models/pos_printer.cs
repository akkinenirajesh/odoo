C#
public partial class PosPrinter
{
    public void ConstrainsEpsonPrinterIP()
    {
        if (this.PrinterType == "epson_epos" && string.IsNullOrEmpty(this.EpsonPrinterIP))
        {
            throw new Exception("Epson Printer IP Address cannot be empty.");
        }
    }

    public List<string> LoadPosDataFields(int configId)
    {
        var params = Env.Call("pos.printer", "_load_pos_data_fields", configId) as List<string>;
        params.Add("EpsonPrinterIP");
        return params;
    }
}
