using System;
using System.Collections.Generic;
using System.Text;

namespace ITL.NetCore.Connection.BL
{
    public enum DataModeType
    {
        AddNew = 0,
        Edit = 1,
        View = 2,
        Unknown = 3
    }

    public enum DataFieldAutoValueType
    {
        DateTimeNow = 0,
        Value = 1,
        FuncValue = 2,
    }
}
