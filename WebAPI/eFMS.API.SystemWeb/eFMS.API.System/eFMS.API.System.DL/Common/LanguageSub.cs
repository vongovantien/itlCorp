using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Common
{
    public partial class SystemLanguageSub
    {
        //Global
        //public static readonly string MSG_DATA_NOT_FOUND = "MSG_DATA_NOT_FOUND";
        //public static readonly string MSG_OBJECT_NOT_EXISTS = "MSG_OBJECT_NOT_EXISTS";
        //public static readonly string MSG_OBJECT_DUPLICATED = "MSG_OBJECT_DUPLICATED";

        //public static readonly string MSG_OBJECT_RELATION_NOT_VALID = "MSG_OBJECT_RELATION_NOT_VALID";

        //public static readonly string MSG_INSERT_SUCCESS = "MSG_INSERT_SUCCESS";

        //public static readonly string MSG_UPDATE_SUCCESS = "MSG_UPDATE_SUCCESS";

        //public static readonly string MSG_DELETE_SUCCESS = "MSG_DELETE_SUCCESS";

        //public static readonly string MSG_NAME_EXISTED = "MSG_NAME_EXISTED";

        //public static readonly string MSG_CODE_EXISTED = "MSG_CODE_EXISTED";
        //public static readonly string MSG_DELETE_FAIL_INCLUDED_CHILD = "MSG_DELETE_FAIL_INCLUDED_CHILD";
        //public static readonly string EF_ANNOTATIONS_REQUIRED = "EF_ANNOTATIONS_REQUIRED";
        //public static readonly string EF_ANNOTATIONS_STRING_LENGTH = "EF_ANNOTATIONS_STRING_LENGTH";
        //public static readonly string FILE_NOT_FOUND = "FILE_NOT_FOUND";
        //public static readonly string NOT_FOUND_DATA_EXCEL = "NOT_FOUND_DATA_EXCEL";

        public static readonly string MSG_USERNAME_EXISTED = "MSG_USERNAME_EXISTED";


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
        #region User
        public static readonly string MSG_USER_USERNAME_EMPTY = "MSG_USER_USERNAME_EMPTY";
        public static readonly string MSG_USER_USERNAME_EXISTED = "MSG_USER_USERNAME_EXISTED";

        public static readonly string MSG_USER_NAMEEN_EMPTY = "MSG_USER_NAMEEN_EMPTY";
        public static readonly string MSG_USER_NAMEVN_EMPTY = "MSG_USER_NAMEVN_EMPTY";
        public static readonly string MSG_USER_USERTYPE_EMPTY = "MSG_USER_USERTYPE_EMPTY";
        public static readonly string MSG_USER_WORKINGSTATUS_EMPTY = "MSG_USER_WORKINGSTATUS_EMPTY";
        public static readonly string MSG_USER_STATUS_EMPTY = "MSG_USER_STATUS_EMPTY";
        public static readonly string MSG_USER_STAFFCODE_EMPTY = "MSG_USER_STAFFCODE_EMPTY";
        public static readonly string MSG_USER_TITLE_EMPTY = "MSG_USER_TITLE_EMPTY";
        public static readonly string MSG_USER_STAFFCODE_EXISTED = "MSG_USER_STAFFCODE_EXISTED";
        public static readonly string MSG_USER_STAFFCODE_DUPLICATE = "MSG_USER_STAFFCODE_DUPLICATE";
        public static readonly string MSG_USER_USERNAME_DUPLICATE = "MSG_USER_USERNAME_DUPLICATE";

        public static readonly string MSG_USER_USERTYPE_NOTFOUND = "MSG_USER_USERTYPE_NOTFOUND";
        public static readonly string MSG_USER_EMAIL_EMPTY = "MSG_USER_EMAIL_EMPTY";
        public static readonly string MSG_USER_STATUS_NOTFOUND = "MSG_USER_STATUS_NOTFOUND";
        public static readonly string MSG_USER_TEL_EMPTY = "MSG_USER_TEL_EMPTY";
        public static readonly string MSG_USER_USERROLE_EMPTY = "MSG_USER_USERROLE_EMPTY";
        public static readonly string MSG_USER_USERROLE_NOTFOUND = "MSG_USER_USERROLE_NOTFOUND";








        #endregion

        #region Email
        public static readonly string MSG_EMAIL_EN_EXISTED_IN_DEPT = "MSG_EMAIL_EN_EXISTED_IN_DEPT";
        public static readonly string MSG_EMAIL_EN_NOT_EXISTED_IN_DEPT = "MSG_EMAIL_EN_NOT_EXISTED_IN_DEPT";
        #endregion


        //#region Name field
        //public const string EF_DISPLAYNAME_CODE = "EF_DISPLAYNAME_CODE";
        //#endregion

        public static readonly string MSG_ITEM_IS_ACTIVE_NOT_ALLOW_DELETED = "MSG_ITEM_IS_ACTIVE_NOT_ALLOW_DELETED";

        public static readonly string MSG_ITEM_EXPIRATION_DATE_GREATER_OR_EQUAL_EFFECTIVE_DATE = "MSG_ITEM_EXPIRATION_DATE_GREATER_OR_EQUAL_EFFECTIVE_DATE";
        public static readonly string MSG_ITEM_SIMILAR_AUTHORIZATION = "MSG_ITEM_SIMILAR_AUTHORIZATION";

        public static readonly string MSG_ITEM_DUPLICATE_USER_ON_USER_LEVEL = "MSG_ITEM_DUPLICATE_USER_ON_USER_LEVEL";
        public static readonly string MSG_ITEM_EXISTED_USER_ON_USER_LEVEL = "MSG_ITEM_EXISTED_USER_ON_USER_LEVEL";

        public static readonly string MSG_ITEM_DUPLICATE_ROLE_ON_USER = "MSG_ITEM_DUPLICATE_ROLE_ON_USER";

        public static readonly string MSG_ITEM__EXISTED_ACCOUNTANT_DEPT_IN_OFFICE = "MSG_ITEM__EXISTED_ACCOUNTANT_DEPT_IN_OFFICE";

    }
}
