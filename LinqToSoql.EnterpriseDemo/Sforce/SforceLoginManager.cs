using System;
using System.Security.Authentication;
using System.ServiceModel;
using LinqToSoql.EnterpriseDemo.SforceEnterpriseService;

namespace LinqToSoql.EnterpriseDemo.Sforce
{
    public class SforceLoginManager : IDisposable
    {
        public SessionHeader SessionHeader { get; private set; }
        public SoapClient Client { get; private set; } // for API endpoint

        public SforceLoginManager()
        {
            var loginClient = new SoapClient();

            LoginResult lr = loginClient.login(null, Config.Login, Config.Password + Config.Token);

            if (lr.passwordExpired)
            {
                throw new AuthenticationException("Password expired");
            }

            var endpoint = new EndpointAddress(lr.serverUrl);

            SessionHeader = new SessionHeader {sessionId = lr.sessionId};

            Client = new SoapClient("Soap", endpoint);
        }

        public void Dispose()
        {
            Client.logout(SessionHeader);
        }
    }
}