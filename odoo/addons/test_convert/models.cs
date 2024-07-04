csharp
public partial class TestModel {
    public bool ActionTestDate(DateTime todayDate) {
        return true;
    }

    public bool ActionTestTime(DateTime curTime) {
        return true;
    }

    public bool ActionTestTimezone(string timezone) {
        return true;
    }
}

public partial class Usered {
    public (Usered, object[], Dictionary<string, object>) ModelMethod(params object[] args, Dictionary<string, object> kwargs) {
        return (this, args, kwargs);
    }

    public (Usered, object[], Dictionary<string, object>) Method(params object[] args, Dictionary<string, object> kwargs) {
        return (this, args, kwargs);
    }
}
