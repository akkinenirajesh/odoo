C#
public partial class MrpRoutingWorkcenter {
    public float TotalCostPerHour { get; set; }
    public Workcenter WorkcenterId { get; set; }
    public string Name { get; set; }

    public float _TotalCostPerHour() {
        return this.WorkcenterId.CostsHour;
    }
}
