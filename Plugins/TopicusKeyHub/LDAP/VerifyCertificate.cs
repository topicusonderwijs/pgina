namespace pGina.Plugin.TopicusKeyHub.LDAP
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;

    public class VerifyCertificate
    {
        private readonly log4net.ILog logger = log4net.LogManager.GetLogger("VerifyCertificate");

        private static VerifyCertificate _instance;

        public static VerifyCertificate GetInstance()
        {
            if (_instance == null)
            {
                _instance = new VerifyCertificate();
            }
            return _instance;
        }

        internal bool VerifyMachineUser(X509Certificate2 serverCert)
        {
            // We set the RevocationMode to NoCheck because most custom (self-generated) CAs
            // do not work properly with revocation lists.  This is slightly less secure, but
            // the most common use case for this plugin probably doesn't rely on revocation
            // lists.
            var policy = new X509ChainPolicy
            {
                RevocationMode = X509RevocationMode.NoCheck
            };

            // Create a validator using the policy
            var validator =
                X509CertificateValidator.CreatePeerOrChainTrustValidator(true, policy);
            try
            {
                validator.Validate(serverCert);

                // If we get here, validation succeeded.
                this.logger.Debug("Server certificate verification succeeded.");
                return true;
            }
            catch (SecurityTokenValidationException e)
            {
                this.logger.ErrorFormat("Server certificate validation failed: {0}", e.Message);
                return false;
            }
            catch (Exception e)
            {
                this.logger.ErrorFormat("Server certificate validation failed: {0}", e.Message);
                return false;
            }
        }

        internal bool VerifyCARoot(X509Certificate2 givenCert, X509Certificate2 serverCert)
        {
            var certificateKey = givenCert.GetPublicKeyString();
            var serverKey = serverCert.GetPublicKeyString();

            if (string.Equals(serverKey, certificateKey, StringComparison.InvariantCultureIgnoreCase))
            {
                // match
                return true;
            }

            // try root CA check
            var chain = new X509Chain
            {
                ChainPolicy =
                {
                    RevocationMode = X509RevocationMode.NoCheck,
                    RevocationFlag = X509RevocationFlag.ExcludeRoot,
                    VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority,
                    VerificationTime = DateTime.Now,
                    UrlRetrievalTimeout = new TimeSpan(0, 0, 0)
                }
            };

            // This part is very important. You're adding your known root here.
            chain.ChainPolicy.ExtraStore.Add(givenCert);
            bool isChainValid = chain.Build(serverCert);
            if (isChainValid)
            {
                // This piece makes sure it actually matches your known root
                if (chain.ChainElements.Cast<X509ChainElement>().All(x => x.Certificate.Thumbprint != givenCert.Thumbprint))
                {
                    // Trust chain did not complete to the known authority anchor. Thumbprints did not match.
                    return false;
                }
                return true;
            }

            // loop chain
            chain = new X509Chain
            {
                ChainPolicy =
                {
                    RevocationMode = X509RevocationMode.NoCheck,
                    RevocationFlag = X509RevocationFlag.ExcludeRoot,
                    VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority,
                    VerificationTime = DateTime.Now,
                    UrlRetrievalTimeout = new TimeSpan(0, 0, 0)
                }
            };

            chain.Build(serverCert);
            
            foreach (var chainvalue in chain.ChainElements)
            {
                var key = chainvalue.Certificate.GetPublicKeyString();
                if (string.Equals(certificateKey, key, StringComparison.InvariantCultureIgnoreCase))
                {
                    // match in chain.
                    return true;
                }

            }
            return false;
        }
    }
}
