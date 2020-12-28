
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Common
{
    public class DocumentationLanguageSub
    {
        public static readonly string MSG_MAWB_EXISTED = "MSG_MAWB_EXISTED";
        public static readonly string MSG_HBNO_EXISTED = "MSG_HBNO_EXISTED";
        public static readonly string MSG_MBLNO_HBNO_EXISTED = "MSG_MBLNO_HBNO_EXISTED";
        public static readonly string MSG_NOT_ALLOW_DELETED = "MSG_NOT_ALLOW_DELETED";
        public static readonly string MSG_CLEARANCENO_EXISTED = "MSG_CLEARANCENO_EXISTED";
        public static readonly string MSG_LIST_CLEARANCE_CONVERT_TO_JOB = "MSG_LIST_CLEARANCE_CONVERT_TO_JOB";
        public static readonly string MSG_CLEARANCE_CONVERT_TO_JOB = "MSG_CLEARANCE_CONVERT_TO_JOB";
        public static readonly string MSG_CLEARANCE_CARGOTYPE_MUST_HAVE_SERVICE_TYPE = "MSG_CLEARANCE_CARGOTYPE_MUST_HAVE_SERVICE_TYPE";
        public static readonly string MSG_CLEARANCE_CARGOTYPE_NOT_ALLOW_EMPTY = "MSG_CLEARANCE_CARGOTYPE_NOT_ALLOW_EMPTY";

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
        public static readonly string MSG_MAWBCONTAINER_EXISTED = "MSG_MAWBCONTAINER_EXISTED";
        public static readonly string MSG_MAWBCONTAINER_COMMODITY_NAME_NOT_FOUND = "MSG_MAWBCONTAINER_COMMODITY_NAME_NOT_FOUND";
        public static readonly string MSG_MAWBCONTAINER_UNIT_OF_MEASURE_NOT_FOUND = "MSG_MAWBCONTAINER_UNIT_OF_MEASURE_NOT_FOUND";
        public static readonly string MSG_MAWBCONTAINER_QUANTITY_MUST_BE_1 = "MSG_MAWBCONTAINER_QUANTITY_MUST_BE_1";
        public static readonly string MSG_MAWBCONTAINER_PACKAGE_TYPE_EMPTY = "MSG_MAWBCONTAINER_PACKAGE_TYPE_EMPTY";
        public static readonly string MSG_MAWBCONTAINER_PACKAGE_QUANTITY_EMPTY = "MSG_MAWBCONTAINER_PACKAGE_QUANTITY_EMPTY";

        public static readonly string MSG_SURCHARGE_NOT_FOUND = "MSG_SURCHARGE_NOT_FOUND";
        public static readonly string MSG_SURCHARGE_NOT_ALLOW_DELETED = "MSG_SURCHARGE_NOT_ALLOW_DELETED";

        //CD note
        public static readonly string MSG_CDNOTE_NOT_ALLOW_DELETED_NOT_FOUND = "MSG_CDNOTE_NOT_ALLOW_DELETED_NOT_FOUND";
        public static readonly string MSG_CDNOTE_NOT_ALLOW_DELETED_HAD_SOA = "MSG_CDNOTE_NOT_ALLOW_DELETED_HAD_SOA";
        public static readonly string MSG_CDNOTE_NOT_NOT_FOUND = "MSG_CDNOTE_NOT_NOT_FOUND";
        public static readonly string MSG_CDNOTE_NOT_ALLOW_DELETED_HAD_SYNCED = "MSG_CDNOTE_NOT_ALLOW_DELETED_HAD_SYNCED";

        public static readonly string MSG_SURCHARGE_ARE_DUPLICATE_INVOICE = "MSG_SURCHARGE_ARE_DUPLICATE_INVOICE";

        public static readonly string MSG_NOT_FOUND_TRANSACTION_TYPE = "MSG_NOT_FOUND_TRANSACTION_TYPE";
        public static readonly string MSG_MBL_REQUIRED = "MSG_MBL_REQUIRED";
        public static readonly string MSG_POD_DIFFERENT_POL = "MSG_POD_DIFFERENT_POL";
        public static readonly string MSG_POL_DIFFERENT_POD = "MSG_POL_DIFFERENT_POD";
        public static readonly string MSG_ETD_BEFORE_ETA = "MSG_ETD_BEFORE_ETA";
        public static readonly string MSG_ETA_AFTER_ETD = "MSG_ETA_AFTER_ETD";
        public static readonly string MSG_ETA_REQUIRED = "MSG_ETA_REQUIRED";
        public static readonly string MSG_MBL_TYPE_REQUIRED = "MSG_MBL_TYPE_REQUIRED";
        public static readonly string MSG_SHIPMENT_TYPE_REQUIRED = "MSG_SHIPMENT_TYPE_REQUIRED";
        public static readonly string MSG_POD_REQUIRED = "MSG_POD_REQUIRED";
        public static readonly string MSG_PODELI_DIFFERENT_POL = "MSG_PODELI_DIFFERENT_POL";
        public static readonly string MSG_SERVICE_TYPE_REQUIRED = "MSG_SERVICE_TYPE_REQUIRED";
        public static readonly string MSG_PERSON_IN_CHARGE_REQUIRED = "MSG_PERSON_IN_CHARGE_REQUIRED";

        public static readonly string MSG_NOT_EXIST_SHIPMENT = "MSG_NOT_EXIST_SHIPMENT";
        public static readonly string DO_NOT_HAVE_PERMISSION = "DO_NOT_HAVE_PERMISSION";
        public static readonly string MSG_HOUSEBILL_DO_NOT_DELETE_CONTAIN_CDNOTE_SOA = "MSG_HOUSEBILL_DO_NOT_DELETE_CONTAIN_CDNOTE_SOA";
        public static readonly string MSG_HOUSEBILL_NOT_FOUND = "MSG_HOUSEBILL_NOT_FOUND";
        public static readonly string MSG_NOT_EXIST_SHIPMENT_COPY = "MSG_NOT_EXIST_SHIPMENT_COPY";
    }
}
