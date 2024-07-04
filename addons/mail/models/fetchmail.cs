csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mail.FetchmailServer
{
    public partial class FetchmailServer
    {
        public void Connect(bool allowArchived = false)
        {
            if (!allowArchived && !this.Active)
            {
                throw new Exception($"The server \"{this.Name}\" cannot be used because it is archived.");
            }
            var connectionType = GetConnectionType();
            if (connectionType == "Imap")
            {
                var connection = new IMAP4Connection(this.Server, this.Port, this.IsSsl);
                ImapLogin(connection);
            }
            else if (connectionType == "Pop")
            {
                var connection = new POP3Connection(this.Server, this.Port, this.IsSsl);
                connection.User(this.User);
                connection.Pass(this.Password);
            }
        }

        public void ButtonConfirmLogin()
        {
            try
            {
                var connection = Connect(allowArchived: true);
                this.State = "Done";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void FetchMail(bool raiseException = true)
        {
            // ... implementation here ...
        }

        public string GetConnectionType()
        {
            return this.ServerType;
        }
        
        private void ImapLogin(IMAP4Connection connection)
        {
            connection.Login(this.User, this.Password);
        }
    }
}
