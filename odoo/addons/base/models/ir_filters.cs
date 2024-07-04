C#
public partial class IrFilters {
  public IrFilters() { }

  public int Id { get; set; }

  public string Name { get; set; }

  public ResUsers UserId { get; set; }

  public string Domain { get; set; }

  public string Context { get; set; }

  public string Sort { get; set; }

  public string ModelId { get; set; }

  public bool IsDefault { get; set; }

  public IrActionsActions ActionId { get; set; }

  public IrEmbeddedActions EmbeddedActionId { get; set; }

  public int EmbeddedParentResId { get; set; }

  public bool Active { get; set; }

  public List<string> ListAllModels() {
    // TODO: implement ListAllModels method
    return new List<string>();
  }

  public void CopyData(Dictionary<string, object> defaultValues = null) {
    // TODO: implement CopyData method
  }

  public string GetEvalDomain() {
    // TODO: implement GetEvalDomain method
    return string.Empty;
  }

  public List<string> GetActionDomain(int actionId = 0, int embeddedActionId = 0, int embeddedParentResId = 0) {
    // TODO: implement GetActionDomain method
    return new List<string>();
  }

  public List<Dictionary<string, object>> GetFilters(string model, int actionId = 0, int embeddedActionId = 0, int embeddedParentResId = 0) {
    // TODO: implement GetFilters method
    return new List<Dictionary<string, object>>();
  }

  public void CheckGlobalDefault(Dictionary<string, object> vals, List<Dictionary<string, object>> matchingFilters) {
    // TODO: implement CheckGlobalDefault method
  }

  public IrFilters CreateOrReplace(Dictionary<string, object> vals) {
    // TODO: implement CreateOrReplace method
    return new IrFilters();
  }
}
