csharp
public partial class MrpRoutingWorkcenter
{
    // All the model methods are written here.
    public void ComputeTimeComputedOn()
    {
        this.TimeComputedOn = (this.TimeMode != "Manual")
            ? $"{this.TimeModeBatch} work orders"
            : string.Empty;
    }

    public void ComputeTimeCycle()
    {
        if (this.TimeMode == "Manual")
        {
            this.TimeCycle = this.TimeCycleManual;
            return;
        }

        var workOrders = Env.Search<Mrp.Workorder>(new List<object[]> {
            new object[] { "OperationId", "=", this.Id },
            new object[] { "QtyProduced", ">", 0 },
            new object[] { "State", "=", "Done" }
        }, new List<object[]> {
            new object[] { "DateFinished", "desc" },
            new object[] { "Id", "desc" }
        }, this.TimeModeBatch);

        var totalDuration = 0.0;
        var cycleNumber = 0;
        foreach (var workOrder in workOrders)
        {
            totalDuration += workOrder.Duration;
            var capacity = workOrder.WorkcenterId.GetCapacity(workOrder.ProductId);
            cycleNumber += Math.Ceiling(workOrder.QtyProduced / (capacity ?? 1.0));
        }

        this.TimeCycle = (cycleNumber > 0) ? (totalDuration / cycleNumber) : this.TimeCycleManual;
    }

    public void ComputeWorkorderCount()
    {
        var workOrders = Env.Search<Mrp.Workorder>(new List<object[]> {
            new object[] { "OperationId", "in", this.Id },
            new object[] { "State", "=", "Done" }
        }, new List<object[]> {
            new object[] { "OperationId", "count" }
        });
        this.WorkorderCount = workOrders.Length;
    }

    public void CheckNoCyclicDependencies()
    {
        if (this.HasCycle("BlockedByOperationIds"))
        {
            throw new Exception("You cannot create cyclic dependency.");
        }
    }

    public void Archive()
    {
        var bomLines = Env.Search<Mrp.BomLine>(new List<object[]> {
            new object[] { "OperationId", "in", this.Id }
        });
        bomLines.ForEach(bomLine => bomLine.OperationId = null);
        this.BomId.SetOutdatedBomInProductions();
        base.Archive();
    }

    public void Unarchive()
    {
        this.BomId.SetOutdatedBomInProductions();
        base.Unarchive();
    }

    public Dictionary<string, object> CopyToBom()
    {
        var bomId = Env.Context.Get<int>("bomId");
        this.Copy(new Dictionary<string, object>() {
            { "BomId", bomId }
        });
        return new Dictionary<string, object>() {
            { "viewMode", "form" },
            { "resModel", "Mrp.Bom" },
            { "views", new object[] { new object[] { false, "form" } } },
            { "type", "ir.actions.act_window" },
            { "resId", bomId }
        };
    }

    public Dictionary<string, object> CopyExistingOperations()
    {
        return new Dictionary<string, object>() {
            { "type", "ir.actions.act_window" },
            { "name", "Select Operations to Copy" },
            { "resModel", "Mrp.MrpRoutingWorkcenter" },
            { "viewMode", "tree,form" },
            { "domain", new List<object[]> {
                new object[] { "or",
                    new object[] { "BomId", "=", null },
                    new object[] { "BomId.Active", "=", true }
                }
            } },
            { "context", new Dictionary<string, object>() {
                { "bomId", Env.Context.Get<int>("bomId") },
                { "treeViewRef", "mrp.mrp_routing_workcenter_copy_to_bom_tree_view" }
            } }
        };
    }

    public bool SkipOperationLine(Product.Product product)
    {
        // skip operation line if archived
        if (!this.Active)
        {
            return true;
        }

        if (product.GetType().Name == "ProductTemplate")
        {
            return false;
        }

        return !product.MatchAllVariantValues(this.BomProductTemplateAttributeValueIds);
    }
}
