csharp
public partial class AccountMoveLine
{
    public void ComputeL10nInHsnCode()
    {
        if (Move.CountryCode == "IN" && ParentState == "draft")
        {
            L10nInHsnCode = Product?.L10nInHsnCode;
        }
    }
}
