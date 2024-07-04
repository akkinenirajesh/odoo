csharp
public partial class TestExceptionsModel 
{
    public void GenerateRedirectWarning()
    {
        var action = Env.Ref("test_exceptions.action_test_exceptions");
        throw new RedirectWarningException("description", action.Id, "Go to the redirection");
    }

    public void GenerateAccessDenied()
    {
        throw new AccessDeniedException();
    }

    public void GenerateAccessError()
    {
        throw new AccessErrorException("description");
    }

    public void GenerateExcAccessDenied()
    {
        throw new Exception("AccessDenied");
    }

    public void GenerateUndefined()
    {
        // Accessing undefined symbol.
        this.SurelyUndefinedSymbol;
    }

    public void GenerateUserError()
    {
        throw new UserErrorException("description");
    }

    public void GenerateMissingError()
    {
        throw new MissingErrorException("description");
    }

    public void GenerateValidationError()
    {
        throw new ValidationErrorException("description");
    }

    public void GenerateRedirectWarningSafeEval()
    {
        GenerateSafeEval(GenerateRedirectWarning);
    }

    public void GenerateAccessDeniedSafeEval()
    {
        GenerateSafeEval(GenerateAccessDenied);
    }

    public void GenerateAccessErrorSafeEval()
    {
        GenerateSafeEval(GenerateAccessError);
    }

    public void GenerateExcAccessDeniedSafeEval()
    {
        GenerateSafeEval(GenerateExcAccessDenied);
    }

    public void GenerateUndefinedSafeEval()
    {
        GenerateSafeEval(GenerateUndefined);
    }

    public void GenerateUserErrorSafeEval()
    {
        GenerateSafeEval(GenerateUserError);
    }

    public void GenerateMissingErrorSafeEval()
    {
        GenerateSafeEval(GenerateMissingError);
    }

    public void GenerateValidationErrorSafeEval()
    {
        GenerateSafeEval(GenerateValidationError);
    }

    private void GenerateSafeEval(Action action)
    {
        var globalsDict = new Dictionary<string, object> { { "Generate", action } };
        ScriptEngine.Execute("Generate()", globalsDict);
    }
}
