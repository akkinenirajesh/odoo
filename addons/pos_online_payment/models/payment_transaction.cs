csharp
public partial class PaymentTransaction
{
    public string ComputeReferencePrefix(string providerCode, string separator, Dictionary<string, object> values)
    {
        if (values.TryGetValue("PosOrderId", out var posOrderIdObj) && posOrderIdObj is int posOrderId)
        {
            var posOrder = Env.Get<Pos.PosOrder>().Browse(posOrderId);
            if (posOrder != null)
            {
                return posOrder.PosReference;
            }
        }
        return base.ComputeReferencePrefix(providerCode, separator, values);
    }

    public void PostProcess()
    {
        base.PostProcess();
        ProcessPosOnlinePayment();
    }

    private void ProcessPosOnlinePayment()
    {
        if (PosOrderId != null && (State == "authorized" || State == "done") && PaymentId?.PosOrderId == null)
        {
            var posOrder = PosOrderId;
            if (Tools.FloatCompare(Amount, 0.0, posOrder.CurrencyId.Rounding) <= 0)
            {
                throw new ValidationException($"The payment transaction ({Id}) has a negative amount.");
            }

            if (PaymentId == null)
            {
                CreatePayment();
            }
            if (PaymentId == null)
            {
                throw new ValidationException($"The POS online payment (tx.id={Id}) could not be saved correctly");
            }

            var paymentMethod = posOrder.OnlinePaymentMethodId;
            if (paymentMethod == null)
            {
                var posConfig = posOrder.ConfigId;
                paymentMethod = Env.Get<Pos.PosPaymentMethod>().GetOrCreateOnlinePaymentMethod(posConfig.CompanyId.Id, posConfig.Id);
                if (paymentMethod == null)
                {
                    throw new ValidationException($"The POS online payment (tx.id={Id}) could not be saved correctly because the online payment method could not be found");
                }
            }

            posOrder.AddPayment(new Dictionary<string, object>
            {
                {"Amount", Amount},
                {"PaymentDate", LastStateChange},
                {"PaymentMethodId", paymentMethod.Id},
                {"OnlineAccountPaymentId", PaymentId.Id},
                {"PosOrderId", posOrder.Id}
            });

            PaymentId.Update(new Dictionary<string, object>
            {
                {"PosPaymentMethodId", paymentMethod.Id},
                {"PosOrderId", posOrder.Id},
                {"PosSessionId", posOrder.SessionId.Id}
            });

            if (posOrder.State == "draft" && posOrder.IsPosOrderPaid())
            {
                posOrder.ProcessSavedOrder(false);
            }

            posOrder.ConfigId.Notify("ONLINE_PAYMENTS_NOTIFICATION", new { id = posOrder.Id });
        }
    }

    public Dictionary<string, object> ActionViewPosOrder()
    {
        return new Dictionary<string, object>
        {
            {"Name", "POS Order"},
            {"Type", "ir.actions.act_window"},
            {"ResModel", "pos.order"},
            {"Target", "current"},
            {"ResId", PosOrderId.Id},
            {"ViewMode", "form"}
        };
    }
}
