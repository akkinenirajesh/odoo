C#
public partial class RestaurantFloor {
    public virtual void RenameFloor(string newName) {
        this.Name = newName;
    }

    public virtual Dictionary<string, object> SyncFromUI(string name, string backgroundColor, int configId) {
        var floorFields = new Dictionary<string, object>() {
            {"Name", name},
            {"BackgroundColor", backgroundColor},
        };
        var posFloor = Env.Create<RestaurantFloor>(floorFields);
        posFloor.PosConfigIds = new List<int>() { configId };
        return new Dictionary<string, object>() {
            {"id", posFloor.Id},
            {"name", posFloor.Name},
            {"backgroundColor", posFloor.BackgroundColor},
            {"tableIds", new List<int>()},
            {"sequence", posFloor.Sequence},
            {"tables", new List<object>()},
        };
    }

    public virtual bool DeactivateFloor(int sessionId) {
        var draftOrders = Env.Search<Pos.PosOrder>(x => x.SessionId == sessionId && x.State == "draft" && x.TableId.FloorId == this.Id);
        if (draftOrders.Count > 0) {
            throw new Exception("You cannot delete a floor when orders are still in draft for this floor.");
        }
        foreach (var table in this.TableIds) {
            table.Active = false;
        }
        this.Active = false;
        return true;
    }
}

public partial class RestaurantTable {
    public virtual bool AreOrdersStillInDraft() {
        var draftOrdersCount = Env.SearchCount<Pos.PosOrder>(x => x.TableId.IsIn(this.Id) && x.State == "draft");
        if (draftOrdersCount > 0) {
            throw new Exception("You cannot delete a table when orders are still in draft for this table.");
        }
        return true;
    }

    public virtual void UpdateTables(Dictionary<string, object> tablesById) {
        this.Write(tablesById[this.Id.ToString()]);
    }
}
