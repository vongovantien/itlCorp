using eFMS.API.Common.Globals;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Common
{
    public static class UnlockTypeEx
    {
        public static string GetUnlockType(UnlockTypeEnum type)
        {
            string result = "";
            switch (type)
            {
                case UnlockTypeEnum.SHIPMENT:
                    result = "Shipment";
                    break;
                case UnlockTypeEnum.ADVANCE:
                    result = "Advance";
                    break;
                case UnlockTypeEnum.SETTLEMENT:
                    result = "Settlement";
                    break;
                case UnlockTypeEnum.CHANGESERVICEDATE:
                    result = "Change Service Date";
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}
