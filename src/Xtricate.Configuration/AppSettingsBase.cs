using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using ServiceStack.Text;

namespace Xtricate.Configuration
{
    public delegate string ParsingStrategyDelegate(string originalSetting);

    public class AppSettingsBase : IAppSettings, ISettingsWriter
    {
        protected const string ErrorAppsettingNotFound = "Unable to find App Setting: {0}";
        protected ISettings Settings;
        protected ISettingsWriter SettingsWriter;

        public AppSettingsBase(ISettings settings = null)
        {
            Init(settings);
        }

        public string Tier { get; set; }

        public ParsingStrategyDelegate ParsingStrategy { get; set; }

        public virtual Dictionary<string, string> GetAll()
        {
            var keys = GetAllKeys();
            var to = new Dictionary<string, string>();
            foreach (var key in keys)
            {
                to[key] = GetNullableString(key);
            }
            return to;
        }

        public virtual List<string> GetAllKeys()
        {
            var keys = Settings.GetAllKeys().ToHashSet();
            if (SettingsWriter != null)
                SettingsWriter.GetAllKeys().Each(x => keys.Add(x));

            return keys.ToList();
        }

        public virtual bool Exists(string key)
        {
            return GetNullableString(key) != null;
        }

        public virtual string GetString(string name)
        {
            return GetNullableString(name);
        }

        public virtual IList<string> GetList(string key)
        {
            var value = GetString(key);
            return value == null
                ? new List<string>()
                : ConfigUtils.GetListFromAppSettingValue(value);
        }

        public virtual IDictionary<string, string> GetDictionary(string key)
        {
            var value = GetString(key);
            try
            {
                return ConfigUtils.GetDictionaryFromAppSettingValue(value);
            }
            catch (Exception ex)
            {
                var message =
                    string.Format(
                        "The {0} setting had an invalid dictionary format. The correct format is of type \"Key1:Value1,Key2:Value2\"",
                        key);
                throw new ConfigurationErrorsException(message, ex);
            }
        }

        public virtual T Get<T>(string name)
        {
            var stringValue = GetNullableString(name);
            return stringValue != null
                ? TypeSerializer.DeserializeFromString<T>(stringValue) //JsonConvert.DeserializeObject<T>(stringValue)
                : default(T);
        }

        public virtual T Get<T>(string name, T defaultValue)
        {
            var stringValue = GetNullableString(name);

            var ret = defaultValue;
            try
            {
                if (stringValue != null)
                {
                    //ret = JsonConvert.DeserializeObject<T>(stringValue);
                    ret = TypeSerializer.DeserializeFromString<T>(stringValue);
                }
            }
            catch (Exception ex)
            {
                var message =
                    string.Format(
                        "The {0} setting had an invalid format. The value \"{1}\" could not be cast to type {2}",
                        name, stringValue, typeof (T).FullName);
                throw new ConfigurationErrorsException(message, ex);
            }

            if (ret == null) ret = defaultValue;
            return ret;
        }

        public virtual void Set<T>(string key, T value)
        {
            if (SettingsWriter == null)
                SettingsWriter = new DictionarySettings();

            SettingsWriter.Set(key, value);
        }

        public string Get(string name)
        {
            if (SettingsWriter != null)
            {
                var value = SettingsWriter.Get(name);
                if (value != null)
                    return value;
            }
            return Settings.Get(name);
        }

        protected void Init(ISettings settings)
        {
            Settings = settings;
            SettingsWriter = settings as ISettingsWriter;
        }

        public virtual string GetNullableString(string name)
        {
            var value = Tier != null
                ? Get("{0}.{1}".Fmt(Tier, name)) ?? Get(name)
                : Get(name);

            return ParsingStrategy != null
                ? ParsingStrategy(value)
                : value;
        }

        public virtual string GetRequiredString(string name)
        {
            var value = GetNullableString(name);
            if (value == null)
            {
                throw new ConfigurationErrorsException(string.Format(ErrorAppsettingNotFound, name));
            }

            return value;
        }
    }
}