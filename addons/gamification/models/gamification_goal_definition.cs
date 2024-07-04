csharp
public partial class GoalDefinition
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeFullSuffix()
    {
        var items = new List<string>();

        if (Monetary)
        {
            items.Add(Env.Company.Currency.Symbol ?? "¤");
        }
        if (!string.IsNullOrEmpty(Suffix))
        {
            items.Add(Suffix);
        }

        FullSuffix = string.Join(" ", items);
    }

    public void CheckDomainValidity()
    {
        if (ComputationMode != ComputationMode.Count && ComputationMode != ComputationMode.Sum)
        {
            return;
        }

        try
        {
            var domain = SafeEval.Eval(Domain, new { user = Env.User });
            var obj = Env.Get(Model.Model);
            obj.SearchCount(domain); // Dummy search to make sure the domain is valid
        }
        catch (Exception e)
        {
            var msg = e is SyntaxException syntaxEx ? $"{syntaxEx.Message}\n{syntaxEx.Text}" : e.Message;
            throw new UserException($"The domain for the definition {Name} seems incorrect, please check it.\n\n{msg}");
        }
    }

    public void CheckModelValidity()
    {
        try
        {
            if (Model == null || Field == null)
            {
                return;
            }

            var model = Env.Get(Model.Model);
            var field = model.Fields.Get(Field.Name);
            if (field == null || !field.Store)
            {
                throw new UserException($"The model configuration for the definition {Name} seems incorrect, please check it.\n\n{Field.Name} not stored");
            }
        }
        catch (KeyNotFoundException e)
        {
            throw new UserException($"The model configuration for the definition {Name} seems incorrect, please check it.\n\n{e.Message} not found");
        }
    }

    public override void OnCreate()
    {
        base.OnCreate();
        if (ComputationMode == ComputationMode.Count || ComputationMode == ComputationMode.Sum)
        {
            CheckDomainValidity();
        }
        if (Field != null)
        {
            CheckModelValidity();
        }
    }

    public override void OnWrite()
    {
        base.OnWrite();
        if ((ComputationMode == ComputationMode.Count || ComputationMode == ComputationMode.Sum) &&
            (IsFieldChanged(nameof(Domain)) || IsFieldChanged(nameof(Model))))
        {
            CheckDomainValidity();
        }
        if (IsFieldChanged(nameof(Field)) || IsFieldChanged(nameof(Model)) || IsFieldChanged(nameof(BatchMode)))
        {
            CheckModelValidity();
        }
    }
}
