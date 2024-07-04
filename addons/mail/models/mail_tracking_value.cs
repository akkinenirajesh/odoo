C#
public partial class MailTrackingValue 
{
    public MailTrackingValue() { }
    public MailTrackingValue(BuviContext context) { }

    public virtual void FilterHasFieldAccess(BuviContext env) {
        // TODO: Implement method
    }

    public virtual void FilterFreeFieldAccess() {
        // TODO: Implement method
    }

    public virtual void CreateTrackingValues(object initialValue, object newValue, string colName, object colInfo, object record) {
        // TODO: Implement method
    }

    public virtual object TrackingValueFormat() {
        // TODO: Implement method
    }

    public virtual object TrackingValueFormatModel(string model) {
        // TODO: Implement method
    }

    public virtual object FormatDisplayValue(string fieldType, bool new) {
        // TODO: Implement method
    }
}
