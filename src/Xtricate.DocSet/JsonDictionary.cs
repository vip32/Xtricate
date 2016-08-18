using System.Collections.Generic;
using Xtricate.DocSet.Serialize;

namespace Xtricate.DocSet
{
    [JsonFormatting(CamelCase = false)]
    public class JsonDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    { }
}
