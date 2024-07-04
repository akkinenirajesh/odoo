csharp
public partial class MailTestSimple 
{
    public void _MessageComputeSubject()
    {
        // implement your method here
    }

    public void _NotifyByEmailGetFinalMailValues(params object[] args)
    {
        // implement your method here
    }

    public void _NotifyByEmailGetHeaders(string headers = null)
    {
        // implement your method here
    }
}

public partial class MailTestSimpleWithMainAttachment
{
    // inherit MailTestSimple methods and add your own
}

public partial class MailTestSimpleUnfollow
{
    // inherit MailTestSimple methods and add your own
}

public partial class MailTestAliasOptional
{
    public void _AliasGetCreationValues()
    {
        // implement your method here
    }
}

public partial class MailTestGateway 
{
    public void MessageNew(dynamic msgDict, dynamic customValues = null)
    {
        // implement your method here
    }
}

public partial class MailTestGatewayCompany 
{
    // inherit MailTestGateway methods and add your own
}

public partial class MailTestGatewayMainAttachment
{
    // inherit MailTestGateway methods and add your own
}

public partial class MailTestGatewayGroups 
{
    public void _AliasGetCreationValues()
    {
        // implement your method here
    }

    public void _MailGetPartnerFields(bool introspectFields = false)
    {
        // implement your method here
    }

    public void _MessageGetDefaultRecipients()
    {
        // implement your method here
    }
}

public partial class MailTestStandard 
{
    public void _TrackFilterForDisplay(dynamic trackingValues)
    {
        // implement your method here
    }

    public void _TrackGetDefaultLogMessage(dynamic changes)
    {
        // implement your method here
    }
}

public partial class MailTestActivity 
{
    public void ActionStart(string actionSummary)
    {
        // implement your method here
    }

    public void ActionClose(string actionFeedback, dynamic attachmentIds = null)
    {
        // implement your method here
    }
}

public partial class MailTestTicket 
{
    public void _MailGetPartnerFields(bool introspectFields = false)
    {
        // implement your method here
    }

    public void _MessageComputeSubject()
    {
        // implement your method here
    }

    public void _MessageGetDefaultRecipients()
    {
        // implement your method here
    }

    public void _NotifyGetRecipientsGroups(dynamic message, string modelDescription, dynamic msgVals = null)
    {
        // implement your method here
    }

    public void _TrackTemplate(dynamic changes)
    {
        // implement your method here
    }

    public void _CreationSubtype()
    {
        // implement your method here
    }

    public void _TrackSubtype(dynamic initValues)
    {
        // implement your method here
    }

    public void _GetCustomerInformation()
    {
        // implement your method here
    }

    public void _MessageGetSuggestedRecipients()
    {
        // implement your method here
    }
}

public partial class MailTestTicketEL 
{
    // inherit MailTestTicket methods and add your own
}

public partial class MailTestTicketMC 
{
    // inherit MailTestTicket methods and add your own
}

public partial class MailTestContainer 
{
    public void _MailGetPartnerFields(bool introspectFields = false)
    {
        // implement your method here
    }

    public void _MessageGetDefaultRecipients()
    {
        // implement your method here
    }

    public void _NotifyGetRecipientsGroups(dynamic message, string modelDescription, dynamic msgVals = null)
    {
        // implement your method here
    }

    public void _AliasGetCreationValues()
    {
        // implement your method here
    }
}

public partial class MailTestContainerMC 
{
    // inherit MailTestContainer methods and add your own
}

public partial class MailTestComposerMixin
{
    public void _ComputeRenderModel()
    {
        // implement your method here
    }
}

public partial class MailTestComposerSource 
{
    public void _MailGetPartnerFields(bool introspectFields = false)
    {
        // implement your method here
    }
}

public partial class MailTestMailTrackingDuration 
{
    public void _MailGetPartnerFields(bool introspectFields = false)
    {
        // implement your method here
    }
}
