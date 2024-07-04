csharp
public partial class StockPickingType 
{
    public virtual string Name { get; set; }
    public virtual int Color { get; set; }
    public virtual int Sequence { get; set; }
    public virtual CoreIrSequence SequenceId { get; set; }
    public virtual string SequenceCode { get; set; }
    public virtual StockStockLocation DefaultLocationSrcId { get; set; }
    public virtual StockStockLocation DefaultLocationDestId { get; set; }
    public virtual StockStockLocation DefaultLocationReturnId { get; set; }
    public virtual string Code { get; set; }
    public virtual StockPickingType ReturnPickingTypeId { get; set; }
    public virtual bool ShowEntirePacks { get; set; }
    public virtual StockStockWarehouse WarehouseId { get; set; }
    public virtual bool Active { get; set; }
    public virtual bool UseCreateLots { get; set; }
    public virtual bool UseExistingLots { get; set; }
    public virtual bool PrintLabel { get; set; }
    public virtual bool ShowOperations { get; set; }
    public virtual string ReservationMethod { get; set; }
    public virtual int ReservationDaysBefore { get; set; }
    public virtual int ReservationDaysBeforePriority { get; set; }
    public virtual bool AutoShowReceptionReport { get; set; }
    public virtual bool AutoPrintDeliverySlip { get; set; }
    public virtual bool AutoPrintReturnSlip { get; set; }
    public virtual bool AutoPrintProductLabels { get; set; }
    public virtual string ProductLabelFormat { get; set; }
    public virtual bool AutoPrintLotLabels { get; set; }
    public virtual string LotLabelFormat { get; set; }
    public virtual bool AutoPrintReceptionReport { get; set; }
    public virtual bool AutoPrintReceptionReportLabels { get; set; }
    public virtual bool AutoPrintPackages { get; set; }
    public virtual bool AutoPrintPackageLabel { get; set; }
    public virtual string PackageLabelToPrint { get; set; }
    public virtual int CountPickingDraft { get; set; }
    public virtual int CountPickingReady { get; set; }
    public virtual int CountPicking { get; set; }
    public virtual int CountPickingWaiting { get; set; }
    public virtual int CountPickingLate { get; set; }
    public virtual int CountPickingBackorders { get; set; }
    public virtual int CountMoveReady { get; set; }
    public virtual bool HideReservationMethod { get; set; }
    public virtual string Barcode { get; set; }
    public virtual CoreResCompany CompanyId { get; set; }
    public virtual string CreateBackorder { get; set; }
    public virtual bool ShowPickingType { get; set; }
    public virtual PropertiesDefinition PickingPropertiesDefinition { get; set; }
    public virtual ICollection<CoreResUsers> FavoriteUserIds { get; set; }
    public virtual bool IsFavorite { get; set; }
    public virtual string KanbanDashboardGraph { get; set; }
    public virtual string ReadyItemsLabel { get; set; }

    public virtual void ComputeDefaultLocationSrcId() 
    {
        if (Code == "incoming")
        {
            DefaultLocationSrcId = Env.Ref("Stock.stock_location_suppliers");
        }
        else
        {
            DefaultLocationSrcId = WarehouseId.LotStockId;
        }
    }
    public virtual void ComputeDefaultLocationDestId() 
    {
        if (Code == "outgoing")
        {
            DefaultLocationDestId = Env.Ref("Stock.stock_location_customers");
        }
        else
        {
            DefaultLocationDestId = WarehouseId.LotStockId;
        }
    }
    public virtual void ComputePrintLabel() 
    {
        if (Code == "incoming" || Code == "internal")
        {
            PrintLabel = false;
        }
        else if (Code == "outgoing")
        {
            PrintLabel = true;
        }
    }
    public virtual void ComputeUseCreateLots() 
    {
        if (Code == "incoming")
        {
            UseCreateLots = true;
        }
    }
    public virtual void ComputeUseExistingLots() 
    {
        if (Code == "outgoing")
        {
            UseExistingLots = true;
        }
    }
    public virtual void ComputeHideReservationMethod() 
    {
        HideReservationMethod = Code == "incoming";
    }
    public virtual void ComputePickingCount() 
    {
        CountPickingDraft = Env.Model<StockPicking>().SearchCount(new[] { ("State", "=", "draft"), ("PickingTypeId", "in", this.Id) });
        CountPickingWaiting = Env.Model<StockPicking>().SearchCount(new[] { ("State", "in", new[] { "confirmed", "waiting" }), ("PickingTypeId", "in", this.Id) });
        CountPickingReady = Env.Model<StockPicking>().SearchCount(new[] { ("State", "=", "assigned"), ("PickingTypeId", "in", this.Id) });
        CountPicking = Env.Model<StockPicking>().SearchCount(new[] { ("State", "in", new[] { "assigned", "waiting", "confirmed" }), ("PickingTypeId", "in", this.Id) });
        CountPickingLate = Env.Model<StockPicking>().SearchCount(new[] { ("ScheduledDate", "<", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), ("State", "in", new[] { "assigned", "waiting", "confirmed" }), ("PickingTypeId", "in", this.Id) });
        CountPickingBackorders = Env.Model<StockPicking>().SearchCount(new[] { ("BackorderId", "!=", null), ("State", "in", new[] { "confirmed", "assigned", "waiting" }), ("PickingTypeId", "in", this.Id) });
    }
    public virtual void ComputeMoveCount() 
    {
        CountMoveReady = Env.Model<StockMove>().SearchCount(new[] { ("State", "=", "assigned"), ("PickingTypeId", "in", this.Id) });
    }
    public virtual void ComputeShowPickingType() 
    {
        ShowPickingType = Code == "incoming" || Code == "outgoing" || Code == "internal";
    }
    public virtual void ComputeIsFavorite() 
    {
        IsFavorite = FavoriteUserIds.Contains(Env.User);
    }
    public virtual void InverseIsFavorite() 
    {
        if (!FavoriteUserIds.Contains(Env.User))
        {
            FavoriteUserIds.Add(Env.User);
        }
        else
        {
            FavoriteUserIds.Remove(Env.User);
        }
    }
    public virtual Domain SearchIsFavorite(string operator, object value)
    {
        if (operator != "=" && operator != "!=")
        {
            throw new NotImplementedException("Operation not supported");
        }
        if (!(value is bool))
        {
            throw new NotImplementedException("Value should be boolean");
        }
        if (operator == "=" && (bool)value == true || operator == "!=" && (bool)value == false)
        {
            return new Domain(new[] { ("FavoriteUserIds", "in", Env.User.Id) });
        }
        else
        {
            return new Domain(new[] { ("FavoriteUserIds", "not in", Env.User.Id) });
        }
    }
    public virtual void ComputeKanbanDashboardGraph() 
    {
        var groupedRecords = _GetAggregatedRecordsByDate();

        var startToday = DateTime.Now.Date;

        var startYesterday = startToday.AddDays(-1);
        var startDay1 = startToday.AddDays(1);
        var startDay2 = startToday.AddDays(2);

        var summaries = new Dictionary<int, Dictionary<string, int>>();
        foreach (var (pickingTypeId, dates, dataSeriesName) in groupedRecords)
        {
            summaries[pickingTypeId.Id] = new Dictionary<string, int>()
            {
                { "DataSeriesName", dataSeriesName },
                { "TotalBefore", 0 },
                { "TotalYesterday", 0 },
                { "TotalToday", 0 },
                { "TotalDay1", 0 },
                { "TotalDay2", 0 },
                { "TotalAfter", 0 },
            };
            foreach (var pDate in dates)
            {
                var pDateUtc = TimeZoneInfo.ConvertTime(pDate, TimeZoneInfo.Utc).Date;
                if (pDateUtc < startYesterday)
                {
                    summaries[pickingTypeId.Id]["TotalBefore"] += 1;
                }
                else if (pDateUtc == startYesterday)
                {
                    summaries[pickingTypeId.Id]["TotalYesterday"] += 1;
                }
                else if (pDateUtc == startToday)
                {
                    summaries[pickingTypeId.Id]["TotalToday"] += 1;
                }
                else if (pDateUtc == startDay1)
                {
                    summaries[pickingTypeId.Id]["TotalDay1"] += 1;
                }
                else if (pDateUtc == startDay2)
                {
                    summaries[pickingTypeId.Id]["TotalDay2"] += 1;
                }
                else
                {
                    summaries[pickingTypeId.Id]["TotalAfter"] += 1;
                }
            }
        }

        foreach (var pickingType in this)
        {
            var pickingTypeSummary = summaries.GetValueOrDefault(pickingType.Id);
            var graphData = _PrepareGraphData(pickingTypeSummary);
            KanbanDashboardGraph = JsonSerializer.Serialize(graphData);
        }
    }
    public virtual void ComputeReadyItemsLabel() 
    {
        switch (Code)
        {
            case "incoming":
                ReadyItemsLabel = "To Receive";
                break;
            case "outgoing":
                ReadyItemsLabel = "To Deliver";
                break;
            default:
                ReadyItemsLabel = "To Process";
                break;
        }
    }
    private List<(int PickingTypeId, List<DateTime>, string)> _GetAggregatedRecordsByDate()
    {
        var records = Env.Model<StockPicking>().ReadGroup(
            new[]
            {
                ("PickingTypeId", "in", this.Id),
                ("State", "=", "assigned")
            },
            new[] { "PickingTypeId" },
            new[] { ("ScheduledDate", "array_agg") }
        );
        return records.Select(r => (r[0], r[1].Select(x => DateTime.Parse(x)).ToList(), "Ready")).ToList();
    }
    private List<Dictionary<string, object>> _PrepareGraphData(Dictionary<string, int> pickingTypeSummary)
    {
        var mapping = new Dictionary<string, Dictionary<string, object>>()
        {
            { "TotalBefore", new Dictionary<string, object>() { { "Label", "Before" }, { "Type", "past" } } },
            { "TotalYesterday", new Dictionary<string, object>() { { "Label", "Yesterday" }, { "Type", "past" } } },
            { "TotalToday", new Dictionary<string, object>() { { "Label", "Today" }, { "Type", "present" } } },
            { "TotalDay1", new Dictionary<string, object>() { { "Label", "Tomorrow" }, { "Type", "future" } } },
            { "TotalDay2", new Dictionary<string, object>() { { "Label", "The day after tomorrow" }, { "Type", "future" } } },
            { "TotalAfter", new Dictionary<string, object>() { { "Label", "After" }, { "Type", "future" } } },
        };

        if (pickingTypeSummary != null)
        {
            var graphData = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    { "Key", pickingTypeSummary["DataSeriesName"] },
                    { "Values", mapping.Select(kvp =>
                        new Dictionary<string, object>()
                        {
                            { "Label", kvp.Value["Label"] },
                            { "Type", kvp.Value["Type"] },
                            { "Value", pickingTypeSummary[kvp.Key] },
                        }
                    ).ToList() }
                }
            };
            return graphData;
        }
        else
        {
            var graphData = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    { "Key", "Sample data" },
                    { "Values", mapping.Select(kvp =>
                        new Dictionary<string, object>()
                        {
                            { "Label", kvp.Value["Label"] },
                            { "Type", "sample" },
                            { "Value", Random.Shared.Next(1, 11) },
                        }
                    ).ToList() }
                }
            };
            return graphData;
        }
    }
    public virtual void OnChangeSequenceCode() 
    {
        if (string.IsNullOrEmpty(SequenceCode))
        {
            return;
        }
        var domain = new Domain(new[] { ("SequenceCode", "=", SequenceCode), ("CompanyId", "=", CompanyId.Id) });
        if (this.Id != 0)
        {
            domain.Conditions.Add(new DomainCondition("Id", "!=", this.Id));
        }
        var pickingType = Env.Model<StockPickingType>().Search(domain, 1);
        if (pickingType != null && pickingType.SequenceId != SequenceId)
        {
            throw new UserError($"This sequence prefix is already being used by another operation type. It is recommended that you select a unique prefix to avoid issues and/or repeated reference values or assign the existing reference sequence to this operation type.");
        }
    }
    public virtual void OnChangePickingCode() 
    {
        if (Code == "internal" && !Env.User.HasGroup("Stock.group_stock_multi_locations"))
        {
            throw new UserError("You need to activate storage locations to be able to do internal operation types.");
        }
    }
    public virtual void ComputeWarehouseId() 
    {
        if (WarehouseId != null)
        {
            return;
        }
        if (CompanyId != null)
        {
            WarehouseId = Env.Model<StockStockWarehouse>().Search(new[] { ("CompanyId", "=", CompanyId.Id) }, 1);
        }
    }
}
