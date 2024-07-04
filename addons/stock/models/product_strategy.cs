csharp
public partial class StockPutawayRule
{
    public virtual void ComputeStorageCategory()
    {
        if (this.Sublocation != "ClosestLocation")
        {
            this.StorageCategoryId = null;
        }
    }

    public virtual void OnchangeSublocation()
    {
        if (this.Sublocation == "ClosestLocation")
        {
            var childLocationIds = Env.Search<Stock.Location>(
                new[] {
                    new Domain("Id", "child_of", this.LocationOutId.Id),
                    new Domain("StorageCategoryId", "=", this.StorageCategoryId.Id)
                }
            );
            if (!childLocationIds.Any())
            {
                Env.Warning("Warning", "Selected storage category does not exist in the 'store to' location or any of its sublocations");
            }
        }
    }

    public virtual void OnchangeLocationIn()
    {
        var childLocationCount = Env.SearchCount<Stock.Location>(
            new[] {
                new Domain("Id", "=", this.LocationOutId.Id),
                new Domain("Id", "child_of", this.LocationInId.Id),
                new Domain("Id", "!=", this.LocationInId.Id)
            }
        );
        if (childLocationCount == 0 || this.LocationOutId == null)
        {
            this.LocationOutId = this.LocationInId;
        }
    }

    public virtual StockPutawayRule Create(Dictionary<string, object> values)
    {
        // your custom create logic here
        return this;
    }

    public virtual StockPutawayRule Write(Dictionary<string, object> values)
    {
        if (values.ContainsKey("CompanyId"))
        {
            if (this.CompanyId.Id != (long)values["CompanyId"])
            {
                Env.UserError("Changing the company of this record is forbidden at this point, you should rather archive it and create a new one.");
            }
        }
        // your custom write logic here
        return this;
    }

    public virtual Stock.Location GetLastUsedLocation(Stock.Product product)
    {
        var domain = new[] {
            new Domain("State", "=", "done"),
            new Domain("LocationDestId", "child_of", this.LocationOutId.Id),
            new Domain("ProductId", "=", product.Id)
        };
        if (this.PackageTypeIds != null && this.PackageTypeIds.Any())
        {
            domain = new[] {
                domain,
                new Domain("ResultPackageId.PackageTypeId", "in", this.PackageTypeIds.Select(p => p.Id).ToArray())
            };
        }

        return Env.Search<Stock.MoveLine>(domain, new[] { new Order("Date", "desc") }, 1).FirstOrDefault().LocationDestId;
    }

    public virtual Stock.Location GetPutawayLocation(Stock.Product product, decimal quantity, Stock.Package package = null, Stock.Packaging packaging = null, Dictionary<long, decimal> qtyByLocation = null)
    {
        var packageType = Env.Get<Stock.PackageType>();
        if (package != null)
        {
            packageType = package.PackageTypeId;
        }
        else if (packaging != null)
        {
            packageType = packaging.PackageTypeId;
        }

        var checkedLocations = new HashSet<long>();
        foreach (var putawayRule in this)
        {
            var locationOut = putawayRule.LocationOutId;
            if (putawayRule.Sublocation == "LastUsed")
            {
                var locationDestId = putawayRule.GetLastUsedLocation(product);
                locationOut = locationDestId != null ? locationDestId : locationOut;
            }

            var childLocations = locationOut.ChildInternalLocationIds;

            if (putawayRule.StorageCategoryId == null)
            {
                if (checkedLocations.Contains(locationOut.Id))
                {
                    continue;
                }
                if (locationOut.CheckCanBeUsed(product, quantity, package, qtyByLocation[locationOut.Id]))
                {
                    return locationOut;
                }
                checkedLocations.Add(locationOut.Id);
                continue;
            }
            else
            {
                childLocations = childLocations.Where(loc => loc.StorageCategoryId == putawayRule.StorageCategoryId).ToArray();
            }

            // check if already have the product/package type stored
            foreach (var location in childLocations)
            {
                if (checkedLocations.Contains(location.Id))
                {
                    continue;
                }
                if (packageType != null)
                {
                    if (location.QuantIds.Any(q => q.PackageId != null && q.PackageId.PackageTypeId == packageType))
                    {
                        if (location.CheckCanBeUsed(product, quantity, package, qtyByLocation[location.Id]))
                        {
                            return location;
                        }
                        else
                        {
                            checkedLocations.Add(location.Id);
                        }
                    }
                }
                else if (qtyByLocation[location.Id] > 0)
                {
                    if (location.CheckCanBeUsed(product, quantity, location_qty: qtyByLocation[location.Id]))
                    {
                        return location;
                    }
                    else
                    {
                        checkedLocations.Add(location.Id);
                    }
                }
            }

            // check locations with matched storage category
            foreach (var location in childLocations.Where(l => l.StorageCategoryId == putawayRule.StorageCategoryId).ToArray())
            {
                if (checkedLocations.Contains(location.Id))
                {
                    continue;
                }
                if (location.CheckCanBeUsed(product, quantity, package, qtyByLocation[location.Id]))
                {
                    return location;
                }
                checkedLocations.Add(location.Id);
            }
        }

        return null;
    }
}
