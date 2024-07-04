csharp
public partial class Location {
    public virtual string Name { get; set; }
    public virtual string CompleteName { get; set; }
    public virtual bool Active { get; set; }
    public virtual string Usage { get; set; }
    public virtual Location LocationId { get; set; }
    public virtual ICollection<Location> ChildIds { get; set; }
    public virtual ICollection<Location> ChildInternalLocationIds { get; set; }
    public virtual string Comment { get; set; }
    public virtual int PosX { get; set; }
    public virtual int PosY { get; set; }
    public virtual int PosZ { get; set; }
    public virtual string ParentPath { get; set; }
    public virtual ResCompany CompanyId { get; set; }
    public virtual bool ScrapLocation { get; set; }
    public virtual bool ReturnLocation { get; set; }
    public virtual bool ReplenishLocation { get; set; }
    public virtual ProductRemoval RemovalStrategyId { get; set; }
    public virtual ICollection<PutawayRule> PutawayRuleIds { get; set; }
    public virtual string Barcode { get; set; }
    public virtual ICollection<Quant> QuantIds { get; set; }
    public virtual int CyclicInventoryFrequency { get; set; }
    public virtual DateTime? LastInventoryDate { get; set; }
    public virtual DateTime? NextInventoryDate { get; set; }
    public virtual ICollection<Warehouse> WarehouseViewIds { get; set; }
    public virtual Warehouse WarehouseId { get; set; }
    public virtual StorageCategory StorageCategoryId { get; set; }
    public virtual ICollection<MoveLine> OutgoingMoveLineIds { get; set; }
    public virtual ICollection<MoveLine> IncomingMoveLineIds { get; set; }
    public virtual double NetWeight { get; set; }
    public virtual double ForecastWeight { get; set; }
    public virtual int Sequence { get; set; }
    public virtual bool IsEmpty { get; set; }

    public virtual void ComputeCompleteName() {
        // TODO: Implement ComputeCompleteName
        this.CompleteName = "";
    }

    public virtual void ComputeChildInternalLocationIds() {
        // TODO: Implement ComputeChildInternalLocationIds
    }

    public virtual void ComputeReplenishLocation() {
        // TODO: Implement ComputeReplenishLocation
    }

    public virtual void ComputeNextInventoryDate() {
        // TODO: Implement ComputeNextInventoryDate
    }

    public virtual void ComputeWarehouseId() {
        // TODO: Implement ComputeWarehouseId
    }

    public virtual void ComputeWeight() {
        // TODO: Implement ComputeWeight
    }

    public virtual void ComputeIsEmpty() {
        // TODO: Implement ComputeIsEmpty
    }

    public virtual void SearchIsEmpty(string operator_, string value) {
        // TODO: Implement SearchIsEmpty
    }

    // Other methods can be added here
}
