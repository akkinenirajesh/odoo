csharp
public partial class StockPicking
{
    public bool ComputeL10nItShowPrintDdtButton()
    {
        return CountryCode == "IT"
            && State == "done"
            && IsLocked
            && (PickingTypeCode == "outgoing"
                || (MoveIdsWithoutPackage.Any()
                    && MoveIdsWithoutPackage[0].Partner != null
                    && LocationId.Usage == "supplier"
                    && LocationDestId.Usage == "customer"
                   )
                );
    }

    public void ActionDone()
    {
        base.ActionDone();
        if (PickingTypeId.L10nItDdtSequence != null)
        {
            L10nItDdtNumber = PickingTypeId.L10nItDdtSequence.NextById();
        }
    }
}

public partial class StockPickingType
{
    private (string, string) GetDttIrSeqVals(int? warehouseId, string sequenceCode)
    {
        string irSeqName, irSeqPrefix;
        if (warehouseId.HasValue)
        {
            var wh = Env.Get<StockWarehouse>().Browse(warehouseId.Value);
            irSeqName = $"{wh.Name} Sequence {sequenceCode}";
            irSeqPrefix = $"{wh.Code}/{sequenceCode}/DDT";
        }
        else
        {
            irSeqName = $"Sequence {sequenceCode}";
            irSeqPrefix = $"{sequenceCode}/DDT";
        }
        return (irSeqName, irSeqPrefix);
    }

    public override void OnCreate()
    {
        base.OnCreate();
        var company = Company ?? Env.Company;
        if (company.Country.Code == "IT" && Code == "outgoing" && L10nItDdtSequence == null)
        {
            var (irSeqName, irSeqPrefix) = GetDttIrSeqVals(WarehouseId, SequenceCode);
            L10nItDdtSequence = Env.Get<Core.IrSequence>().Create(new Core.IrSequence
            {
                Name = irSeqName,
                Prefix = irSeqPrefix,
                Padding = 5,
                Company = company,
                Implementation = "no_gap"
            });
        }
    }

    public override void OnWrite()
    {
        base.OnWrite();
        if (Changes.ContainsKey(nameof(SequenceCode)) && L10nItDdtSequence != null)
        {
            var warehouse = Changes.ContainsKey(nameof(WarehouseId)) ? (int?)Changes[nameof(WarehouseId)] : WarehouseId;
            var (irSeqName, irSeqPrefix) = GetDttIrSeqVals(warehouse, SequenceCode);
            L10nItDdtSequence.Write(new
            {
                Name = irSeqName,
                Prefix = irSeqPrefix
            });
        }
    }
}
