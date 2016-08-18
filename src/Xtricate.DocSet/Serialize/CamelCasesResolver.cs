using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;

namespace Xtricate.DocSet.Serialize
{
    public class CamelCasesResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            var formatting = objectType.GetCustomAttributes(typeof(JsonFormatting)).Select(x => (JsonFormatting)x);

            JsonDictionaryContract contract = base.CreateDictionaryContract(objectType);
            if (!formatting.IsNullOrEmpty() && formatting.Any(x => !x.CamelCase))
            {
                contract.DictionaryKeyResolver = key => key;
            }

            return contract;
        }
    }
}
