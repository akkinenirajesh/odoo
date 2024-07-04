C#
public partial class Website {
  public object SearchGetDetails(string searchType, string order, object options) {
    var result = Env.Call("super", "_search_get_details", searchType, order, options);
    if (searchType == "test") {
      result.Append(Env.Call("Test.TestModel", "_search_get_detail", this, order, options));
    }
    return result;
  }
}
