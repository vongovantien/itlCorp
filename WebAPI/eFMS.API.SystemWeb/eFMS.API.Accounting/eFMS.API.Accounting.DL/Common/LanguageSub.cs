using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Common
{
    public partial class AccountingLanguageSub
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
        public static readonly string MSG_CLEARANCENO_EXISTED = "MSG_CLEARANCENO_EXISTED";
        public static readonly string MSG_MAWB_EXISTED = "MSG_MAWB_EXISTED";
        public static readonly string MSG_HBNO_EXISTED = "MSG_HBNO_EXISTED";




        #region Unit
        public static readonly string MSG_NAME_EN_EXISTED = "MSG_NAME_EN_EXISTED";
        public static readonly string MSG_NAME_LOCAL_EXISTED = "MSG_NAME_LOCAL_EXISTED";
        #endregion

        #region Partner
        public static readonly string MSG_PARTNER_TAXCODE_EMPTY = "MSG_PARTNER_TAXCODE_EMPTY";
        public static readonly string MSG_PARTNER_TAXCODE_NOT_NUMBER = "MSG_PARTNER_TAXCODE_NOT_NUMBER";
        public static readonly string MSG_PARTNER_TAXCODE_EXISTED = "MSG_PARTNER_TAXCODE_EXISTED";
        public static readonly string MSG_PARTNER_TAXCODE_DUPLICATED = "MSG_PARTNER_TAXCODE_DUPLICATED";
        public static readonly string MSG_PARTNER_GROUP_EMPTY = "MSG_PARTNER_GROUP_EMPTY";
        public static readonly string MSG_PARTNER_GROUP_NOT_FOUND = "MSG_PARTNER_GROUP_NOT_FOUND";
        public static readonly string MSG_PARTNER_SALEMAN_EMPTY = "MSG_PARTNER_SALEMAN_EMPTY";
        public static readonly string MSG_PARTNER_SALEMAN_NOT_FOUND = "MSG_PARTNER_SALEMAN_NOT_FOUND";
        public static readonly string MSG_PARTNER_NAME_EN_EMPTY = "MSG_PARTNER_NAME_EN_EMPTY";
        public static readonly string MSG_PARTNER_NAME_VN_EMPTY = "MSG_PARTNER_NAME_VN_EMPTY";
        public static readonly string MSG_PARTNER_SHORT_NAME_EMPTY = "MSG_PARTNER_SHORT_NAME_EMPTY";
        public static readonly string MSG_PARTNER_COUNTRY_BILLING_EMPTY = "MSG_PARTNER_COUNTRY_BILLING_EMPTY";
        public static readonly string MSG_PARTNER_COUNTRY_BILLING_NOT_FOUND = "MSG_PARTNER_COUNTRY_BILLING_NOT_FOUND";
        public static readonly string MSG_PARTNER_PROVINCE_BILLING_EMPTY = "MSG_PARTNER_PROVINCE_BILLING_EMPTY";
        public static readonly string MSG_PARTNER_PROVINCE_BILLING_NOT_FOUND = "MSG_PARTNER_PROVINCE_BILLING_NOT_FOUND";
        public static readonly string MSG_PARTNER_COUNTRY_SHIPPING_EMPTY = "MSG_PARTNER_COUNTRY_SHIPPING_EMPTY";
        public static readonly string MSG_PARTNER_COUNTRY_SHIPPING_NOT_FOUND = "MSG_PARTNER_COUNTRY_SHIPPING_NOT_FOUND";
        public static readonly string MSG_PARTNER_PROVINCE_SHIPPING_EMPTY = "MSG_PARTNER_PROVINCE_SHIPPING_EMPTY";
        public static readonly string MSG_PARTNER_PROVINCE_SHIPPING_NOT_FOUND = "MSG_PARTNER_PROVINCE_SHIPPING_NOT_FOUND";
        public static readonly string MSG_PARTNER_ADDRESS_BILLING_EN_NOT_FOUND = "MSG_PARTNER_ADDRESS_BILLING_EN_NOT_FOUND";
        public static readonly string MSG_PARTNER_ADDRESS_BILLING_VN_NOT_FOUND = "MSG_PARTNER_ADDRESS_BILLING_VN_NOT_FOUND";
        public static readonly string MSG_PARTNER_ADDRESS_SHIPPING_EN_NOT_FOUND = "MSG_PARTNER_ADDRESS_SHIPPING_EN_NOT_FOUND";
        public static readonly string MSG_PARTNER_ADDRESS_SHIPPING_VN_NOT_FOUND = "MSG_PARTNER_ADDRESS_SHIPPING_VN_NOT_FOUND";
        public static readonly string MSG_PARTNER_WORKPLACE_NOT_FOUND = "MSG_PARTNER_WORKPLACE_NOT_FOUND";
        #endregion

        #region
        public static readonly string MSG_PLACE_NAME_EN_EMPTY = "MSG_PLACE_NAME_EN_EMPTY";
        public static readonly string MSG_PLACE_NAME_LOCAL_EMPTY = "MSG_PLACE_NAME_LOCAL_EMPTY";
        public static readonly string MSG_PLACE_COUNTRY_NAME_EMPTY = "MSG_PLACE_COUNTRY_NAME_EMPTY";
        public static readonly string MSG_PLACE_COUNTRY_NOT_FOUND = "MSG_PLACE_COUNTRY_NOT_FOUND";
        public static readonly string MSG_PLACE_CODE_EMPTY = "MSG_PLACE_CODE_EMPTY";
        public static readonly string MSG_PLACE_CODE_EXISTED = "MSG_PLACE_CODE_EXISTED";
        public static readonly string MSG_PLACE_PROVINCE_NAME_EMPTY = "Province name is not allow empty!|wrong";
        public static readonly string MSG_PLACE_PROVINCE_NOT_FOUND = "MSG_PLACE_PROVINCE_NOT_FOUND";
        public static readonly string MSG_PLACE_DISTRICT_NAME_EMPTY = "Province name is not allow empty!|wrong";
        public static readonly string MSG_PLACE_DISTRICT_NOT_FOUND = "MSG_PLACE_PROVINCE_NOT_FOUND";
        public static readonly string MSG_PLACE_CODE_DUPLICATE = "MSG_PLACE_CODE_DUPLICATE";
        public static readonly string MSG_PLACE_PORTINDEX_MODE_EMPTY = "MSG_PLACE_PORTINDEX_MODE_EMPTY";
        public static readonly string MSG_PLACE_PORTINDEX_MODE_NOT_FOUND = "MSG_PLACE_PORTINDEX_MODE_NOT_FOUND";
        public static readonly string MSG_PLACE_PORTINDEX_AREA_NOT_FOUND = "MSG_PLACE_PORTINDEX_AREA_NOT_FOUND";
        public static readonly string MSG_PLACE_ADDRESS_EMPTY = "MSG_PLACE_ADDRESS_EMPTY";
        #endregion

        #region ChargeDefault
        public static readonly string MSG_CHARGE_DEFAULT_CODE_EMPTY = "MSG_CHARGE_DEFAULT_CODE_EMPTY";
        public static readonly string MSG_CHARGE_DEFAULT_CODE_NOT_FOUND = "MSG_CHARGE_DEFAULT_CODE_NOT_FOUND";
        public static readonly string MSG_CHARGE_DEFAULT_VOUCHER_TYPE_EMPTY = "MSG_CHARGE_DEFAULT_VOUCHER_TYPE_EMPTY";
        public static readonly string MSG_CHARGE_DEFAULT_ACCOUNT_DEBIT_EMPTY = "MSG_CHARGE_DEFAULT_ACCOUNT_DEBIT_EMPTY";
        public static readonly string MSG_CHARGE_DEFAULT_ACCOUNT_CREDIT_EMPTY = "MSG_CHARGE_DEFAULT_ACCOUNT_CREDIT_EMPTY";
        #endregion

        #region Charge
        public static readonly string MSG_CHARGE_NAME_EN_EMPTY = "MSG_CHARGE_NAME_EN_EMPTY";
        public static readonly string MSG_CHARGE_NAME_LOCAL_EMPTY = "MSG_CHARGE_NAME_LOCAL_EMPTY";
        public static readonly string MSG_CHARGE_CURRENCY_EMPTY = "MSG_CHARGE_CURRENCY_EMPTY";
        public static readonly string MSG_CHARGE_CURRENCY_NOT_FOUND = "MSG_CHARGE_CURRENCY_NOT_FOUND";
        public static readonly string MSG_CHARGE_TYPE_EMPTY = "MSG_CHARGE_TYPE_EMPTY";
        public static readonly string MSG_CHARGE_SERVICE_TYPE_EMPTY = "MSG_CHARGE_SERVICE_TYPE_EMPTY";
        public static readonly string MSG_CHARGE_CODE_EMPTY = "MSG_CHARGE_CODE_EMPTY";
        public static readonly string MSG_CHARGE_CODE_EXISTED = "MSG_CHARGE_CODE_EXISTED";
        public static readonly string MSG_CHARGE_CODE_DUPLICATED= "MSG_CHARGE_CODE_DUPLICATED";
        public static readonly string MSG_CHARGE_UNIT_EMPTY = "MSG_CHARGE_UNIT_EMPTY";
        public static readonly string MSG_CHARGE_UNIT_NOT_FOUND = "MSG_CHARGE_UNIT_NOT_FOUND";
        #endregion

        #region commodity group
        public static readonly string MSG_COMMOIDITY_CODE_EMPTY = "MSG_COMMOIDITY_CODE_EMPTY";
        public static readonly string MSG_COMMOIDITY_CODE_EXISTED= "MSG_COMMOIDITY_CODE_EXISTED";
        public static readonly string MSG_COMMOIDITY_CODE_DUPLICATED = "MSG_COMMOIDITY_CODE_DUPLICATED";
        public static readonly string MSG_COMMOIDITY_NAME_EN_EMPTY = "MSG_COMMOIDITY_GROUP_NAME_EN_EMPTY";
        public static readonly string MSG_COMMOIDITY_NAME_LOCAL_EMPTY = "MSG_COMMOIDITY_GROUP_NAME_LOCAL_EMPTY";
        public static readonly string MSG_COMMOIDITY_STATUS_EMPTY = "MSG_COMMOIDITY_GROUP_STATUS_EMPTY";
        #endregion

        #region country
        public static readonly string MSG_COUNTRY_NAME_EN_EMPTY = "MSG_COUNTRY_NAME_EN_EMPTY";
        public static readonly string MSG_COUNTRY_NAME_LOCAL_EMPTY = "MSG_COUNTRY_NAME_LOCAL_EMPTY";
        public static readonly string MSG_COUNTRY_CODE_EMPTY = "MSG_COUNTRY_CODE_EMPTY";
        public static readonly string MSG_COUNTRY_EXISTED = "MSG_COUNTRY_EXISTED";
        public static readonly string MSG_COUNTRY_CODE_DUPLICATE = "MSG_COUNTRY_CODE_DUPLICATE";
        #endregion

        #region Stage
        public static readonly string MSG_STAGE_NAME_EN_EMPTY = "MSG_STAGE_NAME_EN_EMPTY";
        public static readonly string MSG_STAGE_CODE_EMPTY = "MSG_STAGE_CODE_EMPTY";
        public static readonly string MSG_STAGE_EXISTED = "MSG_STAGE_EXISTED";
        public static readonly string MSG_STAGE_CODE_DUPLICATE = "MSG_STAGE_CODE_DUPLICATE";
        public static readonly string MSG_STAGE_STATUS_EMPTY = "MSG_STAGE_STATUS_EMPTY";
        #endregion

        //#region Name field
        //public const string EF_DISPLAYNAME_CODE = "EF_DISPLAYNAME_CODE";
        //#endregion
        #region voucher advance payment
        public static readonly string MSG_ADVANCE_NO_EMPTY = "MSG_ADVANCE_NO_EMPTY";
        public static readonly string MSG_ADVANCE_NO_NOT_EXIST = "MSG_ADVANCE_NO_NOT_EXIST";
        public static readonly string MSG_VOUCHER_NO_EMPTY = "MSG_VOUCHER_NO_EMPTY";
        public static readonly string MSG_VOUCHER_DATE_EMPTY = "MSG_VOUCHER_DATE_EMPTY";
        public static readonly string MSG_ADVANCE_NO_DUPLICATE = "MSG_ADVANCE_NO_DUPLICATE";
        public static readonly string MSG_VOUCHER_DATE_NOT_VALID = "MSG_VOUCHER_DATE_NOT_VALID";
        public static readonly string MSG_ADVANCE_NO_NOT_APPROVAL = "MSG_ADVANCE_NO_NOT_APPROVAL";
        #endregion

        #region voucher advance payment
        public static readonly string MSG_VOUCHER_ID_EMPTY = "MSG_VOUCHER_ID_EMPTY";
        public static readonly string MSG_VOUCHER_ID_NOT_EXIST = "MSG_VOUCHER_ID_NOT_EXIST";
        public static readonly string MSG_VOUCHER_ID_DUPLICATE = "MSG_VOUCHER_ID_DUPLICATE";
        public static readonly string MSG_INVOICE_DATE_NOT_EMPTY = "MSG_INVOICE_DATE_NOT_EMPTY";
        public static readonly string MSG_SERIE_NO_NOT_EMPTY = "MSG_SERIE_NO_NOT_EMPTY";
        public static readonly string MSG_INVOICE_NO_NOT_EMPTY = "MSG_INVOICE_NO_NOT_EMPTY";
        public static readonly string MSG_SERIE_NO_DUPLICATE = "MSG_SERIE_NO_DUPLICATE";
        public static readonly string MSG_INVOICE_DUPLICATE = "MSG_INVOICE_DUPLICATE";
        public static readonly string MSG_VOUCHER_ID_DUPLICATE_ROW = "MSG_VOUCHER_ID_DUPLICATE_ROW";

        public static readonly string MSG_INVOICE_NO_EXISTED = "MSG_INVOICE_NO_EXISTED";
        public static readonly string MSG_SERIE_NO_EXISTED = "MSG_SERIE_NO_EXISTED";






        #endregion

    }
}
