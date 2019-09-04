
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Common
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
        public static readonly string EF_ANNOTATIONS_STRING_LENGTH = "EF_ANNOTATIONS_STRING_LENGTH";
        public static readonly string FILE_NOT_FOUND = "FILE_NOT_FOUND";
        public static readonly string NOT_FOUND_DATA_EXCEL = "NOT_FOUND_DATA_EXCEL";
        public static readonly string MSG_MAWB_EXISTED = "MSG_MAWB_EXISTED";
        public static readonly string MSG_HBNO_EXISTED = "MSG_HBNO_EXISTED";
        public static readonly string MSG_NOT_ALLOW_DELETED = "MSG_NOT_ALLOW_DELETED";
        public static readonly string MSG_CLEARANCENO_EXISTED = "MSG_CLEARANCENO_EXISTED";
        public static readonly string MSG_LIST_CLEARANCE_CONVERT_TO_JOB = "MSG_LIST_CLEARANCE_CONVERT_TO_JOB";
        public static readonly string MSG_CLEARANCE_CONVERT_TO_JOB = "MSG_CLEARANCE_CONVERT_TO_JOB";

        public static readonly string MSG_MAWBCONTAINER_CONTAINERTYPE_EMPTY = "MSG_MAWBCONTAINER_CONTAINERTYPE_EMPTY";
        public static readonly string MSG_MAWBCONTAINER_CONTAINERTYPE_NOT_FOUND = "MSG_MAWBCONTAINER_CONTAINERTYPE_NOT_FOUND";
        public static readonly string MSG_MAWBCONTAINER_QUANTITY_EMPTY = "MSG_MAWBCONTAINER_QUANTITY_EMPTY";
        public static readonly string MSG_MAWBCONTAINER_QUANTITY_MUST_BE_NUMBER = "MSG_MAWBCONTAINER_QUANTITY_MUST_BE_NUMBER";
        public static readonly string MSG_MAWBCONTAINER_PACKAGE_QUANTITY_MUST_BE_NUMBER = "MSG_MAWBCONTAINER_PACKAGE_QUANTITY_MUST_BE_NUMBER";
        public static readonly string MSG_MAWBCONTAINER_NW_MUST_BE_NUMBER = "MSG_MAWBCONTAINER_NW_MUST_BE_NUMBER";
        public static readonly string MSG_MAWBCONTAINER_GW_MUST_BE_NUMBER = "MSG_MAWBCONTAINER_GW_MUST_BE_NUMBER";
        public static readonly string MSG_MAWBCONTAINER_CBM_MUST_BE_NUMBER = "MSG_MAWBCONTAINER_CBM_MUST_BE_NUMBER";
        public static readonly string MSG_MAWBCONTAINER_PACKAGE_TYPE_NOT_FOUND = "MSG_MAWBCONTAINER_PACKAGE_TYPE_NOT_FOUND";
        public static readonly string MSG_MAWBCONTAINER_DUPLICATE = "MSG_MAWBCONTAINER_DUPLICATE";
        public static readonly string MSG_MAWBCONTAINER_COMMODITY_NAME_NOT_FOUND = "MSG_MAWBCONTAINER_COMMODITY_NAME_NOT_FOUND";
        public static readonly string MSG_MAWBCONTAINER_UNIT_OF_MEASURE_NOT_FOUND = "MSG_MAWBCONTAINER_UNIT_OF_MEASURE_NOT_FOUND";
        public static readonly string MSG_MAWBCONTAINER_QUANTITY_MUST_BE_1 = "MSG_MAWBCONTAINER_QUANTITY_MUST_BE_1";
    }
}
