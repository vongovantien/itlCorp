using eFMS.API.Common.Globals;
using ITL.NetCore.Common;
using System;

namespace eFMS.API.Common.Infrastructure.Common
{
    public static class HandleError
    {

        public static string GetMessage(HandleState hs, Crud crud)
        {
            string message = string.Empty;
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
                case 403:
                    message = LanguageSub.DO_NOT_HAVE_PERMISSION;
                    break;
                case 400:
                    message = hs.Exception.Message;
                    break;
                default:
                    message = LanguageSub.MSG_DATA_NOT_FOUND;
                    break;
            }
            return message;
        }

        private static string SuccessMessage(Crud crud)
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
