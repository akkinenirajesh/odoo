csharp
public partial class ChallengeLine
{
    public override string ToString()
    {
        return Name;
    }

    public void OnChange_DefinitionId()
    {
        if (DefinitionId != null)
        {
            Name = DefinitionId.Name;
            Condition = DefinitionId.Condition;
            DefinitionSuffix = DefinitionId.Suffix;
            DefinitionMonetary = DefinitionId.Monetary;
            DefinitionFullSuffix = DefinitionId.FullSuffix;
        }
        else
        {
            Name = null;
            Condition = null;
            DefinitionSuffix = null;
            DefinitionMonetary = false;
            DefinitionFullSuffix = null;
        }
    }
}
