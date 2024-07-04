csharp
using System;
using System.Collections.Generic;
using System.Linq;

public partial class AccountMove
{
  public AccountMove()
  {
  }

  public void OnchangePurchaseAutoComplete()
  {
    if (Env.Get("PurchaseVendorBillId").Get("VendorBillId"))
    {
      Env.Set("InvoiceVendorBillId", Env.Get("PurchaseVendorBillId").Get("VendorBillId"));
      OnchangeInvoiceVendorBill();
    }
    else if (Env.Get("PurchaseVendorBillId").Get("PurchaseOrderId"))
    {
      Env.Set("PurchaseId", Env.Get("PurchaseVendorBillId").Get("PurchaseOrderId"));
    }
    Env.Set("PurchaseVendorBillId", false);

    if (Env.Get("PurchaseId"))
    {
      var invoiceVals = Env.Get("PurchaseId").WithCompany(Env.Get("PurchaseId").Get("CompanyId"))._PrepareInvoice();
      var hasInvoiceLines = Env.Get("InvoiceLineIds").Any(x => x.Get("DisplayType") != "line_note" && x.Get("DisplayType") != "line_section");
      var newCurrencyId = Env.Get("CurrencyId") != null ? Env.Get("CurrencyId") : invoiceVals.Get("CurrencyId");

      invoiceVals.Remove("Ref");
      invoiceVals.Remove("PaymentReference");
      invoiceVals.Remove("CompanyId");
      if (Env.Get("MoveType") == invoiceVals.Get("MoveType"))
      {
        invoiceVals.Remove("MoveType");
      }
      Env.Update(invoiceVals);
      Env.Set("CurrencyId", newCurrencyId);

      var poLines = Env.Get("PurchaseId").Get("OrderLine").Except(Env.Get("InvoiceLineIds").Select(x => x.Get("PurchaseLineId")));
      foreach (var line in poLines.Where(l => l.Get("DisplayType") == null))
      {
        Env.Get("InvoiceLineIds").Add(Env["Account.MoveLine"].New(line._PrepareAccountMoveLine(this)));
      }

      var origins = Env.Get("InvoiceLineIds").Select(x => x.Get("PurchaseLineId").Get("OrderId").Get("Name")).Distinct().ToList();
      Env.Set("InvoiceOrigin", String.Join(",", origins));

      var refs = _GetInvoiceReference();
      Env.Set("Ref", String.Join(", ", refs));

      if (Env.Get("PaymentReference") == null)
      {
        if (refs.Count == 1)
        {
          Env.Set("PaymentReference", refs[0]);
        }
        else if (refs.Count > 1)
        {
          Env.Set("PaymentReference", refs.Last());
        }
      }

      Env.Set("PurchaseId", false);
    }
  }

  public void OnchangePartnerId()
  {
    // ... (implementation for onchange_partner_id)
  }

  public void ComputePurchaseOrderCount()
  {
    Env.Set("PurchaseOrderCount", Env.Get("LineIds").Count(x => x.Get("PurchaseLineId") != null));
  }

  public object ActionViewSourcePurchaseOrders()
  {
    // ... (implementation for action_view_source_purchase_orders)
  }

  public void Create(Dictionary<string, object> vals)
  {
    // ... (implementation for create)
  }

  public void Write(Dictionary<string, object> vals)
  {
    // ... (implementation for write)
  }

  private List<string> _GetInvoiceReference()
  {
    // ... (implementation for _get_invoice_reference)
  }

  // ... (Implement other methods, like _FindMatchingSubsetPOLines, _FindMatchingPOAndInvLines, 
  // _SetPurchaseOrders, _MatchPurchaseOrders, _FindAndSetPurchaseOrders, _GetEDICreation)
}
