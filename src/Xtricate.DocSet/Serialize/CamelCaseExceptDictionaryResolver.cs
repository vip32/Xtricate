using Newtonsoft.Json.Serialization;

namespace Xtricate.DocSet
{
    public class CamelCaseExceptDictionaryResolver : CamelCasePropertyNamesContractResolver
    {
        protected override string ResolveDictionaryKey(string dictionaryKey)
        {
            return dictionaryKey;
        }
    }
}