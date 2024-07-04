C#
public partial class LunchOrder 
{
    public void ComputeAvailableOnDate()
    {
        this.AvailableOnDate = Env.Get<Lunch.Supplier>().AvailableOnDate(this.Date);
    }

    public void ComputeOrderDeadlinePassed()
    {
        var today = Env.Get<Date>().Today;
        if (this.Date < today)
        {
            this.OrderDeadlinePassed = true;
        }
        else if (this.Date == today)
        {
            this.OrderDeadlinePassed = this.SupplierId.OrderDeadlinePassed;
        }
        else
        {
            this.OrderDeadlinePassed = false;
        }
    }

    public void ComputeTotalPrice()
    {
        this.Price = this.Quantity * (this.ProductId.Price + (this.ToppingIds1 | this.ToppingIds2 | this.ToppingIds3).Sum(t => t.Price));
    }

    public void ComputeDisplayToppings()
    {
        this.DisplayToppings = (this.ToppingIds1 | this.ToppingIds2 | this.ToppingIds3).Select(t => t.Name).Aggregate((a, b) => a + " + " + b);
    }

    public void ComputeProductImages()
    {
        this.Image1920 = this.ProductId.Image1920 ?? this.CategoryId.Image1920;
        this.Image128 = this.ProductId.Image128 ?? this.CategoryId.Image128;
    }

    public void ComputeAvailableToppings()
    {
        this.AvailableToppings1 = Env.Get<Lunch.Topping>().SearchCount(t => t.SupplierId.Id == this.SupplierId.Id && t.ToppingCategory == 1) > 0;
        this.AvailableToppings2 = Env.Get<Lunch.Topping>().SearchCount(t => t.SupplierId.Id == this.SupplierId.Id && t.ToppingCategory == 2) > 0;
        this.AvailableToppings3 = Env.Get<Lunch.Topping>().SearchCount(t => t.SupplierId.Id == this.SupplierId.Id && t.ToppingCategory == 3) > 0;
    }

    public void ComputeDisplayReorderButton()
    {
        this.DisplayReorderButton = Env.Context.ContainsKey("ShowReorderButton") && this.State == "Confirmed" && this.SupplierId.AvailableToday;
    }

    public LunchOrder Create(LunchOrder order)
    {
        // Implementation of create method
        return order;
    }

    public LunchOrder Write(LunchOrder order)
    {
        // Implementation of write method
        return order;
    }

    public Lunch.LunchOrder[] FindMatchingLines(LunchOrder order)
    {
        // Implementation of _find_matching_lines method
        return new Lunch.LunchOrder[] { };
    }

    public void UpdateQuantity(double increment)
    {
        if (this.Quantity <= -increment)
        {
            this.Active = false;
        }
        else
        {
            this.Quantity += increment;
        }
        CheckWallet();
    }

    public void AddToCart()
    {
        // Implementation of add_to_cart method
    }

    public void CheckWallet()
    {
        // Implementation of _check_wallet method
    }

    public void ActionOrder()
    {
        if (!this.AvailableOnDate)
        {
            throw new Exception("The vendor related to this order is not available at the selected date.");
        }
        if (this.ProductId.Active == false)
        {
            throw new Exception("Product is no longer available.");
        }
        this.State = "Ordered";
        CheckWallet();
    }

    public void ActionReorder()
    {
        var newOrder = this.Copy(new LunchOrder 
        {
            Date = Env.Get<Date>().Today,
            State = "Ordered"
        });
        // Implementation for opening the action to view the new order
    }

    public void ActionConfirm()
    {
        this.State = "Confirmed";
    }

    public void ActionCancel()
    {
        this.State = "Cancelled";
    }

    public void ActionReset()
    {
        this.State = "Ordered";
    }

    public void ActionSend()
    {
        this.State = "Sent";
    }

    public void ActionNotify()
    {
        if (this.Notified)
        {
            return;
        }
        // Implementation for sending notifications
        this.Notified = true;
    }

}
