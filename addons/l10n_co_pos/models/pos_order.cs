csharp
public partial class PosOrder
{
    public Dictionary<string, object> PrepareInvoiceVals()
    {
        var moveVals = base.PrepareInvoiceVals();

        if (Env.GetModel("Account.Move").HasField("L10nCoEdiDescriptionCodeCredit") &&
            moveVals.TryGetValue("MoveType", out var moveType) &&
            moveType.ToString() == "out_refund" &&
            moveVals.ContainsKey("ReversedEntryId"))
        {
            if (!moveVals.ContainsKey("L10nCoEdiDescriptionCodeCredit"))
            {
                moveVals["L10nCoEdiDescriptionCodeCredit"] = "1";
            }
        }

        return moveVals;
    }
}
