C#
public partial class MrpProduction {
    public int SaleOrderCount { get; set; }

    public void ComputeSaleOrderCount() {
        this.SaleOrderCount = Env.Model("mrp.production").Search(new List<object> {
            new object[] {
                "procurement_group_id.mrp_production_ids.move_dest_ids.group_id.sale_id", "in", this.ProcurementGroupId.MrpProductionIds.MoveDestIds.GroupId.SaleId
            }
        }).Count;
    }

    public ActionViewSaleOrders() {
        var saleOrders = Env.Model("sale.order").Search(new List<object> {
            new object[] {
                "id", "in", this.ProcurementGroupId.MrpProductionIds.MoveDestIds.GroupId.SaleId
            }
        });

        var action = new Action {
            ResModel = "sale.order",
            Type = "ir.actions.act_window"
        };

        if (saleOrders.Count == 1) {
            action.ViewMode = "form";
            action.ResId = saleOrders[0].Id;
        } else {
            action.Name = string.Format("Sources Sale Orders of {0}", this.Name);
            action.Domain = new List<object> {
                new object[] {
                    "id", "in", saleOrders.Select(s => s.Id).ToArray()
                }
            };
            action.ViewMode = "tree,form";
        }

        return action;
    }
}
