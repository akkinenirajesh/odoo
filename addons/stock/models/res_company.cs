csharp
public partial class Stock.Company {
    public void CreateMissingTransitLocation() {
        var companyWithoutTransit = Env.Model<Stock.Company>().Search(c => c.InternalTransitLocation == null);
        companyWithoutTransit.CreateTransitLocation();
    }

    public void CreateMissingInventoryLossLocation() {
        var companyIds = Env.Model<Stock.Company>().Search();
        var inventoryLossProductTemplateField = Env.Model<Ir.Model.Fields>().Get("product.template", "property_stock_inventory");
        var companiesHavingProperty = Env.Model<Ir.Property>().Search(p => p.Fields == inventoryLossProductTemplateField && p.Res == null).Map(p => p.Company);
        var companyWithoutProperty = companyIds - companiesHavingProperty;
        companyWithoutProperty.CreateInventoryLossLocation();
    }

    public void CreateMissingProductionLocation() {
        var companyIds = Env.Model<Stock.Company>().Search();
        var productionProductTemplateField = Env.Model<Ir.Model.Fields>().Get("product.template", "property_stock_production");
        var companiesHavingProperty = Env.Model<Ir.Property>().Search(p => p.Fields == productionProductTemplateField && p.Res == null).Map(p => p.Company);
        var companyWithoutProperty = companyIds - companiesHavingProperty;
        companyWithoutProperty.CreateProductionLocation();
    }

    public void CreateMissingScrapLocation() {
        var companyIds = Env.Model<Stock.Company>().Search();
        var companiesHavingScrapLoc = Env.Model<Stock.Location>().Search(l => l.ScrapLocation).Map(l => l.Company);
        var companyWithoutProperty = companyIds - companiesHavingScrapLoc;
        companyWithoutProperty.CreateScrapLocation();
    }

    public void CreateMissingScrapSequence() {
        var companyIds = Env.Model<Stock.Company>().Search();
        var companyHasScrapSeq = Env.Model<Ir.Sequence>().Search(s => s.Code == "stock.scrap").Map(s => s.Company);
        var companyTodoSequence = companyIds - companyHasScrapSeq;
        companyTodoSequence.CreateScrapSequence();
    }

    private void CreateTransitLocation() {
        var parentLocation = Env.Ref<Stock.Location>("stock.stock_location_locations");
        var location = Env.Model<Stock.Location>().Create(new Stock.Location {
            Name = "Inter-warehouse transit",
            Usage = "transit",
            Location = parentLocation,
            Company = this,
            Active = false
        });
        this.InternalTransitLocation = location;
        this.Partner.WithCompany(this).Update(new Core.Partner {
            PropertyStockCustomer = location,
            PropertyStockSupplier = location
        });
    }

    private void CreateInventoryLossLocation() {
        var parentLocation = Env.Ref<Stock.Location>("stock.stock_location_locations_virtual");
        var inventoryLossLocation = Env.Model<Stock.Location>().Create(new Stock.Location {
            Name = "Inventory adjustment",
            Usage = "inventory",
            Location = parentLocation,
            Company = this
        });
        Env.Model<Ir.Property>().SetDefault("property_stock_inventory", "product.template", inventoryLossLocation, this.Id);
    }

    private void CreateProductionLocation() {
        var parentLocation = Env.Ref<Stock.Location>("stock.stock_location_locations_virtual");
        var productionLocation = Env.Model<Stock.Location>().Create(new Stock.Location {
            Name = "Production",
            Usage = "production",
            Location = parentLocation,
            Company = this
        });
        Env.Model<Ir.Property>().SetDefault("property_stock_production", "product.template", productionLocation, this.Id);
    }

    private void CreateScrapLocation() {
        var parentLocation = Env.Ref<Stock.Location>("stock.stock_location_locations_virtual");
        var scrapLocation = Env.Model<Stock.Location>().Create(new Stock.Location {
            Name = "Scrap",
            Usage = "inventory",
            Location = parentLocation,
            Company = this,
            ScrapLocation = true
        });
    }

    private void CreateScrapSequence() {
        Env.Model<Ir.Sequence>().Create(new Ir.Sequence {
            Name = $"{this.Name} Sequence scrap",
            Code = "stock.scrap",
            Company = this,
            Prefix = "SP/",
            Padding = 5,
            NumberNext = 1,
            NumberIncrement = 1
        });
    }

    public void CreateMissingWarehouse() {
        var companyIds = Env.Model<Stock.Company>().Search();
        var companyWithWarehouse = Env.Model<Stock.Warehouse>().Search(w => w.Company != null).Map(w => w.Company);
        var companyWithoutWarehouse = companyIds - companyWithWarehouse;
        companyWithoutWarehouse.CreateWarehouse();
    }

    private void CreateWarehouse() {
        Env.Model<Stock.Warehouse>().Create(new Stock.Warehouse {
            Name = this.Name,
            Code = Env.Context.Get<string>("default_code") ?? this.Name[..5],
            Company = this,
            Partner = this.Partner
        });
    }

    public void CreatePerCompanyLocations() {
        CreateTransitLocation();
        CreateInventoryLossLocation();
        CreateProductionLocation();
        CreateScrapLocation();
    }

    public void CreatePerCompanySequences() {
        CreateScrapSequence();
    }

    public void SetPerCompanyInterCompanyLocations(Stock.Location interCompanyLocation) {
        if (!Env.User.HasGroup("base.group_multi_company")) return;
        var otherCompanies = Env.Model<Stock.Company>().Search(c => c.Id != this.Id);
        otherCompanies.Partner.WithCompany(this).Update(new Core.Partner {
            PropertyStockCustomer = interCompanyLocation,
            PropertyStockSupplier = interCompanyLocation
        });
        foreach (var company in otherCompanies) {
            this.Partner.WithCompany(company).Update(new Core.Partner {
                PropertyStockCustomer = interCompanyLocation,
                PropertyStockSupplier = interCompanyLocation
            });
        }
    }

    public static void Create(Stock.Company[] companies) {
        var createdCompanies = Env.Model<Stock.Company>().Create(companies);
        var interCompanyLocation = Env.Ref<Stock.Location>("stock.stock_location_inter_company");
        if (!interCompanyLocation.Active) interCompanyLocation.Update(new Stock.Location { Active = true });
        foreach (var company in createdCompanies) {
            company.CreatePerCompanyLocations();
            company.CreatePerCompanySequences();
            company.SetPerCompanyInterCompanyLocations(interCompanyLocation);
        }
        foreach (var company in createdCompanies) {
            Env.Model<Stock.Warehouse>().Create(new Stock.Warehouse {
                Name = company.Name,
                Code = Env.Context.Get<string>("default_code") ?? company.Name[..5],
                Company = company,
                Partner = company.Partner
            });
        }
    }
}
