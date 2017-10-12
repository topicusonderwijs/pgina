namespace pGina.Plugin.TopicusKeyHub.LDAP
{
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices.Protocols;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using log4net;
    using Model;
    using Settings.Model;

    public class LdapServer : IDisposable
    {
        private readonly ILog logger = LogManager.GetLogger("LdapServer");

        private LdapConnection ldapconnection;
        private LdapDirectoryIdentifier serverIdentifier;
        private X509Certificate2 cert;        
        private ConnectionSettings connectionsettings;

        public LdapServer(ConnectionSettings connectionsettings)
        {
            this.ldapconnection = null;
            this.cert = null;
            this.Connect(connectionsettings);
        }

        public void Dispose()
        {
            this.Close();
        }

        internal void Close()
        {
            if (this.ldapconnection != null)
            {
                this.logger.DebugFormat("Closing LDAP connection to {0}.", this.ldapconnection.SessionOptions.HostName);
                this.ldapconnection.SessionOptions.StopTransportLayerSecurity();
                this.ldapconnection.Dispose();
                this.ldapconnection = null;
            }
        }

        internal IEnumerable<KeyHubGroup> GetGroups(string distinguishedName)
        {
            var searchRequest = new SearchRequest
            (distinguishedName,
                "(objectclass=groupofnames)",
                SearchScope.Subtree,
                "*");

            var list = new List<KeyHubGroup>();

            var searchResponse = (SearchResponse)this.ldapconnection.SendRequest(searchRequest);
            foreach (SearchResultEntry searchResponseEntry in searchResponse.Entries)
            {
                list.Add(new KeyHubGroup(searchResponseEntry.DistinguishedName, searchResponseEntry.Attributes["cn"][0].ToString()));
            }

            return list;
        }

        internal KeyHubUser GetUser(string rootdistributionName, string commonName)
        {
            var searchRequest = new SearchRequest(rootdistributionName,
                string.Format("(&(objectclass=keyhubUser)(cn={0}))",commonName),
                SearchScope.Subtree,
                "*");
            var searchResponse = (SearchResponse)this.ldapconnection.SendRequest(searchRequest);
            if (searchResponse.Entries.Count != 1)
            {
                return null;
            }
            return this.CreateKeyHubUser(searchResponse.Entries[0]);
        }

        private KeyHubUser CreateKeyHubUser(SearchResultEntry searchResultEntry)
        {
            var commonname = searchResultEntry.Attributes["cn"][0].ToString();
            var mail = searchResultEntry.Attributes["mail"][0].ToString();
            var displayName = searchResultEntry.Attributes["displayName"][0].ToString();
            var memberoflist = GetListOfAttribute(searchResultEntry.Attributes, "memberOf");
            return new KeyHubUser(searchResultEntry.DistinguishedName, commonname, displayName, mail, memberoflist);
        }

        internal bool UserIsInGroup(KeyHubUser user, KeyHubGroup groep)
        {
            if (user.MemberOflist == null || groep == null)
            {
                return false;
            }
            return user.MemberOflist.Any(b => b.ToLower().Equals(groep.DistinguishedName.ToLower()));
        }
        
        private static IEnumerable<string> GetListOfAttribute(SearchResultAttributeCollection searchResultAttributeCollection, string attributename)
        {
            if (searchResultAttributeCollection[attributename] != null)
            {
                var i = searchResultAttributeCollection[attributename].Count - 1;
                while (i >= 0)
                {
                    var value = searchResultAttributeCollection[attributename][i];
                    var namingcontext = value.ToString();
                    yield return namingcontext;
                    i--;
                }
            }
        }
        
        internal bool PasswordCheck(KeyHubUser user, string password)
        {
            var valuepassword = System.Text.Encoding.ASCII.GetBytes(password);
            var compareRequest = new CompareRequest(user.DistinguishedName, "userPassword", valuepassword);
            var compareResponse = (CompareResponse)this.ldapconnection.SendRequest(compareRequest);
            return compareResponse.ResultCode == ResultCode.CompareTrue;
        }

        internal IEnumerable<KeyHubNamingContexts> GetTopNamingContexts()
        {
            var searchRequest = new SearchRequest
            (null,
                "(objectClass=top)",
                SearchScope.Base,
                "*");

            var searchResponse = (SearchResponse)this.ldapconnection.SendRequest(searchRequest);
            var namingcontexts = GetListOfAttribute(searchResponse.Entries[0].Attributes, "namingcontexts");

            var list = new List<KeyHubNamingContexts>();

            foreach (var namingcontext in namingcontexts)
            {
                list.Add(new KeyHubNamingContexts(namingcontext.Contains("dynamic"), namingcontext));
            }

            if (list.Count != 2)
            {
                throw new Exception("Topicus KeyHub can't have more than 2 namingcontexts");
            }

            if (list.Count(b => b.Dynamic) != 1)
            {
                throw new Exception("Topicus KeyHub must have 1 dynamic namingcontext");
            }

            return list;
        }

        /// <summary>
        /// Try to bind to the LDAP server with credentials.  This uses
        /// basic authentication.  Throws LdapException if the bind fails.
        /// </summary>
        internal void BindForSearch()
        {
            NetworkCredential creds = new NetworkCredential(this.connectionsettings.SearchDN, this.connectionsettings.SearchPW);

            if (this.ldapconnection == null)
                throw new LdapException("Bind attempted when server is not connected.");

            this.logger.DebugFormat("Attempting bind as {0}", creds.UserName);

            this.ldapconnection.AuthType = AuthType.Basic;

            try
            {
                this.ldapconnection.Bind(creds);
                this.logger.DebugFormat("Successful bind to {0} as {1}", this.ldapconnection.SessionOptions.HostName, creds.UserName);
            }
            catch (LdapException e)
            {
                this.logger.ErrorFormat("LdapException: {0} {1} {2}", e.ErrorCode, e.Message, e.ServerErrorMessage);
                throw;
            }
            catch (InvalidOperationException e)
            {
                // This shouldn't happen, but log it and re-throw
                this.logger.ErrorFormat("InvalidOperationException: {0}", e.Message);
                throw;
            }
            catch (Exception e)
            {
                // This shouldn't happen, but log it and re-throw
                this.logger.ErrorFormat("Bind Exception: {0}", e.Message);
                throw;
            }
        }

        /// <summary>
        /// This is the verify certificate callback method used when initially binding to the
        /// LDAP server.  This manages all certificate validation.
        /// </summary>
        /// <param name="ldapconn">The LDAP connection.</param>
        /// <param name="servercert">The server's certificate</param>
        /// <returns>true if verification succeeds, false otherwise.</returns>
        private bool VerifyCert(LdapConnection ldapconn, X509Certificate servercert)
        {
            this.logger.Debug("VerifyCert(...)");
            this.logger.DebugFormat("Verifying certificate from host: {0}", ldapconn.SessionOptions.HostName);

            var verifyCertificate = VerifyCertificate.GetInstance();

            // Convert to X509Certificate2
            X509Certificate2 serverCert = new X509Certificate2(servercert);

            // If we don't need to verify the cert, the verification succeeds
            if (!this.connectionsettings.RequireCert)
            {
                this.logger.Debug("Server certificate accepted without verification.");
                return true;
            }

            // If the certificate is null, then we verify against the machine's/user's certificate store
            if (this.cert == null)
            {
                this.logger.Debug("Verifying server cert with Windows store.");
                return verifyCertificate.VerifyMachineUser(serverCert);
            }
            this.logger.Debug("Validating server certificate with provided certificate.");

            // Verify against the provided cert by comparing the thumbprint
            var result = this.cert.Thumbprint == serverCert.Thumbprint;
            if (result)
            {
                this.logger.Debug("Server certificate validated.");
                return true;
            }

            // Verify CA
            var caresult = verifyCertificate.VerifyCARoot(this.cert, serverCert);
            if (caresult)
            {
                this.logger.Debug("Server certificate validated.");
                return true;
            }
            
            this.logger.Debug("Server certificate validation failed.");

            return false;
        }

        private void Connect(ConnectionSettings settings)
        {
            this.connectionsettings = settings;

            // Are we re-connecting?  If so, close the previous connection.
            if (this.ldapconnection != null)
            {
                this.Close();
            }

            if (this.connectionsettings.RequireCert)
            {
                if (!String.IsNullOrEmpty(this.connectionsettings.ServerCertFile) && File.Exists(this.connectionsettings.ServerCertFile))
                {
                    this.logger.DebugFormat("Loading server certificate: {0}", this.connectionsettings.ServerCertFile);
                    this.cert = new X509Certificate2(this.connectionsettings.ServerCertFile);
                }
                else
                {
                    this.logger.DebugFormat("Certificate file not provided or not found, will validate against Windows store.");
                }
            }

            this.serverIdentifier = new LdapDirectoryIdentifier(this.connectionsettings.LdapHosts, this.connectionsettings.LdapPort, false, false);
            this.ldapconnection = new LdapConnection(this.serverIdentifier)
            {
                Timeout = new TimeSpan(0, 0, this.connectionsettings.LdapTimeout)
            };
            this.logger.DebugFormat("Timeout set to {0} seconds.", this.connectionsettings.LdapTimeout);
            this.ldapconnection.SessionOptions.ProtocolVersion = 3;


            this.logger.DebugFormat("Initializing LdapServer host(s): [{0}], port: {1}, verifyCert = {2}",
                String.Join(", ", this.connectionsettings.LdapHosts), this.connectionsettings.LdapPort, this.connectionsettings.RequireCert);

            if (this.connectionsettings.RequireCert)
            {
                this.ldapconnection.SessionOptions.VerifyServerCertificate = this.VerifyCert;
            }
            else
            {
                this.ldapconnection.SessionOptions.VerifyServerCertificate = (ldapconn, servercert) => true;
            }
            try
            {
                this.ldapconnection.SessionOptions.StartTransportLayerSecurity(new DirectoryControlCollection());
            }
            catch (Exception e)
            {
                this.logger.ErrorFormat("Start TLS failed with {0}", e.Message);
                this.ldapconnection = null;
                throw;
            }
        }
    }
}
