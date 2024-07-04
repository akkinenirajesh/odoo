csharp
public partial class PosOrder {
    public string Cashier { get; set; }
    public Hr.Employee EmployeeId { get; set; }

    public void ComputeCashier() {
        if (this.EmployeeId != null) {
            this.Cashier = this.EmployeeId.Name;
        } else {
            this.Cashier = Env.User.Name;
        }
    }
}
