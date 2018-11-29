
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SystemManagementAPI.Resources
{
    public class LanguageSub
    {
        //Global
        public static readonly string MSG_DATA_NOT_FOUND = "MSG_DATA_NOT_FOUND";
        public static readonly string MSG_OBJECT_NOT_EXISTS = "MSG_OBJECT_NOT_EXISTS";
        public static readonly string MSG_OBJECT_DUPLICATED = "MSG_OBJECT_DUPLICATED";

        public static readonly string MSG_OBJECT_RELATION_NOT_VALID = "MSG_OBJECT_RELATION_NOT_VALID";

        public static readonly string MSG_INSERT_SUCCESS = "MSG_INSERT_SUCCESS";

        public static readonly string MSG_UPDATE_SUCCESS = "MSG_UPDATE_SUCCESS";

        public static readonly string MSG_DELETE_SUCCESS = "MSG_DELETE_SUCCESS";

        public static readonly string MSG_NAME_EXISTED = "MSG_NAME_EXISTED";

        public static readonly string MSG_CODE_EXISTED = "MSG_CODE_EXISTED";
        public static readonly string MSG_DELETE_FAIL_INCLUDED_CHILD = "MSG_DELETE_FAIL_INCLUDED_CHILD";
        public static readonly string EF_ANNOTATIONS_REQUIRED = "EF_ANNOTATIONS_REQUIRED";
    }
}
