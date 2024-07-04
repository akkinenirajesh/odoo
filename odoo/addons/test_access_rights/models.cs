csharp
public partial class TestAccessRightSomeObj {
    // all the model methods are written here.
}

public partial class TestAccessRightContainer {
    // all the model methods are written here.
}

public partial class TestAccessRightInherits {
    // all the model methods are written here.
}

public partial class TestAccessRightChild {
    // all the model methods are written here.
}

public partial class TestAccessRightObjCateg {
    // all the model methods are written here.
    public virtual List<TestAccessRightObjCateg> SearchFetch(List<object> domain, List<string> fieldNames, int offset = 0, int? limit = null, string order = null) {
        if (Env.Context.Get("OnlyMedia") != null) {
            domain.Add(new object[] { "Name", "=", "Media" });
        }
        return base.SearchFetch(domain, fieldNames, offset, limit, order);
    }
}

public partial class TestAccessRightFakeTicket {
    // all the model methods are written here.
}

public partial class ResPartner {
    // all the model methods are written here.
    public virtual void GetCompanyCurrency() {
        this.CurrencyId = this.Company.CurrencyId;
    }
}
