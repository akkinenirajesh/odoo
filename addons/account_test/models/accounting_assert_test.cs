csharp
public partial class AccountingAssertTest
{
    public override string ToString()
    {
        return Name;
    }

    public void ExecuteTest()
    {
        // Implementation for executing the test
        // This would involve parsing and executing the CodeExec
        // and handling the result
    }

    public static IEnumerable<AccountingAssertTest> GetActiveTests()
    {
        return Env.Query<AccountingAssertTest>()
            .Where(t => t.Active)
            .OrderBy(t => t.Sequence);
    }
}
