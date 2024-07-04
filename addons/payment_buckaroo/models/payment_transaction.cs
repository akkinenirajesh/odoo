csharp
public partial class PaymentTransaction {
    public virtual float Amount { get; set; }
    public virtual Core.Currency Currency { get; set; }
    public virtual string Reference { get; set; }
    public virtual string ProviderCode { get; set; }
    public virtual string ProviderReference { get; set; }
    public virtual string PartnerLang { get; set; }
    public virtual Payment.PaymentMethod PaymentMethod { get; set; }
    public virtual Payment.TransactionState State { get; set; }
    public virtual string ErrorMessage { get; set; }
    public virtual Payment.Provider Provider { get; set; }

    public virtual Dictionary<string, object> GetSpecificRenderingValues(Dictionary<string, object> processingValues) {
        if (this.ProviderCode != "buckaroo") {
            return processingValues;
        }

        var returnUrl = Env.Utils.UrlJoin(this.Provider.GetBaseUrl(), "BuckarooController._returnUrl");
        var renderingValues = new Dictionary<string, object>() {
            { "api_url", this.Provider._BuckarooGetApiUrl() },
            { "Brq_websitekey", this.Provider.BuckarooWebsiteKey },
            { "Brq_amount", this.Amount },
            { "Brq_currency", this.Currency.Name },
            { "Brq_invoicenumber", this.Reference },
            { "Brq_return", returnUrl },
            { "Brq_returncancel", returnUrl },
            { "Brq_returnerror", returnUrl },
            { "Brq_returnreject", returnUrl },
        };

        if (!string.IsNullOrEmpty(this.PartnerLang)) {
            renderingValues.Add("Brq_culture", this.PartnerLang.Replace("_", "-"));
        }

        renderingValues.Add("Brq_signature", this.Provider._BuckarooGenerateDigitalSign(renderingValues, false));

        return renderingValues;
    }

    public virtual PaymentTransaction GetTransactionFromNotificationData(string providerCode, Dictionary<string, object> notificationData) {
        if (providerCode != "buckaroo") {
            return Env.Model("Payment.PaymentTransaction").Search(new Dictionary<string, object>() { { "ProviderCode", providerCode } });
        }

        var reference = notificationData["brq_invoicenumber"] as string;
        return Env.Model("Payment.PaymentTransaction").Search(new Dictionary<string, object>() { { "Reference", reference }, { "ProviderCode", "buckaroo" } });
    }

    public virtual void ProcessNotificationData(Dictionary<string, object> notificationData) {
        if (this.ProviderCode != "buckaroo") {
            return;
        }

        var transactionKeys = notificationData["brq_transactions"] as string;
        if (string.IsNullOrEmpty(transactionKeys)) {
            throw new Exception("Buckaroo: Received data with missing transaction keys");
        }
        this.ProviderReference = transactionKeys.Split(',')[0];

        var paymentMethodCode = notificationData["brq_payment_method"] as string;
        var paymentMethod = Env.Model("Payment.PaymentMethod")._GetFromCode(paymentMethodCode, new Dictionary<string, string>() { { "brq_ideal", "ideal" }, { "brq_paypal", "paypal" }, { "brq_creditcard", "creditcard" }, { "brq_afterpay", "afterpay" }, { "brq_banktransfer", "banktransfer" }, { "brq_sofort", "sofort" }, { "brq_mistercash", "mistercash" }, { "brq_giropay", "giropay" }, { "brq_eps", "eps" }, { "brq_kbc", "kbc" }, { "brq_belfius", "belfius" }, { "brq_inghomepay", "inghomepay" }, { "brq_paysafecard", "paysafecard" }, { "brq_bitcoin", "bitcoin" }, { "brq_bancontact", "bancontact" } });
        this.PaymentMethod = paymentMethod ?? this.PaymentMethod;

        var statusCode = Convert.ToInt32(notificationData["brq_statuscode"] ?? 0);
        if (new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }.Contains(statusCode)) {
            this.SetPending();
        }
        else if (new List<int>() { 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199 }.Contains(statusCode)) {
            this.SetDone();
        }
        else if (new List<int>() { 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239 }.Contains(statusCode)) {
            this.SetCanceled();
        }
        else if (new List<int>() { 300, 301, 302, 303, 304, 305, 306, 307, 308, 309, 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 320, 321, 322, 323, 324, 325, 326, 327, 328, 329, 330, 331, 332, 333, 334, 335, 336, 337, 338, 339, 340, 341, 342, 343, 344, 345, 346, 347, 348, 349, 350, 351, 352, 353, 354, 355, 356, 357, 358, 359, 360, 361, 362, 363, 364, 365, 366, 367, 368, 369, 370, 371, 372, 373, 374, 375, 376, 377, 378, 379, 380, 381, 382, 383, 384, 385, 386, 387, 388, 389, 390, 391, 392, 393, 394, 395, 396, 397, 398, 399 }.Contains(statusCode)) {
            this.SetError(string.Format("Your payment was refused (code {0}). Please try again.", statusCode));
        }
        else if (new List<int>() { 400, 401, 402, 403, 404, 405, 406, 407, 408, 409, 410, 411, 412, 413, 414, 415, 416, 417, 418, 419, 420, 421, 422, 423, 424, 425, 426, 427, 428, 429, 430, 431, 432, 433, 434, 435, 436, 437, 438, 439, 440, 441, 442, 443, 444, 445, 446, 447, 448, 449, 450, 451, 452, 453, 454, 455, 456, 457, 458, 459, 460, 461, 462, 463, 464, 465, 466, 467, 468, 469, 470, 471, 472, 473, 474, 475, 476, 477, 478, 479, 480, 481, 482, 483, 484, 485, 486, 487, 488, 489, 490, 491, 492, 493, 494, 495, 496, 497, 498, 499 }.Contains(statusCode)) {
            this.SetError(string.Format("An error occurred during processing of your payment (code {0}). Please try again.", statusCode));
        }
        else {
            Env.Logger.Warning("received data with invalid payment status ({0}) for transaction with reference {1}", statusCode, this.Reference);
            this.SetError(string.Format("Buckaroo: Unknown status code: {0}", statusCode));
        }
    }

    private void SetPending() {
        this.State = Payment.TransactionState.Pending;
    }

    private void SetDone() {
        this.State = Payment.TransactionState.Done;
    }

    private void SetCanceled() {
        this.State = Payment.TransactionState.Canceled;
    }

    private void SetError(string errorMessage) {
        this.State = Payment.TransactionState.Error;
        this.ErrorMessage = errorMessage;
    }
}
