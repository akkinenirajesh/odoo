C#
public partial class ResCompany 
{
    // ...

    public int ManufacturingLead { get; set; }

    // ...
}

public partial class Warehouse 
{
    // ...

    public ManufactureSteps ManufactureSteps { get; set; }

    // ...
}

public partial class MrpBom
{
    // ...

    public ResCompany Company { get; set; }
    public ProductTemplate ProductTemplate { get; set; }
    public Product Product { get; set; }
    public int ProductQuantity { get; set; }
    public int Sequence { get; set; }
    public string Code { get; set; }
    public ReadyToProduce ReadyToProduce { get; set; }
    public int ProduceDelay { get; set; }

    // ...
}

public partial class MrpBomLine
{
    // ...

    public MrpBom Bom { get; set; }
    public int Sequence { get; set; }
    public Product Product { get; set; }
    public Uom ProductUom { get; set; }
    public int ProductQuantity { get; set; }

    // ...
}

public partial class MrpWorkcenter
{
    // ...

    public string Name { get; set; }
    public ResCompany Company { get; set; }
    public ResourceCalendar ResourceCalendar { get; set; }
    public bool Active { get; set; }
    public string Code { get; set; }
    public double DefaultCapacity { get; set; }
    public int Sequence { get; set; }
    public int Color { get; set; }
    public double CostsHour { get; set; }
    public double TimeStart { get; set; }
    public double TimeStop { get; set; }
    public int OeeTarget { get; set; }
    public List<MrpWorkcenter> AlternativeWorkcenters { get; set; }

    // ...
}

public partial class MrpRoutingWorkcenter
{
    // ...

    public MrpBom Bom { get; set; }
    public ResCompany Company { get; set; }
    public MrpWorkcenter Workcenter { get; set; }
    public string Name { get; set; }
    public int Sequence { get; set; }
    public TimeMode TimeMode { get; set; }
    public int TimeModeBatch { get; set; }
    public double TimeCycleManual { get; set; }

    // ...
}

public partial class MrpBomByproduct
{
    // ...

    public MrpBom Bom { get; set; }
    public Product Product { get; set; }
    public Uom ProductUom { get; set; }
    public int ProductQuantity { get; set; }

    // ...
}

public partial class MrpProduction
{
    // ...

    public ResCompany Company { get; set; }
    public MrpBom Bom { get; set; }
    public Consumption Consumption { get; set; }
    public Product Product { get; set; }
    public Uom ProductUom { get; set; }
    public int ProductQuantity { get; set; }
    public PickingType PickingType { get; set; }
    public DateTime DateStart { get; set; }
    public Location LocationSource { get; set; }
    public Location LocationDestination { get; set; }
    public Priority Priority { get; set; }

    // ...
}

public partial class StockMove
{
    // ...

    public MrpProduction RawMaterialProduction { get; set; }
    public Location Location { get; set; }
    public Location LocationDestination { get; set; }
    public PickingType PickingType { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public ResCompany Company { get; set; }

    // ...
}
