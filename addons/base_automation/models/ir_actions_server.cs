csharp
using System;
using System.Linq;
using Core;

namespace BaseAutomation
{
    public partial class ServerAction
    {
        public void CheckModelCoherencyWithAutomation()
        {
            if (BaseAutomation != null && Model != BaseAutomation.Model)
            {
                throw new ValidationException(
                    string.Format("Model of action {0} should match the one from automated rule {1}.",
                        Name,
                        BaseAutomation.Name
                    )
                );
            }
        }

        public void ComputeAvailableModelIds()
        {
            // Implement the logic to compute available model IDs
            // This method would typically set a property like AvailableModels
            if (Usage == ServerActionUsage.BaseAutomation && BaseAutomation?.Model != null)
            {
                AvailableModels = new[] { BaseAutomation.Model };
            }
            else
            {
                // Implement the default logic here
            }
        }

        public void ComputeName()
        {
            if (BaseAutomation == null)
            {
                Name = Name ?? string.Empty;
                return;
            }

            switch (State)
            {
                case "object_write":
                    var actionType = EvaluationType == "value" ? "Update" : "Compute";
                    Name = $"{actionType} {StringifyPath()}";
                    break;
                case "object_create":
                    Name = $"Create {CrudModel?.Name} with name {Value}";
                    break;
                case "webhook":
                    Name = "Send Webhook Notification";
                    break;
                case "sms":
                    Name = $"Send SMS: {SmsTemplate?.Name}";
                    break;
                case "mail_post":
                    Name = $"Send email: {Template?.Name}";
                    break;
                case "followers":
                    Name = $"Add followers: {string.Join(", ", Partners.Select(p => p.Name))}";
                    break;
                case "remove_followers":
                    Name = $"Remove followers: {string.Join(", ", Partners.Select(p => p.Name))}";
                    break;
                case "next_activity":
                    Name = $"Create activity: {ActivitySummary ?? ActivityType?.Name}";
                    break;
                default:
                    // Implement logic to get the state description
                    Name = "Default Action Name";
                    break;
            }
        }

        private string StringifyPath()
        {
            // Implement the logic to stringify the path
            return "Path"; // Placeholder
        }

        public object GetEvalContext(ServerAction action = null)
        {
            var evalContext = new
            {
                // Add properties that would typically be in the eval context
            };

            if (action != null && action.State == "code")
            {
                // Add json and payload to the context
                // Note: You'll need to implement json_scriptsafe and get_webhook_request_payload equivalents
            }

            return evalContext;
        }
    }
}
