using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Xtricate.Configuration
{
    public class ConnectionStrings : AppSettingsBase
    {
        public ConnectionStrings(string tier = null) : base(new ConnectionStringsWrapper())
        {
            Tier = tier;
        }

        public override string GetString(string name)
        {
            return GetNullableString(name);
        }

        private class ConnectionStringsWrapper : ISettings
        {
            public string Get(string key)
            {
                if(!GetAllKeys().Contains(key)) throw new ConfigurationErrorsException($"Unable to find Connection String: {key}");
                return ConfigurationManager.ConnectionStrings[key].ConnectionString;
            }

            public List<string> GetAllKeys()
            {
                return (ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>()
                    .Select(settings => settings.Name)).ToList();
            }
        }
    }
}