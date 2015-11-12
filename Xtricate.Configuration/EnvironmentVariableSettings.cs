using System;
using System.Collections.Generic;

namespace Xtricate.Configuration
{
    public class EnvironmentVariableSettings : AppSettingsBase
    {
        public EnvironmentVariableSettings() : base(new EnvironmentSettingsWrapper())
        {
        }

        public override string GetString(string name)
        {
            return GetNullableString(name);
        }

        private class EnvironmentSettingsWrapper : ISettings
        {
            public string Get(string key)
            {
                return Environment.GetEnvironmentVariable(key);
            }

            public List<string> GetAllKeys()
            {
                return Environment.GetEnvironmentVariables().Keys.Map(x => x.ToString());
            }
        }
    }
}