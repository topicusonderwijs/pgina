namespace pGina.Plugin.TopicusKeyHub.LDAP
{
    using System;

    public static class UsernameHelper
    {
        public static string RemoveDomainFromUsername(this string username)
        {
            var result = username;
            var index = result.IndexOf("@", StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                result = result.Substring(0, index);
            }
            index = result.IndexOf("\\", StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                result = result.Substring(index+1);
            }
            return result;
        }
    }
}
