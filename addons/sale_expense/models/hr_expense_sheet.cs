csharp
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SaleOrder 
{
    public int OrderLineCount { get; set; }
    public bool IsLocked { get; set; }
    public bool IsSent { get; set; }

    public void ComputeOrderLineCount()
    {
        this.OrderLineCount = Env.Get("Sale.OrderLine").Search(new Dictionary<string, object> { { "SaleOrderId", this.Id } }).Count;
    }

    public void ActionConfirm()
    {
        this.State = "Sale";
        this.ConfirmationDate = DateTime.Now;
    }

    public void ActionCancel()
    {
        this.State = "Cancel";
    }

    public void ActionSend()
    {
        this.State = "Sent";
        this.IsSent = true;
    }

    public void ActionLock()
    {
        this.IsLocked = true;
    }

    public void ActionUnlock()
    {
        this.IsLocked = false;
    }
}
