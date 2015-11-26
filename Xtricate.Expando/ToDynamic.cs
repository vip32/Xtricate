using System.Dynamic;
using System.Linq;

namespace Xtricate.Dynamic
{
    public static class Extensions
    {
        public static dynamic ToExpando<T>(this T obj)
        {
            if (!typeof (T).GetInterfaces().Contains(typeof (IDynamicMetaObjectProvider))) return null;

            dynamic dobj = obj;
            return dobj;
        }
    }
}