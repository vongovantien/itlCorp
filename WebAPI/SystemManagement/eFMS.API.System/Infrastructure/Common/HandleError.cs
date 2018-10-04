using ITL.NetCore.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemManagementAPI.Resources;

namespace eFMS.API.System.Infrastructure.Common
{
    public enum Crud
    {
        Get,
        Insert,
        Update,
        Delete
    }
    public static class HandleError
    {
        public static string GetMessage(HandleState hs, Crud crud)
        {
            string message = LanguageSub.MSG_DATA_NOT_FOUND;
            switch (hs.Code)
            {
                case 200:
                    message = SuccessMessage(crud);
                    break;
                case 201:
                    message = LanguageSub.MSG_OBJECT_NOT_EXISTS;
                    break;
                case 202:
                    message = LanguageSub.MSG_DELETE_FAIL_INCLUDED_CHILD;
                    break;
                case 203:
                    message = LanguageSub.MSG_OBJECT_DUPLICATED;
                    break;
                case 204:
                    message = LanguageSub.MSG_OBJECT_RELATION_NOT_VALID;
                    break;

            }
            return message;
        }
        public static String SuccessMessage(Crud crud)
        {
            string message = LanguageSub.MSG_DATA_NOT_FOUND;
            switch (crud)
            {
                case Crud.Insert:
                    message = LanguageSub.MSG_INSERT_SUCCESS;
                    break;
                case Crud.Update:
                    message = LanguageSub.MSG_UPDATE_SUCCESS;
                    break;
                case Crud.Delete:
                    message = LanguageSub.MSG_DELETE_SUCCESS;
                    break;

            }
            return message;
        }
    }
}
