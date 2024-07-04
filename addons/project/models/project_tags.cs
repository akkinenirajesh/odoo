csharp
public partial class ProjectTags {
    public int GetDefaultColor() {
        return new Random().Next(1, 12);
    }

    public void ReadGroup(
      List<object> domain,
      List<string> fields,
      string groupby,
      int offset = 0,
      int limit = -1,
      string orderby = null,
      bool lazy = true
    ) {
      if (Env.Context.ContainsKey("projectId")) {
        var tagIds = _NameSearch("");
        domain.Add(new List<object>() { "Id", "in", tagIds });
      }
      // TODO: Call super() method
    }

    public List<object> SearchRead(
      List<object> domain = null,
      List<string> fields = null,
      int offset = 0,
      int limit = -1,
      string order = null
    ) {
      if (Env.Context.ContainsKey("projectId")) {
        var tagIds = _NameSearch("");
        domain.Add(new List<object>() { "Id", "in", tagIds });
        var result = ArrangeTagListById(
          // TODO: Call super() method
          tagIds
        );
        return result;
      }
      // TODO: Call super() method
    }

    private List<object> ArrangeTagListById(List<object> tagList, List<int> idOrder) {
      var tagsById = new Dictionary<int, object>();
      foreach (var tag in tagList) {
        tagsById[(int)tag["Id"]] = tag;
      }
      return idOrder.Where(id => tagsById.ContainsKey(id)).Select(id => tagsById[id]).ToList();
    }

    private List<int> _NameSearch(string name, List<object> domain = null, string operator = "ilike", int limit = -1, string order = null) {
      var ids = new List<int>();
      if (!(name == "" && (operator == "like" || operator == "ilike"))) {
        if (domain == null) {
          domain = new List<object>();
        }
        domain.Add(new List<object>() { "Name", operator, name });
      }
      if (Env.Context.ContainsKey("projectId")) {
        // TODO: Execute SQL query to get tags from last 1000 tasks
        // Add the retrieved ids to the list: ids.AddRange(...)
      }
      if (limit == -1 || ids.Count < limit) {
        limit = limit == -1 ? -1 : limit - ids.Count;
        // TODO: Use Search method with the domain and limit
      }
      return ids;
    }

    public int NameCreate(string name) {
      var existingTag = Env["Project.ProjectTags"].Search(new List<object>() {
        new List<object>() { "Name", "ilike", name.Trim() }
      }, limit: 1);
      if (existingTag.Count > 0) {
        return (int)existingTag[0]["Id"];
      }
      // TODO: Call super() method
    }
}
