csharp
public partial class WebHierarchyView {
    public virtual void ValidateTagHierarchy(XmlNode node, NameManager nameManager, NodeInfo nodeInfo) {
        if (!nodeInfo.Validate) {
            return;
        }

        int templatesCount = 0;
        foreach (XmlNode child in node.ChildNodes) {
            if (child.Name == "templates") {
                if (templatesCount == 0) {
                    templatesCount++;
                } else {
                    string msg = Env.Translate("Hierarchy view can contain only one templates tag");
                    RaiseViewError(msg, child);
                }
            } else if (child.Name != "field") {
                string msg = Env.Translate("Hierarchy child can only be field or template, got {0}", child.Name);
                RaiseViewError(msg, child);
            }
        }

        List<string> remaining = node.Attributes.Cast<XmlAttribute>().Select(a => a.Name).Except(HIERARCHY_VALID_ATTRIBUTES).ToList();
        if (remaining.Count > 0) {
            string msg = Env.Translate(
                "Invalid attributes ({0}) in hierarchy view. Attributes must be in ({1})",
                string.Join(", ", remaining),
                string.Join(", ", HIERARCHY_VALID_ATTRIBUTES)
            );
            RaiseViewError(msg, node);
        }
    }

    private void RaiseViewError(string msg, XmlNode node) {
        // Implement error handling logic here
        throw new Exception(msg);
    }
}
