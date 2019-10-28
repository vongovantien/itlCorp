using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Common
{
    public struct Templates
    {
        public struct SysBranch
        {
            public struct NameCaching
            {
                public static string ListName = "SysBranch";
            }
        }
        public struct CatPlace
        {
            public static string ExelImportFileName = "Place";
            public struct NameCaching
            {
                public static string ListName = "Places";
            }
        }
        public struct CatStage
        {
            public static string ExelImportFileName = "Stage";
            public struct NameCaching
            {
                public static string ListName = "Stages";
            }
        }
        public struct CatUnit
        {
            public struct NameCaching
            {
                public static string ListName = "Units";
            }
        }
        public struct SysUser
        {
            public static string ExelImportFileName = "User-Import.xlsx";

        }
    }
}
