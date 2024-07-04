C#
public partial class UtmTag {
    public int Color { get; set; }
    public string Name { get; set; }

    public int DefaultColor() {
        return new Random().Next(1, 12);
    }
}
