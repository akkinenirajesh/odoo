csharp
public partial class Employee
{
    public ActionResult ActionOpenEmployeeCars()
    {
        this.EnsureOne();

        return new ActionResult
        {
            Type = "Ir.Actions.ActWindow",
            ResModel = "Fleet.VehicleAssignationLog",
            Views = new List<object[]>
            {
                new object[] { Env.Ref("Hr.Fleet.FleetVehicleAssignationLogEmployeeViewList").Id, "tree" },
                new object[] { false, "form" }
            },
            Domain = new List<object> { new object[] { "DriverEmployeeId", "in", new List<int> { this.Id } } },
            Context = new Dictionary<string, object>(Context)
            {
                { "default_driver_id", this.UserId.PartnerId },
                { "default_driver_employee_id", this.Id }
            },
            Name = "History Employee Cars"
        };
    }

    public void ComputeLicensePlate()
    {
        if (!string.IsNullOrEmpty(this.PrivateCarPlate) && this.CarIds.Any(c => !string.IsNullOrEmpty(c.LicensePlate)))
        {
            this.LicensePlate = string.Join(" ", this.CarIds.Where(c => !string.IsNullOrEmpty(c.LicensePlate)).Select(c => c.LicensePlate).Concat(new[] { this.PrivateCarPlate }));
        }
        else
        {
            this.LicensePlate = string.Join(" ", this.CarIds.Where(c => !string.IsNullOrEmpty(c.LicensePlate)).Select(c => c.LicensePlate)) ?? this.PrivateCarPlate;
        }
    }

    public List<object> SearchLicensePlate(string @operator, string value)
    {
        var employees = Env.Get<Hr.Employee>().Search(new List<object>
        {
            "|",
            new object[] { "CarIds.LicensePlate", @operator, value },
            new object[] { "PrivateCarPlate", @operator, value }
        });
        return new List<object> { new object[] { "Id", "in", employees.Select(e => e.Id).ToList() } };
    }

    public void ComputeEmployeeCarsCount()
    {
        var rg = Env.Get<Fleet.VehicleAssignationLog>().ReadGroup(
            new List<object> { new object[] { "DriverEmployeeId", "in", this.Ids } },
            new List<string> { "DriverEmployeeId" },
            new List<string> { "__count" }
        );
        var carsCount = rg.ToDictionary(g => g.DriverEmployeeId.Id, g => g.__count);
        this.EmployeeCarsCount = carsCount.GetValueOrDefault(this.Id, 0);
    }

    public void CheckWorkContactId()
    {
        var noAddress = this.Where(r => r.WorkContactId == null);
        var carIds = Env.Get<Fleet.Vehicle>().Sudo().Search(new List<object>
        {
            new object[] { "DriverEmployeeId", "in", noAddress.Select(e => e.Id).ToList() }
        });

        if (carIds.Any())
        {
            throw new ValidationException("Cannot remove address from employees with linked cars.");
        }
    }

    public override bool Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("UserId"))
        {
            this.SyncEmployeeCars(Env.Get<Core.Users>().Browse((int)vals["UserId"]));
        }

        var result = base.Write(vals);

        if (vals.ContainsKey("WorkContactId"))
        {
            var carIds = Env.Get<Fleet.Vehicle>().Sudo().Search(new List<object>
            {
                new object[] { "DriverEmployeeId", "in", this.Ids },
                new object[] { "DriverId", "in", this.Select(e => e.WorkContactId).ToList() }
            });
            if (carIds.Any())
            {
                carIds.Write(new Dictionary<string, object> { { "DriverId", vals["WorkContactId"] } });
            }
        }

        if (vals.ContainsKey("MobilityCard"))
        {
            var vehicles = Env.Get<Fleet.Vehicle>().Search(new List<object>
            {
                new object[] { "DriverId", "in", this.Select(e => e.UserId.PartnerId).Concat(this.Sudo().Select(e => e.WorkContactId)).ToList() }
            });
            vehicles.ComputeMobilityCard();
        }

        return result;
    }

    private void SyncEmployeeCars(Core.Users user)
    {
        if (this.WorkContactId != null && this.WorkContactId != user.PartnerId)
        {
            var cars = Env.Get<Fleet.Vehicle>().Search(new List<object>
            {
                "|",
                new object[] { "FutureDriverId", "=", this.WorkContactId },
                new object[] { "DriverId", "=", this.WorkContactId },
                new object[] { "CompanyId", "=", this.CompanyId }
            });

            foreach (var car in cars)
            {
                if (car.FutureDriverId == this.WorkContactId)
                {
                    car.FutureDriverId = user.PartnerId;
                }
                if (car.DriverId == this.WorkContactId)
                {
                    car.DriverId = user.PartnerId;
                }
            }
        }
    }
}
