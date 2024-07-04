csharp
public partial class StockScrap
{
    public void ComputeProductUom()
    {
        this.ProductUom = this.Product.Uom;
    }

    public void ComputeLocation()
    {
        if (this.Picking != null)
        {
            this.Location = this.Picking.LocationDest;
        }
        else
        {
            var groups = Env.GetModel("Stock.Warehouse").ReadGroup(
                new[] { new Domain("Company", "in", new[] { this.Company.Id }) },
                new[] { "Company" },
                new[] { new Aggregate("LotStock", "array_agg") }
            );
            var locationsPerCompany = groups.ToDictionary(
                g => g.Company.Id,
                g => g.LotStock.FirstOrDefault());
            this.Location = locationsPerCompany[this.Company.Id];
        }
    }

    public void ComputeScrapLocation()
    {
        var groups = Env.GetModel("Stock.Location").ReadGroup(
            new[] { new Domain("Company", "in", new[] { this.Company.Id }), new Domain("ScrapLocation", "=", true) },
            new[] { "Company" },
            new[] { new Aggregate("Id", "min") }
        );
        var locationsPerCompany = groups.ToDictionary(
            g => g.Company.Id,
            g => g.Id.FirstOrDefault());
        this.ScrapLocation = locationsPerCompany[this.Company.Id];
    }

    public void ComputeScrapQty()
    {
        this.ScrapQty = 1;
        if (this.MoveIds.Any())
        {
            this.ScrapQty = this.MoveIds[0].Quantity;
        }
    }

    public void OnChangeSerialNumber()
    {
        if (this.Product.Tracking == "serial" && this.Lot != null)
        {
            var message = Env.GetModel("Stock.Quant").CheckSerialNumber(
                this.Product,
                this.Lot,
                this.Company,
                this.Location,
                this.Picking.LocationDest);
            if (!string.IsNullOrEmpty(message))
            {
                if (message.Contains("recommended location"))
                {
                    this.Location = message.Split(new[] { "recommended location: " }, StringSplitOptions.RemoveEmptyEntries)[1];
                }
                Env.Notify(message);
            }
        }
    }

    public void UnlinkExceptDone()
    {
        if (this.State == "done")
        {
            Env.Notify("You cannot delete a scrap which is done.");
        }
    }

    public StockMove PrepareMoveValues()
    {
        return new StockMove
        {
            Name = this.Name,
            Origin = this.Origin ?? this.Picking.Name ?? this.Name,
            Company = this.Company,
            Product = this.Product,
            ProductUom = this.ProductUom,
            State = "draft",
            Quantity = this.ScrapQty,
            Location = this.Location,
            Scrapped = true,
            Scrap = this,
            LocationDest = this.ScrapLocation,
            MoveLineIds = new[] {
                new StockMoveLine
                {
                    Product = this.Product,
                    ProductUom = this.ProductUom,
                    Quantity = this.ScrapQty,
                    Location = this.Location,
                    LocationDest = this.ScrapLocation,
                    Package = this.Package,
                    Owner = this.Owner,
                    Lot = this.Lot
                }
            },
            Picked = true,
            Picking = this.Picking
        };
    }

    public void DoScrap()
    {
        this.Name = Env.GetModel("Ir.Sequence").NextByCode("stock.scrap");
        if (string.IsNullOrEmpty(this.Name))
        {
            this.Name = "New";
        }

        var move = Env.GetModel("Stock.Move").Create(PrepareMoveValues());
        move.ActionDone(new { is_scrap = true });
        this.State = "done";
        this.DateDone = DateTime.Now;
        if (this.ShouldReplenish)
        {
            DoReplenish();
        }
    }

    public void DoReplenish()
    {
        var procurement = new Procurement
        {
            Product = this.Product,
            Quantity = this.ScrapQty,
            ProductUom = this.ProductUom,
            Location = this.Location,
            Name = this.Name,
            Origin = this.Name,
            Company = this.Company
        };
        Env.GetModel("Procurement.Group").Run(new[] { procurement });
    }

    public Action ActionGetStockPicking()
    {
        var action = Env.GetModel("Ir.Actions.ActWindow").ForXmlId("stock.action_picking_tree_all");
        action.Domain = new[] { new Domain("Id", "=", this.Picking.Id) };
        return action;
    }

    public Action ActionGetStockMoveLines()
    {
        var action = Env.GetModel("Ir.Actions.ActWindow").ForXmlId("stock.stock_move_line_action");
        action.Domain = new[] { new Domain("Move", "in", this.MoveIds.Select(m => m.Id).ToArray()) };
        return action;
    }

    public bool ShouldCheckAvailableQty()
    {
        return this.Product.IsStorable;
    }

    public bool CheckAvailableQty()
    {
        if (!ShouldCheckAvailableQty())
        {
            return true;
        }

        var precision = Env.GetModel("Decimal.Precision").GetPrecision("Product Unit of Measure");
        var availableQty = Env.GetModel("Product.Product").QtyAvailable(new
        {
            location = this.Location.Id,
            lot_id = this.Lot.Id,
            package_id = this.Package.Id,
            owner_id = this.Owner.Id
        });
        var scrapQty = this.ProductUom.ComputeQuantity(this.ScrapQty, this.Product.Uom);
        return availableQty >= scrapQty;
    }

    public Action ActionValidate()
    {
        if (this.ScrapQty <= 0)
        {
            Env.Notify("You can only enter positive quantities.");
            return null;
        }

        if (CheckAvailableQty())
        {
            DoScrap();
            return null;
        }

        var context = new
        {
            default_product_id = this.Product.Id,
            default_location_id = this.Location.Id,
            default_scrap_id = this.Id,
            default_quantity = this.ProductUom.ComputeQuantity(this.ScrapQty, this.Product.Uom),
            default_product_uom_name = this.Product.UomName
        };
        return new Action
        {
            Name = this.Product.DisplayName + ": Insufficient Quantity To Scrap",
            ViewMode = ViewMode.Form,
            ResModel = "Stock.Warn.Insufficient.Qty.Scrap",
            ViewId = Env.GetModel("Ir.Actions.ActWindow").ForXmlId("stock.stock_warn_insufficient_qty_scrap_form_view").Id,
            Type = ActionType.ActWindow,
            Context = context,
            Target = TargetType.New
        };
    }
}
