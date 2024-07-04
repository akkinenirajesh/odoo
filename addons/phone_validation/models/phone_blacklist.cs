csharp
public partial class PhoneBlackList {
    public PhoneBlackList Add(string number, string message = null) {
        string sanitized = Env.User.PhoneFormat(number);
        return _Add(new List<string> { sanitized }, message);
    }

    private PhoneBlackList _Add(List<string> numbers, string message = null) {
        // Log on existing records
        var existing = Env.Model<PhoneBlackList>().Search(x => numbers.Contains(x.Number));
        if (existing != null && !string.IsNullOrEmpty(message)) {
            existing.TrackSetLogMessage(message);
        }

        var records = Env.Model<PhoneBlackList>().Create(numbers.Select(n => new PhoneBlackList { Number = n }).ToList());

        // Post message on new records
        var newRecords = records.Where(x => !existing.Contains(x)).ToList();
        if (newRecords.Any() && !string.IsNullOrEmpty(message)) {
            foreach (var record in newRecords) {
                record.WithContext(new Dictionary<string, object> { { "mail_create_nosubscribe", true } }).MessagePost(message, "mail.mt_note");
            }
        }
        return this;
    }

    public PhoneBlackList Remove(string number, string message = null) {
        string sanitized = Env.User.PhoneFormat(number);
        return _Remove(new List<string> { sanitized }, message);
    }

    private PhoneBlackList _Remove(List<string> numbers, string message = null) {
        var records = Env.Model<PhoneBlackList>().Search(x => numbers.Contains(x.Number));
        var todo = numbers.Where(n => !records.Select(x => x.Number).Contains(n)).ToList();
        if (records != null) {
            if (!string.IsNullOrEmpty(message)) {
                records.TrackSetLogMessage(message);
            }
            records.ActionArchive();
        }
        if (todo.Any()) {
            var newRecords = Env.Model<PhoneBlackList>().Create(todo.Select(n => new PhoneBlackList { Number = n, Active = false }).ToList());
            if (!string.IsNullOrEmpty(message)) {
                foreach (var record in newRecords) {
                    record.WithContext(new Dictionary<string, object> { { "mail_create_nosubscribe", true } }).MessagePost(message, "mail.mt_note");
                }
            }
            records.AddRange(newRecords);
        }
        return this;
    }

    public Dictionary<string, object> PhoneActionBlacklistRemove() {
        return new Dictionary<string, object> {
            { "name", "Are you sure you want to unblacklist this phone number?" },
            { "type", "ir.actions.act_window" },
            { "view_mode", "form" },
            { "res_model", "Phone.PhoneBlacklistRemove" },
            { "target", "new" },
            { "context", new Dictionary<string, object> { { "dialog_size", "medium" } } }
        };
    }

    public PhoneBlackList ActionAdd() {
        Add(Number);
        return this;
    }

    public PhoneBlackList Create(List<PhoneBlackList> values) {
        // Extract and sanitize numbers, ensuring uniques
        var toCreate = new List<PhoneBlackList>();
        var done = new HashSet<string>();
        foreach (var value in values) {
            try {
                var sanitizedValue = Env.User.PhoneFormat(value.Number, true);
                if (done.Contains(sanitizedValue)) {
                    continue;
                }
                done.Add(sanitizedValue);
                toCreate.Add(new PhoneBlackList { Number = sanitizedValue, Active = value.Active });
            } catch (Exception err) {
                throw new Exception(err.Message + " Please correct the number and try again.");
            }
        }

        // Search for existing phone blacklist entries, even inactive ones (will be activated again)
        var numbersRequested = toCreate.Select(x => x.Number).ToList();
        var existing = Env.Model<PhoneBlackList>().Search(x => numbersRequested.Contains(x.Number), true);

        // Out of existing pb records, activate non-active, (unless requested to leave them alone with 'active' set to False)
        var numbersToKeepInactive = toCreate.Where(x => !x.Active).Select(x => x.Number).ToHashSet();
        numbersToKeepInactive.IntersectWith(existing.Select(x => x.Number).ToHashSet());
        existing.Where(x => !x.Active && !numbersToKeepInactive.Contains(x.Number)).ToList().ForEach(x => x.Active = true);

        // Create new records, while skipping existing_numbers
        var existingNumbers = existing.Select(x => x.Number).ToHashSet();
        var toCreateFiltered = toCreate.Where(x => !existingNumbers.Contains(x.Number)).ToList();
        var created = Env.Model<PhoneBlackList>().Create(toCreateFiltered);

        // Preserve the original order of numbers requested to create
        var numbersToId = existing.Concat(created).ToDictionary(x => x.Number, x => x.Id);
        return Env.Model<PhoneBlackList>().Browse(numbersRequested.Select(n => numbersToId[n]).ToList());
    }

    public PhoneBlackList Write(Dictionary<string, object> values) {
        if (values.ContainsKey("Number")) {
            try {
                var sanitized = Env.User.PhoneFormat(values["Number"].ToString(), true);
                values["Number"] = sanitized;
            } catch (Exception err) {
                throw new Exception(err.Message + " Please correct the number and try again.");
            }
        }
        return Env.Model<PhoneBlackList>().Browse(Id).Write(values);
    }

    public List<PhoneBlackList> Search(Func<PhoneBlackList, bool> predicate, bool activeTest = false) {
        return Env.Model<PhoneBlackList>().Search(predicate, activeTest);
    }

    public void TrackSetLogMessage(string message) {
        // ... Implementation to track the log message.
    }

    public void MessagePost(string body, string subtypeXmlid) {
        // ... Implementation to post a message.
    }

    public void ActionArchive() {
        // ... Implementation to archive the record.
    }
}
