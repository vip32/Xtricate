using System.IO;

namespace XtricateSql
{
    public interface ISerializer
    {
        string ToJson(object value);
        T FromJson<T>(string value);
    }
}