csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.IrMailServer
{
    public partial class IrMailServer
    {
        // All the model methods are written here.
        public void ComputeSmtpAuthenticationInfo()
        {
            if (this.SmtpAuthentication == "Login")
            {
                this.SmtpAuthenticationInfo = "Connect to your server through your usual username and password. \n This is the most basic SMTP authentication process and may not be accepted by all providers. \n";
            }
            else if (this.SmtpAuthentication == "Certificate")
            {
                this.SmtpAuthenticationInfo = "Authenticate by using SSL certificates, belonging to your domain name. \n SSL certificates allow you to authenticate your mail server for the entire domain name.";
            }
            else if (this.SmtpAuthentication == "Cli")
            {
                this.SmtpAuthenticationInfo = "Use the SMTP configuration set in the &quot;Command Line Interface&quot; arguments.";
            }
            else
            {
                this.SmtpAuthentication = null;
            }
        }
    }
}
