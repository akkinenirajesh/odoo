csharp
public partial class PaymentTransaction
{
    public void ActionDemoSetDone()
    {
        if (this.ProviderCode != "demo")
        {
            return;
        }

        var notificationData = new Dictionary<string, object>()
        {
            { "Reference", this.Reference },
            { "SimulatedState", "done" }
        };
        this.HandleNotificationData("demo", notificationData);
    }

    public void ActionDemoSetCanceled()
    {
        if (this.ProviderCode != "demo")
        {
            return;
        }

        var notificationData = new Dictionary<string, object>()
        {
            { "Reference", this.Reference },
            { "SimulatedState", "cancel" }
        };
        this.HandleNotificationData("demo", notificationData);
    }

    public void ActionDemoSetError()
    {
        if (this.ProviderCode != "demo")
        {
            return;
        }

        var notificationData = new Dictionary<string, object>()
        {
            { "Reference", this.Reference },
            { "SimulatedState", "error" }
        };
        this.HandleNotificationData("demo", notificationData);
    }

    public void SendPaymentRequest()
    {
        base.SendPaymentRequest();
        if (this.ProviderCode != "demo")
        {
            return;
        }

        if (this.Token == null)
        {
            throw new Exception("Demo: The transaction is not linked to a token.");
        }

        var simulatedState = this.Token.DemoSimulatedState;
        var notificationData = new Dictionary<string, object>()
        {
            { "Reference", this.Reference },
            { "SimulatedState", simulatedState }
        };
        this.HandleNotificationData("demo", notificationData);
    }

    public PaymentTransaction SendRefundRequest()
    {
        var refundTx = base.SendRefundRequest();
        if (this.ProviderCode != "demo")
        {
            return refundTx;
        }

        var notificationData = new Dictionary<string, object>()
        {
            { "Reference", refundTx.Reference },
            { "SimulatedState", "done" }
        };
        refundTx.HandleNotificationData("demo", notificationData);

        return refundTx;
    }

    public PaymentTransaction SendCaptureRequest(decimal amountToCapture = 0)
    {
        var childCaptureTx = base.SendCaptureRequest(amountToCapture);
        if (this.ProviderCode != "demo")
        {
            return childCaptureTx;
        }

        var tx = childCaptureTx ?? this;
        var notificationData = new Dictionary<string, object>()
        {
            { "Reference", tx.Reference },
            { "SimulatedState", "done" },
            { "ManualCapture", true }
        };
        tx.HandleNotificationData("demo", notificationData);

        return childCaptureTx;
    }

    public PaymentTransaction SendVoidRequest(decimal amountToVoid = 0)
    {
        var childVoidTx = base.SendVoidRequest(amountToVoid);
        if (this.ProviderCode != "demo")
        {
            return childVoidTx;
        }

        var tx = childVoidTx ?? this;
        var notificationData = new Dictionary<string, object>()
        {
            { "Reference", tx.Reference },
            { "SimulatedState", "cancel" }
        };
        tx.HandleNotificationData("demo", notificationData);

        return childVoidTx;
    }

    public PaymentTransaction GetTransactionFromNotificationData(string providerCode, Dictionary<string, object> notificationData)
    {
        var tx = base.GetTransactionFromNotificationData(providerCode, notificationData);
        if (providerCode != "demo" || tx != null)
        {
            return tx;
        }

        var reference = notificationData["Reference"] as string;
        tx = Env.Search<PaymentTransaction>(t => t.Reference == reference && t.ProviderCode == "demo");
        if (tx == null)
        {
            throw new Exception($"Demo: No transaction found matching reference {reference}.");
        }
        return tx;
    }

    public void ProcessNotificationData(Dictionary<string, object> notificationData)
    {
        base.ProcessNotificationData(notificationData);
        if (this.ProviderCode != "demo")
        {
            return;
        }

        this.ProviderReference = $"demo-{this.Reference}";

        if (this.Tokenize)
        {
            this.DemoTokenizeFromNotificationData(notificationData);
        }

        var state = notificationData["SimulatedState"] as string;
        if (state == "pending")
        {
            this.SetPending();
        }
        else if (state == "done")
        {
            if (this.CaptureManually && !notificationData.ContainsKey("ManualCapture"))
            {
                this.SetAuthorized();
            }
            else
            {
                this.SetDone();

                if (this.Operation == "refund")
                {
                    Env.Ref("payment.cron_post_process_payment_tx")._trigger();
                }
            }
        }
        else if (state == "cancel")
        {
            this.SetCanceled();
        }
        else
        {
            this.SetError($"You selected the following demo payment status: {state}");
        }
    }

    public void DemoTokenizeFromNotificationData(Dictionary<string, object> notificationData)
    {
        var state = notificationData["SimulatedState"] as string;
        var token = Env.Create<PaymentToken>(new PaymentToken()
        {
            Provider = this.Provider,
            PaymentMethod = this.PaymentMethod,
            PaymentDetails = notificationData["PaymentDetails"] as string,
            Partner = this.Partner,
            ProviderRef = "fake provider reference",
            DemoSimulatedState = state
        });
        this.Token = token;
        this.Tokenize = false;
        Env.Logger.Info($"Created token with id {token.Id} for partner with id {this.Partner.Id}.");
    }

    // All other methods from base PaymentTransaction class should be here.
}
