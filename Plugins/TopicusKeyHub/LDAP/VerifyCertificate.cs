namespace pGina.Plugin.TopicusKeyHub.LDAP
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
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

        public bool VerifyMachineUser(X509Certificate2 serverCert)
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

        public bool VerifyCARoot(X509Certificate2 chainCert, X509Certificate2 serverCert)
        {
            var chain = new X509Chain();
            chain.ChainPolicy.ExtraStore.Add(chainCert);
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            // untrusted certificate(s) to the chain you need to validate
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            return chain.Build(serverCert);
        }
    }
}
