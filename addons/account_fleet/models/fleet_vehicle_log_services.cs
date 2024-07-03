csharp
public partial class VehicleLogServices
{
    public void ComputeVehicle()
    {
        if (this.AccountMoveLine?.Vehicle == null)
        {
            return;
        }
        this.Vehicle = this.AccountMoveLine.Vehicle;
    }

    public void InverseAmount()
    {
        if (this.AccountMoveLine != null)
        {
            throw new UserException("You cannot modify amount of services linked to an account move line. Do it on the related accounting entry instead.");
        }
    }

    public void ComputeAmount()
    {
        this.Amount = this.AccountMoveLine?.Debit ?? 0;
    }

    public ActionResult ActionOpenAccountMove()
    {
        if (this.AccountMoveLine?.Move == null)
        {
            return null;
        }

        return new ActionResult
        {
            Type = ActionType.Window,
            ViewMode = "form",
            ResModel = "Account.Move",
            Target = "current",
            Name = "Bill",
            ResId = this.AccountMoveLine.Move.Id
        };
    }

    public void OnDelete()
    {
        if (Env.Context.Get("ignore_linked_bill_constraint", false))
        {
            return;
        }

        if (this.AccountMoveLine != null)
        {
            throw new UserException("You cannot delete log services records because one or more of them were bill created.");
        }
    }
}
