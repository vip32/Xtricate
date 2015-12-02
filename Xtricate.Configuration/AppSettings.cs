using System.Collections.Generic;
using System.Configuration;

namespace Xtricate.Configuration
{
    public class AppSettings : AppSettingsBase
    {
        /// <summary>
        ///     The tier lets you specify retrieving a setting with the tier prefix first before falling back to the original
        ///     key.
        ///     E.g a tier of 'Live' looks for 'Live.{Key}' or if not found falls back to '{Key}'.
        /// </summary>
        public AppSettings(string tier = null) : base(new ConfigurationManagerWrapper())
        {
            Tier = tier;
        }

        /// <summary>
        ///     Returns string if exists, otherwise null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string GetString(string name) //Keeping backwards compatible
        {
            return GetNullableString(name);
        }

        private class ConfigurationManagerWrapper : ISettings
        {
            public string Get(string key)
            {
                return ConfigurationManager.AppSettings[key];
            }

            public List<string> GetAllKeys()
            {
                return new List<string>(ConfigurationManager.AppSettings.AllKeys);
            }
        }
    }
}