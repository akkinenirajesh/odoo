C#
public partial class Boolean {
    public int Const { get; set; }
    public bool Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.Boolean:{this.Value}";
    }

    public List<object> NameSearch(string name, List<object> domain = null, string operator = "ilike", int limit = 0, string order = null) {
        if (name is string s && s.Split(':')[0] == "TestImpex.Boolean") {
            return Env.Search("TestImpex.Boolean", $"Value {operator} {int.Parse(s.Split(':')[1])}", limit, order);
        }
        else {
            return new List<object>();
        }
    }
}

public partial class Integer {
    public int Const { get; set; }
    public int Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.Integer:{this.Value}";
    }

    public List<object> NameSearch(string name, List<object> domain = null, string operator = "ilike", int limit = 0, string order = null) {
        if (name is string s && s.Split(':')[0] == "TestImpex.Integer") {
            return Env.Search("TestImpex.Integer", $"Value {operator} {int.Parse(s.Split(':')[1])}", limit, order);
        }
        else {
            return new List<object>();
        }
    }
}

public partial class Float {
    public int Const { get; set; }
    public double Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.Float:{this.Value}";
    }
}

public partial class Decimal {
    public int Const { get; set; }
    public double Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.Decimal:{this.Value}";
    }
}

public partial class StringBounded {
    public int Const { get; set; }
    public string Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.StringBounded:{this.Value}";
    }
}

public partial class StringRequired {
    public int Const { get; set; }
    public string Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.StringRequired:{this.Value}";
    }
}

public partial class String {
    public int Const { get; set; }
    public string Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.String:{this.Value}";
    }
}

public partial class Date {
    public int Const { get; set; }
    public DateTime Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.Date:{this.Value}";
    }
}

public partial class Datetime {
    public int Const { get; set; }
    public DateTime Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.Datetime:{this.Value}";
    }
}

public partial class Text {
    public int Const { get; set; }
    public string Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.Text:{this.Value}";
    }
}

public partial class Selection {
    public int Const { get; set; }
    public string Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.Selection:{this.Value}";
    }
}

public partial class SelectionFunction {
    public int Const { get; set; }
    public string Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.SelectionFunction:{this.Value}";
    }
}

public partial class Many2one {
    public int Const { get; set; }
    public Integer Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.Many2one:{this.Value.Value}";
    }
}

public partial class One2many {
    public int Const { get; set; }
    public List<One2manyChild> Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.One2many:{this.Value[0].Value}";
    }
}

public partial class One2manyChild {
    public One2many Parent { get; set; }
    public string Str { get; set; }
    public Integer M2o { get; set; }
    public int Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.One2manyChild:{this.Value}";
    }

    public List<object> NameSearch(string name, List<object> domain = null, string operator = "ilike", int limit = 0, string order = null) {
        if (name is string s && s.Split(':')[0] == "TestImpex.One2manyChild") {
            return Env.Search("TestImpex.One2manyChild", $"Value {operator} {int.Parse(s.Split(':')[1])}", limit, order);
        }
        else {
            return new List<object>();
        }
    }
}

public partial class Many2many {
    public int Const { get; set; }
    public List<Many2manyOther> Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.Many2many:{this.Value[0].Value}";
    }
}

public partial class Many2manyOther {
    public string Str { get; set; }
    public int Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.Many2manyOther:{this.Value}";
    }

    public List<object> NameSearch(string name, List<object> domain = null, string operator = "ilike", int limit = 0, string order = null) {
        if (name is string s && s.Split(':')[0] == "TestImpex.Many2manyOther") {
            return Env.Search("TestImpex.Many2manyOther", $"Value {operator} {int.Parse(s.Split(':')[1])}", limit, order);
        }
        else {
            return new List<object>();
        }
    }
}

public partial class Function {
    public int Const { get; set; }
    public int Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.Function:{this.Value}";
    }

    public void ComputeValue() {
        this.Value = 3;
    }
}

public partial class Reference {
    public int Const { get; set; }
    public object Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.Reference:{this.Value}";
    }
}

public partial class One2manyMultiple {
    public One2manyRecursive Parent { get; set; }
    public int Const { get; set; }
    public List<One2manyChild1> Child1 { get; set; }
    public List<One2manyChild2> Child2 { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.One2manyMultiple:{this.Parent.Value}";
    }
}

public partial class One2manyMultipleChild {
    public One2manyMultiple Parent { get; set; }
    public string Str { get; set; }
    public int Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.One2manyMultipleChild:{this.Value}";
    }
}

public partial class One2manyChild1 {
    public One2manyMultiple Parent { get; set; }
    public string Str { get; set; }
    public int Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.One2manyChild1:{this.Value}";
    }
}

public partial class One2manyChild2 {
    public One2manyMultiple Parent { get; set; }
    public string Str { get; set; }
    public int Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.One2manyChild2:{this.Value}";
    }
}

public partial class SelectionWithDefault {
    public int Const { get; set; }
    public string Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.SelectionWithDefault:{this.Value}";
    }
}

public partial class One2manyRecursive {
    public int Value { get; set; }
    public List<One2manyMultiple> Child { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.One2manyRecursive:{this.Value}";
    }
}

public partial class Unique {
    public int Value { get; set; }
    public int Value2 { get; set; }
    public int Value3 { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.Unique:{this.Value}";
    }
}

public partial class InheritsParent {
    public int ValueParent { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.InheritsParent:{this.ValueParent}";
    }
}

public partial class InheritsChild {
    public InheritsParent Parent { get; set; }
    public int Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.InheritsChild:{this.Value}";
    }
}

public partial class M2oStr {
    public M2oStrChild Child { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.M2oStr:{this.Child.Name}";
    }
}

public partial class M2oStrChild {
    public string Name { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.M2oStrChild:{this.Name}";
    }
}

public partial class WithRequiredField {
    public string Name { get; set; }
    public int Value { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.WithRequiredField:{this.Value}";
    }
}

public partial class Many2oneRequiredSubfield {
    public WithRequiredField Name { get; set; }

    public void ComputeDisplayName() {
        this.DisplayName = $"TestImpex.Many2oneRequiredSubfield:{this.Name.Value}";
    }
}
