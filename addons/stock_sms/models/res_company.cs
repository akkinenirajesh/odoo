csharp
public partial class StockSms.Company {
    public int StockSmsConfirmationTemplateId { get; set; }

    public bool HasReceivedWarningStockSms { get; set; }

    public bool StockMoveSmsValidation { get; set; }

    public int _default_confirmation_sms_picking_template() {
        try {
            return Env.Ref("stock_sms.sms_template_data_stock_delivery").Id;
        }
        catch (Exception) {
            return 0;
        }
    }
}
