using System;
using System.Collections.Generic;
using System.Text;

namespace ITL.NetCore.Connection
{
    public static class StringEx
    {
        public static bool IsSafeSQL(this String strSQL)
        {
            string[] sqlCheckList = { "--", ";--",/* ";",*/ "/*", "*/", "@@", "@", "char", "nchar", "varchar", "nvarchar", "alter", "begin", "cast", "create", "cursor", "declare", "delete", "drop", "end", "exec", "execute", "fetch", "insert", "kill", "select", "sys", "sysobjects", "syscolumns", "table", "update" };
            foreach(var strCheck in sqlCheckList)
            {
                if (strSQL.IndexOf(strCheck) > -1)
                    return false;
            }            
            return true;
        }
    }
}
