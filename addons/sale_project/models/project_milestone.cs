csharp
public partial class SaleProjectMilestone {
    private SaleProjectMilestone _self;

    public SaleProjectMilestone() {
        _self = this;
    }

    public SaleOrderLine _defaultSaleLineId() {
        var projectId = Env.Context.Get("default_project_id");
        if (projectId == null) {
            return null;
        }
        var project = Env.Get("Project.Project").Browse(projectId);
        return Env.Get("Sale.OrderLine").Search(new List<string> {
            "order_id", "=", project.SaleOrderId.ToString(),
            "qty_delivered_method", "=", "milestones"
        }, 1).First();
    }

    public void _computeQuantityPercentage() {
        _self.QuantityPercentage = _self.SaleLineId.ProductUomQty != 0 ? _self.ProductUomQty / _self.SaleLineId.ProductUomQty : 0;
    }

    public void _computeProductUomQty() {
        _self.ProductUomQty = _self.QuantityPercentage != 0 ? _self.QuantityPercentage * _self.SaleLineId.ProductUomQty : _self.SaleLineId.ProductUomQty;
    }

    public void ActionViewSaleOrder() {
        var action = new Dictionary<string, object>() {
            { "type", "ir.actions.act_window" },
            { "name", "Sales Order" },
            { "res_model", "Sale.Order" },
            { "res_id", _self.SaleLineId.OrderId.Id },
            { "view_mode", "form" },
        };
        Env.Action(action);
    }
}
