csharp
public partial class CrmTag {
    public int GetDefaultColor() {
        return new Random().Next(1, 12);
    }
}
