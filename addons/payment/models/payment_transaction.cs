csharp
public partial class PaymentTransaction {
    public PaymentTransaction() { }

    public virtual void ComputeRefundsCount() {
        // Implementation for _compute_refunds_count
    }

    public virtual void CheckStateAuthorizedSupported() {
        // Implementation for _check_state_authorized_supported
    }

    public virtual void CheckTokenIsActive() {
        // Implementation for _check_token_is_active
    }

    public virtual void ActionViewRefunds() {
        // Implementation for action_view_refunds
    }

    public virtual void ActionCapture() {
        // Implementation for action_capture
    }

    public virtual void ActionVoid() {
        // Implementation for action_void
    }

    public virtual void ActionRefund(decimal amountToRefund = 0) {
        // Implementation for action_refund
    }

    public virtual string ComputeReference(string providerCode, string prefix = null, string separator = "-", params object[] kwargs) {
        // Implementation for _compute_reference
    }

    public virtual string ComputeReferencePrefix(string providerCode, string separator, params object[] values) {
        // Implementation for _compute_reference_prefix
    }

    public virtual Dictionary<string, object> GetProcessingValues() {
        // Implementation for _get_processing_values
    }

    public virtual Dictionary<string, object> GetSpecificProcessingValues(Dictionary<string, object> processingValues) {
        // Implementation for _get_specific_processing_values
    }

    public virtual Dictionary<string, object> GetSpecificRenderingValues(Dictionary<string, object> processingValues) {
        // Implementation for _get_specific_rendering_values
    }

    public virtual Dictionary<string, object> GetMandateValues() {
        // Implementation for _get_mandate_values
    }

    public virtual void SendPaymentRequest() {
        // Implementation for _send_payment_request
    }

    public virtual PaymentTransaction SendRefundRequest(decimal amountToRefund = 0) {
        // Implementation for _send_refund_request
    }

    public virtual PaymentTransaction SendCaptureRequest(decimal amountToCapture = 0) {
        // Implementation for _send_capture_request
    }

    public virtual PaymentTransaction SendVoidRequest(decimal amountToVoid = 0) {
        // Implementation for _send_void_request
    }

    public virtual void EnsureProviderIsNotDisabled() {
        // Implementation for _ensure_provider_is_not_disabled
    }

    public virtual PaymentTransaction CreateChildTransaction(decimal amount, bool isRefund = false, params object[] customCreateValues) {
        // Implementation for _create_child_transaction
    }

    public virtual PaymentTransaction HandleNotificationData(string providerCode, Dictionary<string, object> notificationData) {
        // Implementation for _handle_notification_data
    }

    public virtual PaymentTransaction GetTxFromNotificationData(string providerCode, Dictionary<string, object> notificationData) {
        // Implementation for _get_tx_from_notification_data
    }

    public virtual void ProcessNotificationData(Dictionary<string, object> notificationData) {
        // Implementation for _process_notification_data
    }

    public virtual PaymentTransaction SetPending(string stateMessage = null, params string[] extraAllowedStates) {
        // Implementation for _set_pending
    }

    public virtual PaymentTransaction SetAuthorized(string stateMessage = null, params string[] extraAllowedStates) {
        // Implementation for _set_authorized
    }

    public virtual PaymentTransaction SetDone(string stateMessage = null, params string[] extraAllowedStates) {
        // Implementation for _set_done
    }

    public virtual PaymentTransaction SetCanceled(string stateMessage = null, params string[] extraAllowedStates) {
        // Implementation for _set_canceled
    }

    public virtual PaymentTransaction SetError(string stateMessage, params string[] extraAllowedStates) {
        // Implementation for _set_error
    }

    public virtual PaymentTransaction UpdateState(List<string> allowedStates, string targetState, string stateMessage) {
        // Implementation for _update_state
    }

    public virtual void UpdateSourceTransactionState() {
        // Implementation for _update_source_transaction_state
    }

    public virtual void CronPostProcess() {
        // Implementation for _cron_post_process
    }

    public virtual void PostProcess() {
        // Implementation for _post_process
    }

    public virtual void LogSentMessage() {
        // Implementation for _log_sent_message
    }

    public virtual void LogReceivedMessage() {
        // Implementation for _log_received_message
    }

    public virtual void LogMessageOnLinkedDocuments(string message) {
        // Implementation for _log_message_on_linked_documents
    }

    public virtual string GetSentMessage() {
        // Implementation for _get_sent_message
    }

    public virtual string GetReceivedMessage() {
        // Implementation for _get_received_message
    }

    public virtual PaymentTransaction GetLast() {
        // Implementation for _get_last
    }
}
