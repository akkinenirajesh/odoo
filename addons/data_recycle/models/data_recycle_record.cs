csharp
public partial class DataRecycleRecord
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeName()
    {
        var originalRecords = GetOriginalRecords();
        var originalRecord = originalRecords.FirstOrDefault(r => r.GetType().Name == ResModelName && r.Id == ResId);
        
        if (originalRecord != null)
        {
            Name = originalRecord.ToString() ?? "Undefined Name";
        }
        else
        {
            Name = "**Record Deleted**";
        }
    }

    public void ComputeCompanyId()
    {
        var originalRecords = GetOriginalRecords();
        var originalRecord = originalRecords.FirstOrDefault(r => r.GetType().Name == ResModelName && r.Id == ResId);
        
        if (originalRecord != null)
        {
            Company = GetCompanyId(originalRecord);
        }
        else
        {
            Company = null;
        }
    }

    private Res.Company GetCompanyId(dynamic record)
    {
        if (record.GetType().GetProperty("Company") != null)
        {
            return record.Company;
        }
        return null;
    }

    private List<dynamic> GetOriginalRecords()
    {
        if (ResId == 0) return new List<dynamic>();

        var records = new List<dynamic>();
        var recordsPerModel = new Dictionary<string, List<int>>();

        foreach (var record in Env.Set<DataRecycleRecord>().Where(r => !string.IsNullOrEmpty(r.ResModelName)))
        {
            if (!recordsPerModel.ContainsKey(record.ResModelName))
            {
                recordsPerModel[record.ResModelName] = new List<int>();
            }
            recordsPerModel[record.ResModelName].Add(record.ResId);
        }

        foreach (var kvp in recordsPerModel)
        {
            var modelType = Type.GetType(kvp.Key);
            var recs = Env.Set(modelType)
                .WithContext(new { active_test = false })
                .Sudo()
                .Browse(kvp.Value)
                .Where(r => r != null);
            records.AddRange(recs);
        }

        return records;
    }

    public void ActionValidate()
    {
        var recordsDone = new List<DataRecycleRecord>();
        var recordIdsToArchive = new Dictionary<string, List<int>>();
        var recordIdsToUnlink = new Dictionary<string, List<int>>();
        var originalRecords = GetOriginalRecords().ToDictionary(r => $"{r.GetType().Name}_{r.Id}", r => r);

        foreach (var record in this.Env.Set<DataRecycleRecord>())
        {
            var key = $"{record.ResModelName}_{record.ResId}";
            if (originalRecords.TryGetValue(key, out var originalRecord))
            {
                recordsDone.Add(record);
                if (record.RecycleModel.RecycleAction == "archive")
                {
                    if (!recordIdsToArchive.ContainsKey(originalRecord.GetType().Name))
                        recordIdsToArchive[originalRecord.GetType().Name] = new List<int>();
                    recordIdsToArchive[originalRecord.GetType().Name].Add(originalRecord.Id);
                }
                else if (record.RecycleModel.RecycleAction == "unlink")
                {
                    if (!recordIdsToUnlink.ContainsKey(originalRecord.GetType().Name))
                        recordIdsToUnlink[originalRecord.GetType().Name] = new List<int>();
                    recordIdsToUnlink[originalRecord.GetType().Name].Add(originalRecord.Id);
                }
            }
        }

        foreach (var kvp in recordIdsToArchive)
        {
            var modelType = Type.GetType(kvp.Key);
            Env.Set(modelType).Sudo().Browse(kvp.Value).ToggleActive();
        }

        foreach (var kvp in recordIdsToUnlink)
        {
            var modelType = Type.GetType(kvp.Key);
            Env.Set(modelType).Sudo().Browse(kvp.Value).Unlink();
        }

        foreach (var record in recordsDone)
        {
            record.Unlink();
        }
    }

    public void ActionDiscard()
    {
        Active = false;
    }
}
