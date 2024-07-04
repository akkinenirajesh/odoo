csharp
public partial class TestUnit {
    public void ComputeSurname() {
        this.Surname = this.Name ?? "";
    }
    public void CheckValues() {
        if (this.Val1 != this.Val2) {
            throw new Exception("The two values must be equals");
        }
    }
}
