csharp
public partial class LeadRequest
{
    private const int RegistrationsBatchSize = 200;

    public override string ToString()
    {
        return EventId.ToString();
    }

    public void CronGenerateLeads(int jobLimit = 100, int? registrationsBatchSize = null)
    {
        registrationsBatchSize ??= RegistrationsBatchSize;
        var generateRequests = Env.Set<LeadRequest>().Search(new object[] { }, limit: jobLimit);
        var fulfilledRequests = new List<LeadRequest>();

        foreach (var generateRequest in generateRequests)
        {
            var registrationsToProcess = Env.Set<Event.Registration>().Search(new object[]
            {
                new object[] { "EventId", "=", generateRequest.EventId.Id },
                new object[] { "State", "not in", new[] { "draft", "cancel" } },
                new object[] { "Id", ">", generateRequest.ProcessedRegistrationId }
            }, limit: registrationsBatchSize.Value, orderBy: "Id asc");

            registrationsToProcess.ApplyLeadGenerationRules();

            if (registrationsToProcess.Count() < registrationsBatchSize)
            {
                fulfilledRequests.Add(generateRequest);
            }
            else
            {
                generateRequest.ProcessedRegistrationId = registrationsToProcess.Last().Id;
            }

            // Note: Auto-commit logic is not implemented here as it's database-specific
        }

        if (generateRequests.Except(fulfilledRequests).Any())
        {
            Env.Ref<IrCron>("Event.IrCronGenerateLeads").Trigger();
        }

        if (fulfilledRequests.Any())
        {
            Env.Set<LeadRequest>().Delete(fulfilledRequests);
        }
    }
}
