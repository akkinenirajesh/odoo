csharp
public partial class ResPartnerAutocompleteSync
{
    public void StartSync(int batchSize = 1000)
    {
        var toSyncItems = Env.Search<ResPartnerAutocompleteSync>(x => !x.Synched, batchSize);

        foreach (var toSyncItem in toSyncItems)
        {
            var partner = Env.Get<ResPartner>(toSyncItem.PartnerId);

            var params = new Dictionary<string, object>
            {
                { "partner_gid", partner.PartnerGid }
            };

            if (partner.Vat != null && partner.IsVatSyncable(partner.Vat))
            {
                params["vat"] = partner.Vat;
                var (result, error) = Env.Get<IapAutocompleteApi>().RequestPartnerAutocomplete("update", params);
                if (error != null)
                {
                    Env.LogWarning($"Send Partner to sync failed: {error}");
                }
            }

            toSyncItem.Synched = true;
            Env.Save(toSyncItem);
        }
        var done = toSyncItems.Count;
        Env.Get<IrCron>().NotifyProgress(done, done < batchSize ? 0 : Env.SearchCount<ResPartnerAutocompleteSync>(x => !x.Synched));
    }

    public ResPartnerAutocompleteSync AddToQueue(int partnerId)
    {
        var toSync = Env.Search<ResPartnerAutocompleteSync>(x => x.PartnerId == partnerId);
        if (toSync.Count == 0)
        {
            toSync = Env.Create<ResPartnerAutocompleteSync>(new ResPartnerAutocompleteSync { PartnerId = partnerId });
        }
        return toSync.First();
    }
}
