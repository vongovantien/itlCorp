using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ObjectHelper
/// </summary>
public static class ObjectHelper
{
    public static dynamic GetValueBy(this Object obj, string pro)
    {
        return obj.GetType().GetProperty(pro).GetValue(obj, null);
    }
}