using System;

namespace eFMS.API.Common.Helpers
{
    public static class ObjectHelper
    {
        public static dynamic GetValueBy(this Object obj, string pro)
        {
            return obj.GetType().GetProperty(pro).GetValue(obj, null);
        }
    }
}
