C#
public partial class Mrp.StockMove
{
    public virtual void ComputeManualConsumption()
    {
        if (this != this.GetOrigin())
        {
            this.ManualConsumption = this.GetOrigin().ManualConsumption;
        }
        else
        {
            this.ManualConsumption = IsManualConsumption();
        }
    }

    public virtual void ComputeIsDone()
    {
        this.IsDone = (this.State == "done" || this.State == "cancel");
    }

    public virtual void ComputeUnitFactor()
    {
        var mo = this.RawMaterialProductionId ?? this.ProductionId;
        if (mo != null)
        {
            this.UnitFactor = this.ProductUomQty / ((mo.ProductQty - mo.QtyProduced) ?? 1);
        }
        else
        {
            this.UnitFactor = 1.0;
        }
    }

    public virtual void ComputeShouldConsumeQty()
    {
        var mo = this.RawMaterialProductionId;
        if (mo != null && this.ProductUom != null)
        {
            this.ShouldConsumeQty = (mo.QtyProducing - mo.QtyProduced) * this.UnitFactor;
        }
        else
        {
            this.ShouldConsumeQty = 0;
        }
    }

    public virtual void ComputeDescriptionBomLine()
    {
        if (this.BomLineId == null) return;

        var bom = this.BomLineId.BomId;
        if (bom == null || bom.Type != "phantom") return;

        var lineIds = bom.BomLineIds.Select(l => l.Id).ToList();
        var total = lineIds.Count;
        var name = bom.DisplayName;

        for (var i = 0; i < lineIds.Count; i++)
        {
            var lineId = lineIds[i];
            this.DescriptionBomLine = $"{name} - {i + 1}/{total}";
        }
    }

    public virtual bool IsManualConsumption()
    {
        return this.BomLineId != null && this.BomLineId.ManualConsumption;
    }

    public virtual void _AdjustProcureMethod()
    {
        // ... implementation
    }

    public virtual void _RunProcurement(Dictionary<long, double> oldQties)
    {
        // ... implementation
    }

    public virtual void _ActionAssign(bool forceQty)
    {
        // ... implementation
    }

    public virtual void _ActionConfirm(bool merge, Mrp.StockMove mergeInto)
    {
        // ... implementation
    }

    public virtual IEnumerable<Mrp.StockMove> ActionExplode()
    {
        // ... implementation
    }

    public virtual void _ActionCancel()
    {
        // ... implementation
    }

    public virtual Dictionary<string, object> _PrepareMoveSplitVals(double qty)
    {
        // ... implementation
    }

    public virtual string _PrepareProcurementOrigin()
    {
        // ... implementation
    }

    public virtual Dictionary<string, object> _PreparePhantomMoveValues(Mrp.BomLine bomLine, double productQty, double quantityDone)
    {
        // ... implementation
    }

    public virtual IEnumerable<Dictionary<string, object>> _GenerateMovePhantom(Mrp.BomLine bomLine, double productQty, double quantityDone)
    {
        // ... implementation
    }

    public virtual bool _IsConsuming()
    {
        // ... implementation
    }

    public virtual Dictionary<string, object> _GetBackorderMoveVals()
    {
        // ... implementation
    }

    public virtual object _GetSourceDocument()
    {
        // ... implementation
    }

    public virtual IEnumerable<(object, object, bool)> _GetUpstreamDocumentsAndResponsibles(bool visited)
    {
        // ... implementation
    }

    public virtual IEnumerable<object> _DelayAlertGetDocuments()
    {
        // ... implementation
    }

    public virtual bool _ShouldBypassReservation()
    {
        // ... implementation
    }

    public virtual bool _ShouldBypassSetQtyProducing()
    {
        // ... implementation
    }

    public virtual IEnumerable<object> _KeyAssignPicking()
    {
        // ... implementation
    }

    public virtual IEnumerable<string> _PrepareMergeMovesDistinctFields()
    {
        // ... implementation
    }

    public virtual IEnumerable<string> _PrepareMergeNegativeMovesExcludedDistinctFields()
    {
        // ... implementation
    }

    public virtual double _ComputeKitQuantities(long productId, double kitQty, Mrp.Bom kitBom, Dictionary<string, Func<Mrp.StockMove, bool>> filters)
    {
        // ... implementation
    }

    public virtual void _UpdateCandidateMovesList(HashSet<Mrp.StockMove> candidateMovesSet)
    {
        // ... implementation
    }

    public virtual Dictionary<string, object> _PrepareProcurementValues()
    {
        // ... implementation
    }

    public virtual Dictionary<string, object> ActionOpenReference()
    {
        // ... implementation
    }

    public virtual string _GetRelevantStateAmongMoves()
    {
        // ... implementation
    }

    // ... other methods
}
