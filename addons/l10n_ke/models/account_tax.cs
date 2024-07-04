csharp
public partial class AccountTax 
{
    public void OnChangeAmount()
    {
        if (this._origin.Amount != this.Amount)
        {
            this.L10nKeItemCode = null;
        }
    }
}
