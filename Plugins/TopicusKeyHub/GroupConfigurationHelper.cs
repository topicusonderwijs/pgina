using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pGina.Plugin.TopicusKeyHub
{
    using LDAP;
    using LDAP.Model;
    using Settings.Model;

    public class GroupConfigurationHelper
    {
        internal List<KeyHubGroup> GetKeyHubGroups(ConnectionSettings settings, bool dynamic)
        {
            using (var ldap = new LdapServer(settings))
            {
                ldap.BindForSearch();
                var contexts = ldap.GetTopNamingContexts();
                var groups = ldap.GetGroups(contexts.Single(b => b.Dynamic == dynamic).DistributionName)
                    .OrderBy(c => c.CommonName);
                return groups.ToList();
            }
        }

        internal static GatewayRule GetGatewayRule(string rule, IEnumerable<KeyHubGroup> keyHubGroups)
        {
            var splitrule = rule.Split('*');
            KeyHubGroup keyHubGroup = null;
            if (keyHubGroups != null && keyHubGroups.Any())
            {
                keyHubGroup = keyHubGroups.SingleOrDefault(b => b.DistinguishedName == splitrule[0]);
            }

            return new GatewayRule
            {
                KeyHubGroupCommonName = keyHubGroup == null ? null : keyHubGroup.CommonName,
                LocalMachineGroupName = splitrule[1],
                KeyHubGroupDistinguishedName = splitrule[0]
            };
        }

        internal class GatewayRule
        {
            public string KeyHubGroupDistinguishedName { get; set; }

            public string KeyHubGroupCommonName { get; set; }

            public string LocalMachineGroupName { get; set; }
        }
    }
}
