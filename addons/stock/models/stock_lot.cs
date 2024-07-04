csharp
public partial class StockLot
{
    public void CheckCreate()
    {
        var activePickingId = Env.Context.Get("active_picking_id");
        if (activePickingId != null)
        {
            var pickingId = Env.StockPicking.Browse(activePickingId);
            if (pickingId != null && !pickingId.PickingTypeId.UseCreateLots)
            {
                throw new UserError("You are not allowed to create a lot or serial number with this operation type. To change this, go on the operation type and tick the box \"Create New Lots/Serial Numbers\".");
            }
        }
    }

    public void ComputeCompanyId()
    {
        CompanyId = ProductId.CompanyId;
    }

    public void ComputeDisplayComplete()
    {
        DisplayComplete = this.Id != 0 || Env.Context.Get("display_complete") != null;
    }

    public void ComputeDeliveryIds()
    {
        var deliveryIdsByLot = FindDeliveryIdsByLot();
        DeliveryIds = deliveryIdsByLot[this.Id];
        DeliveryCount = DeliveryIds.Count;
    }

    public void ComputeLastDeliveryPartnerId()
    {
        if (ProductId.Tracking != "serial")
        {
            LastDeliveryPartnerId = null;
            return;
        }

        var deliveryIdsByLot = FindDeliveryIdsByLot();
        if (DeliveryIds.Count > 0)
        {
            LastDeliveryPartnerId = Env.StockPicking.Browse(DeliveryIds).OrderByDescending(x => x.DateDone).FirstOrDefault().PartnerId;
        }
        else
        {
            LastDeliveryPartnerId = null;
        }
    }

    public void ComputeSingleLocation()
    {
        var quants = QuantIds.Where(q => q.Quantity > 0);
        LocationId = quants.Count() == 1 ? quants.FirstOrDefault().LocationId : null;
    }

    public void SetSingleLocation()
    {
        var quants = QuantIds.Where(q => q.Quantity > 0);
        if (quants.Count() == 1)
        {
            var unpack = quants.FirstOrDefault().PackageId.QuantIds.Count() > 1;
            quants.MoveQuants(LocationDestId: LocationId, Message: "Lot/Serial Number Relocated", Unpack: unpack);
        }
        else if (quants.Count() > 1)
        {
            throw new UserError("You can only move a lot/serial to a new location if it exists in a single location.");
        }
    }

    public void CheckUniqueLot()
    {
        var domain = new List<object[]>
        {
            new object[] { "ProductId", "in", new object[] { ProductId.Id } },
            new object[] { "Name", "in", new object[] { Name } }
        };

        var groupby = new List<string> { "CompanyId", "ProductId", "Name" };
        var records = Env.StockLot.ReadGroup(domain, groupby, new List<string> { "__count" }, order: "CompanyId DESC");

        var errorMessages = new List<string>();
        var crossLots = new Dictionary<(long, string), int>();
        foreach (var record in records)
        {
            var company = record[0];
            var product = record[1];
            var name = record[2];
            var count = record[3];

            if (company == null)
            {
                crossLots[(product, name)] = count;
            }
            else if (company != null && (crossLots.GetValueOrDefault((product, name), 0) + count) > 1 || count > 1)
            {
                errorMessages.Add($" - Product: {product}, Lot/Serial Number: {name}");
            }
        }

        if (errorMessages.Count > 0)
        {
            throw new ValidationError($"The combination of lot/serial number and product must be unique within a company including when no company is defined.\nThe following combinations contain duplicates:\n" + Environment.NewLine + string.Join(Environment.NewLine, errorMessages));
        }
    }

    public void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("CompanyId"))
        {
            if (LocationId.CompanyId != null && vals["CompanyId"] != null && LocationId.CompanyId.Id != (long)vals["CompanyId"])
            {
                throw new UserError("You cannot change the company of a lot/serial number currently in a location belonging to another company.");
            }
        }

        if (vals.ContainsKey("ProductId") && vals["ProductId"] != ProductId.Id)
        {
            var moveLines = Env.StockMoveLine.Search(new List<object[]>
            {
                new object[] { "LotId", "in", new object[] { this.Id } },
                new object[] { "ProductId", "!=", vals["ProductId"] }
            });

            if (moveLines.Count > 0)
            {
                throw new UserError("You are not allowed to change the product linked to a serial or lot number if some stock moves have already been created with that number. This would lead to inconsistencies in your stock.");
            }
        }

        base.Write(vals);
    }

    public List<Dictionary<string, object>> CopyData(Dictionary<string, object> defaultValues = null)
    {
        defaultValues = defaultValues ?? new Dictionary<string, object>();
        var valsList = base.CopyData(defaultValues);

        if (!defaultValues.ContainsKey("Name"))
        {
            for (int i = 0; i < valsList.Count; i++)
            {
                valsList[i]["Name"] = $"(copy of) {Name}";
            }
        }

        return valsList;
    }

    public void ComputeProductQty()
    {
        // We only care for the quants in internal or transit locations.
        var quants = QuantIds.Where(q => q.LocationId.Usage == "internal" || (q.LocationId.Usage == "transit" && q.LocationId.CompanyId != null));
        ProductQty = quants.Sum(q => q.Quantity);
    }

    public List<object[]> SearchProductQty(string operatorString, object value)
    {
        if (!OPERATORS.ContainsKey(operatorString))
        {
            throw new UserError($"Invalid domain operator {operatorString}");
        }

        if (!(value is float or value is int))
        {
            throw new UserError($"Invalid domain right operand '{value}'. It must be of type Integer/Float");
        }

        var domain = new List<object[]>
        {
            new object[] { "LotId", "!=", false },
            new object[] { "|", new object[] { "LocationId.Usage", "=", "internal" }, new object[] { "&", new object[] { "LocationId.Usage", "=", "transit" }, new object[] { "LocationId.CompanyId", "!=", false } } }
        };

        var lotsWithQty = Env.StockQuant.ReadGroup(domain, new List<string> { "LotId" }, new List<string> { "quantity:sum" }, new List<object[]> { new object[] { "quantity:sum", "!=", 0 } });

        var ids = new List<long>();
        var lotIdsWithQty = new List<long>();
        foreach (var lot in lotsWithQty)
        {
            var lotId = (long)lot[0];
            lotIdsWithQty.Add(lotId);
            if (OPERATORS[operatorString]((double)lot[1], (double)value))
            {
                ids.Add(lotId);
            }
        }

        if ((double)value == 0 && operatorString == "=")
        {
            return new List<object[]> { new object[] { "Id", "not in", lotIdsWithQty } };
        }

        if ((double)value == 0 && operatorString == "!=")
        {
            return new List<object[]> { new object[] { "Id", "in", lotIdsWithQty } };
        }

        // check if we need include zero values in result
        var includeZero =
            (double)value < 0 && (operatorString == ">" || operatorString == ">=") ||
            (double)value > 0 && (operatorString == "<" || operatorString == "<=") ||
            (double)value == 0 && (operatorString == ">=" || operatorString == "<=");

        if (includeZero)
        {
            return new List<object[]> { new object[] { "|", new object[] { "Id", "in", ids }, new object[] { "Id", "not in", lotIdsWithQty } } };
        }

        return new List<object[]> { new object[] { "Id", "in", ids } };
    }

    public void ActionLotOpenQuants()
    {
        Env.Context["search_default_lot_id"] = this.Id;
        Env.Context["create"] = false;

        if (Env.User.HasGroup("stock.group_stock_manager"))
        {
            Env.Context["inventory_mode"] = true;
        }

        Env.StockQuant.ActionViewQuants();
    }

    public void ActionLotOpenTransfers()
    {
        if (DeliveryIds.Count == 1)
        {
            Env.StockPicking.ActionView(DeliveryIds.FirstOrDefault().Id, viewMode: "form");
        }
        else
        {
            Env.StockPicking.ActionView(new List<object[]> { new object[] { "Id", "in", DeliveryIds.Select(x => x.Id) } }, viewMode: "tree,form");
        }
    }

    private Dictionary<long, List<long>> FindDeliveryIdsByLot(HashSet<long> lotPath = null, Dictionary<long, List<long>> deliveryByLot = null)
    {
        lotPath = lotPath ?? new HashSet<long>();
        var domain = new List<object[]>
        {
            new object[] { "LotId", "in", new object[] { this.Id } },
            new object[] { "State", "=", "done" }
        };

        var domainRestriction = GetOutgoingDomain();
        domain = new List<object[]> { new object[] { "AND", domain, domainRestriction } };

        var moveLines = Env.StockMoveLine.Search(domain);

        var movesByLot = moveLines.LotId.Distinct().ToDictionary(lotId => lotId, lotId => new { ProducingLines = new HashSet<long>(), BarrenLines = new HashSet<long>() });

        foreach (var line in moveLines)
        {
            if (line.ProduceLineIds.Count > 0)
            {
                movesByLot[line.LotId.Id].ProducingLines.Add(line.Id);
            }
            else
            {
                movesByLot[line.LotId.Id].BarrenLines.Add(line.Id);
            }
        }

        deliveryByLot = deliveryByLot ?? new Dictionary<long, List<long>>();

        foreach (var lot in new List<long> { this.Id })
        {
            var deliveryIds = new HashSet<long>();

            if (movesByLot.ContainsKey(lot))
            {
                var producingMoveLines = Env.StockMoveLine.Browse(movesByLot[lot].ProducingLines);
                var barrenMoveLines = Env.StockMoveLine.Browse(movesByLot[lot].BarrenLines);

                if (producingMoveLines.Count > 0)
                {
                    lotPath.Add(lot);
                    var nextLots = producingMoveLines.ProduceLineIds.LotId.Where(l => !lotPath.Contains(l)).ToList();
                    var nextLotsIds = new HashSet<long>(nextLots.Select(x => x.Id));

                    // If some producing lots are in lot_path, it means that they have been previously processed.
                    // Their results are therefore already in delivery_by_lot and we add them to delivery_ids directly.
                    deliveryIds.UnionWith((producingMoveLines.ProduceLineIds.LotId.Except(nextLots).Select(x => x.Id)).SelectMany(lotId => deliveryByLot.GetValueOrDefault(lotId, new List<long>())));

                    foreach (var (lotId, deliveryIdsSet) in nextLots.FindDeliveryIdsByLot(lotPath, deliveryByLot))
                    {
                        if (nextLotsIds.Contains(lotId))
                        {
                            deliveryIds.UnionWith(deliveryIdsSet);
                        }
                    }
                }

                deliveryIds.UnionWith(barrenMoveLines.PickingId.Select(x => x.Id));
            }

            deliveryByLot[lot] = deliveryIds.ToList();
        }

        return deliveryByLot;
    }

    private List<object[]> GetOutgoingDomain()
    {
        return new List<object[]>
        {
            new object[] { "|", new object[] { "PickingCode", "=", "outgoing" }, new object[] { "ProduceLineIds", "!=", false } }
        };
    }

    public string GetNextSerial(Res.Company company, Product.Product product)
    {
        if (product.Tracking != "none")
        {
            var lastSerial = Env.StockLot.Search(new List<object[]>
            {
                new object[] { "|", new object[] { "CompanyId", "=", company.Id }, new object[] { "CompanyId", "=", false } },
                new object[] { "ProductId", "=", product.Id }
            }, limit: 1, order: "Id DESC");

            if (lastSerial.Count > 0)
            {
                return GenerateLotNames(lastSerial.FirstOrDefault().Name, 2)[1]["lot_name"];
            }
        }

        return null;
    }

    private List<Dictionary<string, object>> GenerateLotNames(string firstLot, int count)
    {
        var caughtInitialNumber = Regex.Matches(firstLot, @"\d+").Select(x => x.Value).ToList();

        if (caughtInitialNumber.Count == 0)
        {
            return GenerateLotNames(firstLot + "0", count);
        }

        var initialNumber = caughtInitialNumber.Last();
        var padding = initialNumber.Length;

        var splitted = Regex.Split(firstLot, initialNumber).ToList();
        var prefix = string.Join(initialNumber, splitted.Take(splitted.Count - 1));
        var suffix = splitted.Last();

        var initialNumberInt = int.Parse(initialNumber);

        return Enumerable.Range(0, count).Select(i => new Dictionary<string, object>
        {
            { "lot_name", $"{prefix}{((initialNumberInt + i).ToString().PadLeft(padding, '0'))}{suffix}" }
        }).ToList();
    }
}
