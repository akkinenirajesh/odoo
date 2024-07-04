csharp
public partial class Account.AccountMove
{
  public virtual object StockAccountGetLastStepStockMoves()
  {
    // Your implementation here
    return null;
  }

  public virtual object GetInvoicedLotValues()
  {
    // Your implementation here
    return null;
  }

  public virtual void ComputeDeliveryDate()
  {
    // Your implementation here
    this.DeliveryDate = Env.Now;
  }

  public virtual void ComputeIncotermLocation()
  {
    // Your implementation here
    this.IncotermLocation = Env.Now;
  }
}

public partial class Account.AccountMoveLine
{
  public virtual bool SaleCanBeReinvoice()
  {
    // Your implementation here
    return false;
  }

  public virtual decimal StockAccountGetAngloSaxonPriceUnit()
  {
    // Your implementation here
    return 0;
  }
}
