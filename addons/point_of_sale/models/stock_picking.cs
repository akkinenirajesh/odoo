csharp
public partial class PointOfSale.StockPicking
{
    public PointOfSale.PosSession PosSessionId { get; set; }
    public PointOfSale.PosOrder PosOrderId { get; set; }

    public virtual PointOfSale.PosSession GetPosSession()
    {
        return Env.Get<PointOfSale.PosSession>().Get(PosSessionId);
    }

    public virtual PointOfSale.PosOrder GetPosOrder()
    {
        return Env.Get<PointOfSale.PosOrder>().Get(PosOrderId);
    }

    public virtual List<Stock.StockMove> GetStockMoves()
    {
        return Env.Get<Stock.StockMove>().Search(new Stock.StockMove { PickingId = this.Id });
    }

    public virtual void PreparePickingVals(Core.Partner partner, Stock.StockPickingType pickingType, Stock.Location locationId, Stock.Location locationDestId)
    {
        this.PartnerId = partner?.Id ?? 0;
        this.UserId = 0;
        this.PickingTypeId = pickingType.Id;
        this.MoveType = "direct";
        this.LocationId = locationId.Id;
        this.LocationDestId = locationDestId.Id;
        this.State = "draft";
    }

    public virtual List<Stock.StockPicking> CreatePickingFromPosOrderLines(Stock.Location locationDestId, List<PointOfSale.PosOrderLine> lines, Stock.StockPickingType pickingType, Core.Partner partner = null)
    {
        List<Stock.StockPicking> pickings = new List<Stock.StockPicking>();
        var stockableLines = lines.Where(l => l.ProductId.Type == "consu" && !Utilities.FloatUtils.IsZero(l.Qty, l.ProductId.UomId.Rounding)).ToList();
        if (stockableLines.Count == 0)
        {
            return pickings;
        }
        var positiveLines = stockableLines.Where(l => l.Qty > 0).ToList();
        var negativeLines = stockableLines.Except(positiveLines).ToList();

        if (positiveLines.Count > 0)
        {
            var locationId = pickingType.DefaultLocationSrcId.Id;
            var positivePicking = Env.Get<PointOfSale.StockPicking>().Create(
                this._PreparePickingVals(partner, pickingType, locationId, locationDestId)
            );

            positivePicking._CreateMoveFromPosOrderLines(positiveLines);
            Env.FlushAll();
            try
            {
                using (Env.Cr.Savepoint())
                {
                    positivePicking._ActionDone();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
            }

            pickings.Add(positivePicking);
        }
        if (negativeLines.Count > 0)
        {
            Stock.StockPickingType returnPickingType;
            Stock.Location returnLocationId;
            if (pickingType.ReturnPickingTypeId != 0)
            {
                returnPickingType = pickingType.ReturnPickingTypeId;
                returnLocationId = returnPickingType.DefaultLocationDestId.Id;
            }
            else
            {
                returnPickingType = pickingType;
                returnLocationId = pickingType.DefaultLocationSrcId.Id;
            }

            var negativePicking = Env.Get<PointOfSale.StockPicking>().Create(
                this._PreparePickingVals(partner, returnPickingType, locationDestId, returnLocationId)
            );
            negativePicking._CreateMoveFromPosOrderLines(negativeLines);
            Env.FlushAll();
            try
            {
                using (Env.Cr.Savepoint())
                {
                    negativePicking._ActionDone();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
            }
            pickings.Add(negativePicking);
        }
        return pickings;
    }

    public virtual Stock.StockMove _PrepareStockMoveVals(PointOfSale.PosOrderLine firstLine, List<PointOfSale.PosOrderLine> orderLines)
    {
        return new Stock.StockMove
        {
            Name = firstLine.Name,
            ProductUom = firstLine.ProductId.UomId.Id,
            PickingId = this.Id,
            PickingTypeId = this.PickingTypeId,
            ProductId = firstLine.ProductId.Id,
            ProductUomQty = Math.Abs(orderLines.Sum(l => l.Qty)),
            LocationId = this.LocationId,
            LocationDestId = this.LocationDestId,
            CompanyId = this.CompanyId.Id,
        };
    }

    public virtual void _CreateMoveFromPosOrderLines(List<PointOfSale.PosOrderLine> lines)
    {
        var linesByProduct = lines.GroupBy(l => l.ProductId.Id).ToList();
        List<Stock.StockMove> moveVals = new List<Stock.StockMove>();
        foreach (var group in linesByProduct)
        {
            var orderLines = Env.Get<PointOfSale.PosOrderLine>().Concat(group.ToList());
            moveVals.Add(this._PrepareStockMoveVals(orderLines[0], orderLines.ToList()));
        }
        var moves = Env.Get<Stock.StockMove>().Create(moveVals);
        var confirmedMoves = moves._ActionConfirm();
        confirmedMoves._AddMlsRelatedToOrder(lines, true);
        confirmedMoves.Picked = true;
        this._LinkOwnerOnReturnPicking(lines);
    }

    public virtual void _LinkOwnerOnReturnPicking(List<PointOfSale.PosOrderLine> lines)
    {
        if (lines[0].OrderId.RefundedOrderId.PickingIds.Count > 0)
        {
            var returnedLinesPicking = lines[0].OrderId.RefundedOrderId.PickingIds;
            Dictionary<(int, int), decimal> returnableQtyByProduct = new Dictionary<(int, int), decimal>();
            foreach (var moveLine in returnedLinesPicking.MoveLineIds)
            {
                returnableQtyByProduct[(moveLine.ProductId.Id, moveLine.OwnerId.Id)] = moveLine.Quantity;
            }
            foreach (var move in this.MoveLineIds)
            {
                foreach (var key in returnableQtyByProduct.Keys)
                {
                    if (move.ProductId.Id == key.Item1 && key.Item2 != 0 && returnableQtyByProduct[key] > 0)
                    {
                        move.OwnerId = key.Item2;
                        returnableQtyByProduct[key] -= move.Quantity;
                    }
                }
            }
        }
    }

    public virtual void _SendConfirmationEmail()
    {
        // Avoid sending Mail/SMS for POS deliveries
        if (this.PickingTypeId != this.PickingTypeId.WarehouseId.PosTypeId.Id)
        {
            Env.Get<Stock.StockPicking>()._SendConfirmationEmail();
        }
    }

    public virtual void _ActionDone()
    {
        Env.Get<Stock.StockPicking>()._ActionDone();
        if (this.PickingTypeId.Code != "outgoing")
        {
            return;
        }
        if (this.PosOrderId.ShippingDate != null && !this.PosOrderId.ToInvoice)
        {
            Dictionary<(Account.Account, Account.Account), decimal> costPerAccount = new Dictionary<(Account.Account, Account.Account), decimal>();
            foreach (var line in this.PosOrderId.Lines)
            {
                if (!line.ProductId.IsStorable || line.ProductId.Valuation != "real_time")
                {
                    continue;
                }
                var outAcc = line.ProductId.CategId.PropertyStockAccountOutputCategId;
                var expAcc = line.ProductId._GetProductAccounts()["expense"];
                if (costPerAccount.ContainsKey((outAcc, expAcc)))
                {
                    costPerAccount[(outAcc, expAcc)] += line.TotalCost;
                }
                else
                {
                    costPerAccount.Add((outAcc, expAcc), line.TotalCost);
                }
            }
            List<Account.AccountMove> moveVals = new List<Account.AccountMove>();
            foreach (var ((outAcc, expAcc), cost) in costPerAccount)
            {
                moveVals.Add(new Account.AccountMove
                {
                    JournalId = this.PosOrderId.SaleJournal.Id,
                    Date = this.PosOrderId.DateOrder,
                    Ref = "pos_order_" + this.PosOrderId.Id,
                    LineIds = new List<Account.AccountMoveLine>
                    {
                        new Account.AccountMoveLine
                        {
                            Name = this.PosOrderId.Name,
                            AccountId = expAcc.Id,
                            Debit = cost,
                            Credit = 0.0,
                        },
                        new Account.AccountMoveLine
                        {
                            Name = this.PosOrderId.Name,
                            AccountId = outAcc.Id,
                            Debit = 0.0,
                            Credit = cost,
                        },
                    },
                });
            }
            var move = Env.Get<Account.AccountMove>().Create(moveVals);
            move.ActionPost();
        }
    }
}

public partial class PointOfSale.StockPickingType
{
    public bool UseCreateLots { get; set; }
    public bool UseExistingLots { get; set; }

    public virtual Stock.Warehouse GetWarehouse()
    {
        return Env.Get<Stock.Warehouse>().Get(WarehouseId);
    }
}

public partial class Procurement.ProcurementGroup
{
    public PointOfSale.PosOrder PosOrderId { get; set; }

    public virtual PointOfSale.PosOrder GetPosOrder()
    {
        return Env.Get<PointOfSale.PosOrder>().Get(PosOrderId);
    }
}

public partial class Stock.StockMove
{
    public bool Picked { get; set; }
    public Stock.StockPicking PickingId { get; set; }

    public virtual List<Stock.StockMoveLine> GetMoveLineIds()
    {
        return Env.Get<Stock.StockMoveLine>().Search(new Stock.StockMoveLine { MoveId = this.Id });
    }

    public virtual Stock.StockPicking GetPicking()
    {
        return Env.Get<Stock.StockPicking>().Get(PickingId);
    }

    public virtual List<Stock.StockMove> _ActionConfirm()
    {
        List<Stock.StockMove> confirmedMoves = new List<Stock.StockMove>();
        foreach (var move in this)
        {
            move.State = "assigned";
            confirmedMoves.Add(move);
        }
        return confirmedMoves;
    }

    public virtual void _AddMlsRelatedToOrder(List<PointOfSale.PosOrderLine> relatedOrderLines, bool areQtiesDone = true)
    {
        var linesData = this._PrepareLinesDataDict(relatedOrderLines);
        var movesToAssign = this.Where(m => !linesData.ContainsKey(m.ProductId.Id) || m.ProductId.Tracking == "none" ||
            (!m.PickingTypeId.UseExistingLots && !m.PickingTypeId.UseCreateLots)).ToList();
        foreach (var move in movesToAssign)
        {
            move.Quantity = move.ProductUomQty;
        }
        var movesRemaining = this.Except(movesToAssign).ToList();
        var existingLots = movesRemaining._CreateProductionLotsForPosOrder(relatedOrderLines);
        List<Stock.StockMoveLine> moveLinesToCreate = new List<Stock.StockMoveLine>();
        List<decimal> mlsQties = new List<decimal>();
        if (areQtiesDone)
        {
            foreach (var move in movesRemaining)
            {
                foreach (var moveLine in move.MoveLineIds)
                {
                    moveLine.Quantity = 0;
                }
                foreach (var line in linesData[move.ProductId.Id]["order_lines"])
                {
                    decimal sumOfLots = 0;
                    foreach (var lot in line.PackLotIds.Where(l => !string.IsNullOrEmpty(l.LotName)).ToList())
                    {
                        decimal qty = line.ProductId.Tracking == "serial" ? 1 : Math.Abs(line.Qty);
                        var mlVals = move._PrepareMoveLineVals(qty);
                        if (existingLots.Count > 0)
                        {
                            var existingLot = existingLots.Where(l => l.ProductId.Id == line.ProductId.Id && l.Name == lot.LotName).FirstOrDefault();
                            if (existingLot != null)
                            {
                                var quant = Env.Get<Stock.Quant>().Search(new Stock.Quant { LotId = existingLot.Id, Quantity = new decimal(0.0) { }, LocationId = new Stock.Location { Id = move.LocationId } }).OrderByDescending(q => q.Id).FirstOrDefault();
                                if (quant != null)
                                {
                                    mlVals.QuantId = quant.Id;
                                }
                                else
                                {
                                    mlVals.LotName = existingLot.Name;
                                }
                            }
                            else
                            {
                                mlVals.LotName = lot.LotName;
                            }
                        }
                        else
                        {
                            mlVals.LotName = lot.LotName;
                        }
                        moveLinesToCreate.Add(mlVals);
                        mlsQties.Add(qty);
                        sumOfLots += qty;
                    }
                }
            }
            Env.Get<Stock.StockMoveLine>().Create(moveLinesToCreate);
        }
        else
        {
            foreach (var move in movesRemaining)
            {
                foreach (var line in linesData[move.ProductId.Id]["order_lines"])
                {
                    foreach (var lot in line.PackLotIds.Where(l => !string.IsNullOrEmpty(l.LotName)).ToList())
                    {
                        decimal qty = line.ProductId.Tracking == "serial" ? 1 : Math.Abs(line.Qty);
                        if (existingLots.Count > 0)
                        {
                            var existingLot = existingLots.Where(l => l.ProductId.Id == line.ProductId.Id && l.Name == lot.LotName).FirstOrDefault();
                            if (existingLot != null)
                            {
                                move._UpdateReservedQuantity(qty, move.LocationId, existingLot.Id);
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }

    public virtual Dictionary<int, Dictionary<string, object>> _PrepareLinesDataDict(List<PointOfSale.PosOrderLine> orderLines)
    {
        Dictionary<int, Dictionary<string, object>> linesData = new Dictionary<int, Dictionary<string, object>>();
        foreach (var product_id in orderLines.GroupBy(l => l.ProductId.Id).Select(g => g.Key).ToList())
        {
            linesData.Add(product_id, new Dictionary<string, object>() { { "order_lines", Env.Get<PointOfSale.PosOrderLine>().Concat(orderLines.Where(l => l.ProductId.Id == product_id).ToList()) } });
        }
        return linesData;
    }

    public virtual List<Stock.StockLot> _CreateProductionLotsForPosOrder(List<PointOfSale.PosOrderLine> lines)
    {
        List<Stock.StockLot> validLots = new List<Stock.StockLot>();
        var moves = this.Where(m => m.PickingTypeId.UseExistingLots).ToList();
        this._CheckCompany();
        if (moves.Count > 0)
        {
            var movesProductIds = moves.Select(m => m.ProductId.Id).ToList();
            var lots = lines.PackLotIds.Where(l => !string.IsNullOrEmpty(l.LotName) && movesProductIds.Contains(l.ProductId.Id)).ToList();
            var lotsData = lots.Select(l => (l.ProductId.Id, l.LotName)).ToList();
            var existingLots = Env.Get<Stock.StockLot>().Search(new Stock.StockLot { CompanyId = this[0].PickingTypeId.CompanyId.Id, ProductId = lines.Where(l => l.ProductId.Id == product_id).FirstOrDefault().ProductId, Name = lots.Where(l => l.ProductId.Id == product_id).FirstOrDefault().LotName });
            //The previous search may return (product_id.id, lot_name) combinations that have no matching in lines.pack_lot_ids.
            foreach (var lot in existingLots)
            {
                if (lotsData.Contains((lot.ProductId.Id, lot.Name)))
                {
                    validLots.Add(lot);
                    lotsData.Remove((lot.ProductId.Id, lot.Name));
                }
            }
            moves = moves.Where(m => m.PickingTypeId.UseCreateLots).ToList();
            if (moves.Count > 0)
            {
                var missingLotValues = new List<Stock.StockLot>();
                foreach (var (lotProductId, lotName) in lotsData)
                {
                    missingLotValues.Add(new Stock.StockLot { CompanyId = this.CompanyId.Id, ProductId = lotProductId, Name = lotName });
                }
                validLots.AddRange(Env.Get<Stock.StockLot>().Create(missingLotValues));
            }
        }
        return validLots;
    }

    public virtual void _CheckCompany()
    {
        if (this.Count > 0 && this.First().PickingTypeId.CompanyId.Id != 0)
        {
            foreach (var move in this)
            {
                if (move.CompanyId.Id == 0)
                {
                    move.CompanyId = this[0].PickingTypeId.CompanyId;
                }
            }
        }
    }

    public virtual Stock.StockMoveLine _PrepareMoveLineVals(decimal qty)
    {
        return new Stock.StockMoveLine
        {
            MoveId = this.Id,
            ProductUom = this.ProductUom,
            LocationId = this.LocationId,
            LocationDestId = this.LocationDestId,
            ProductId = this.ProductId,
            Quantity = qty,
            ProductUomQty = qty,
        };
    }

    public virtual void _UpdateReservedQuantity(decimal qty, Stock.Location locationId, int lotId = 0)
    {
        // Update reserved quantity logic
        // Use Env.Get<Stock.Quant>().UpdateReservedQuantity(qty, locationId, lotId)
    }

    public virtual List<Stock.StockMove> _KeyAssignPicking()
    {
        List<Stock.StockMove> moves = new List<Stock.StockMove>();
        foreach (var move in this)
        {
            moves.Add(move);
        }
        return moves;
    }

    public virtual List<Stock.StockMove> _GetNewPickingValues()
    {
        List<Stock.StockMove> newPickingValues = new List<Stock.StockMove>();
        foreach (var move in this)
        {
            var picking = new Stock.StockPicking
            {
                PosSessionId = move.GroupId.PosOrderId.SessionId.Id,
                PosOrderId = move.GroupId.PosOrderId.Id,
            };
            newPickingValues.Add(picking);
        }
        return newPickingValues;
    }
}
