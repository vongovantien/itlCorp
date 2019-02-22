import { SystemConstants } from "./system.const";
import {environment} from 'src/environments/environment';
import { CsTransactionDetail } from "../app/shared/models/document/csTransactionDetail";

export class API_MENU {
    private HOST = {
        Local: "localhost:",
        Test: "test.api-efms.itlvn.com",
        Staging: "staging.api-efms.itlvn.com"
    }

    private PORT = {
        System: 44360,
        Catalogue: 44361,
        Department: 44242,
        auditlog: 44363,
        Documentation: 44366
    }

    private PROTOCOL = "http://";

    /**
     * Use HOST.Local to run on local environment
     * Use HOST.Test to run on test environment
     * Use HOST.Staging to run on staging environment 
     */
    private CURRENT_HOST: String =  environment.HOST.WEB_URL;  //this.HOST.Local;

    private getCurrentLanguage() {
        return localStorage.getItem(SystemConstants.CURRENT_LANGUAGE);
    }

    private getCurrentVersion() {
        return localStorage.getItem(SystemConstants.CURRENT_VERSION);
    }


    private getUrlMainPath(Module: String) {

        if (this.CURRENT_HOST == this.HOST.Local) {
            return this.PROTOCOL + this.HOST.Local + this.getPort(Module) + "/api/v" + this.getCurrentVersion() + "/" + this.getCurrentLanguage() + "/";
        }
        if (this.CURRENT_HOST == this.HOST.Test) {
            return this.PROTOCOL + this.HOST.Test + "/" + Module + "/api/v" + this.getCurrentVersion() + "/" + this.getCurrentLanguage() + "/";
        }
        if (this.CURRENT_HOST == this.HOST.Staging) {
            return this.PROTOCOL + this.HOST.Staging + "/" + Module + "/api/v" + this.getCurrentVersion() + "/" + this.getCurrentLanguage() + "/";
        }
    }

    private getPort(Module: String) {
        return eval("this.PORT." + Module);
    }


    /**
     * CATALOGUE MODULE API URL DEFINITION 
     */
    public Catalogue = {
        CatPlace: {
            query: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) +  "CatPlace/Query",
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) +  "CatPlace/Paging",
            getById: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) +  "CatPlace/",
            add: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) +  "CatPlace/Add",
            update: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPlace/",
            delete: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPlace/",
            getProvinces: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPlace/GetProvinces",
            getDistricts: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPlace/GetDistricts",
            getModeOfTransport: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPlace/GetModeOfTransport",
            downloadExcel: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPlace/DownloadExcel",
            uploadExel: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPlace/UpLoadFile",
            import: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPlace/Import"
        },
        PartnerData: {
            query: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/Query",
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/Paging",
            customerPaging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/PagingCustomer" ,
            getById: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/",
            add: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/Add",
            update: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/",
            delete: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/",
            getDepartments: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/GetDepartments",
            downloadExcel: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/DownloadExcel",
            uploadExel: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/UpLoadFile",
            import: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/Import"
        },
        partnerGroup: {
            getAll: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartnerGroup"
        },
        Commodity: {
            query: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommonity/Query",
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommonity/Paging",
            getById: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommonity/",
            add: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) +  "CatCommonity/add",
            update: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommonity/",
            delete: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommonity/",
            uploadFile : this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommonity/uploadFile",
            import : this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommonity/import",
            downloadExcel : this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommonity/downloadExcel",
        },
        CommodityGroup: {
            query: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommodityGroup/Query",
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommodityGroup/Paging",
            getById: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommodityGroup/",
            add: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) +  "CatCommodityGroup/add",
            update: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommodityGroup/",
            delete: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommodityGroup/",
            getAllByLanguage: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommodityGroup/GetByLanguage",
            uploadFile : this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommodityGroup/uploadFile",
            import : this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommodityGroup/import",
            downloadExcel : this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommodityGroup/downloadExcel",
        },
        Stage_Management: {
            query: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatStage/query",
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatStage/getAll",
            getById: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatStage/getById/",
            addNew: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatStage/addnew",
            update: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatStage/update",
            delete: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatStage/delete/",
            uploadExel: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatStage/uploadFile",
            import: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatStage/import",
            downloadExcel: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatStage/downloadExcel",
        },
        Unit: {
            getAllByQuery: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatUnit/Query",
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatUnit/Paging",
            getById: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatUnit/",
            addNew: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatUnit/Add",
            update: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatUnit/Update",
            delete: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatUnit/Delete/",
            getAll: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatUnit",
            getUnitTypes: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatUnit/GetUnitTypes"
        },    
        Charge: {
            query: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCharge/Query",
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCharge/Paging",
            addNew: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCharge/addnew",
            delete:this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCharge/delete/",
            getById:this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCharge/getById/",
            update:this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCharge/update",
            uploadExel: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE)+"CatCharge/uploadFile",
            import: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCharge/import",
            downloadExcel: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCharge/downloadExcel",
        },
        Charge_DefaultAccount:{
            uploadExel: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE)+"CatChargeDefaultAccount/uploadFile",
            import: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatChargeDefaultAccount/import",
            downloadExcel: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatChargeDefaultAccount/downloadExcel",
        },
        Country:{            
            query: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/query",
            getAll: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/getAll",
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/paging",
            getById: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/getById/",
            addNew: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/addNew",
            update: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/update",
            delete: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/delete/",
            getAllByLanguage: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/GetByLanguage",
            downloadExcel: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/DownloadExcel",
            uploadExel: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/UpLoadFile",
            import: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/Import"
        },
        Area: {
            getAllByLanguage: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatArea/GetByLanguage"
        },
        Currency:{
            getAll:this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCurrency/getAll",
            getAllByQuery:this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCurrency/getAllByQuery",
            getById: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCurrency/getById/",
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCurrency/paging",
            addNew:this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCurrency/add",
            update:this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCurrency/update",
            delete:this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCurrency/",
        }
    }

    /**
    * SYSTEM MODULE API URL DEFINITION 
    */
    public System = {
        User_Management: {
            getAll: this.getUrlMainPath(SystemConstants.MODULE_NAME.SYSTEM) + "SysUser",
            login : this.getUrlMainPath(SystemConstants.MODULE_NAME.SYSTEM) + "SysUser/login",
            getUserByID: this.getUrlMainPath(SystemConstants.MODULE_NAME.SYSTEM) + "SysUser/GetById/",
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.SYSTEM) + "SysUser/Paging"
        },
        Group: {

        },
        Role: {

        },
        Permission: {

        },
        Department: {
            getAll: this.getUrlMainPath(SystemConstants.MODULE_NAME.SYSTEM) + "CatDepartment"
        },
        Company_Info: {

        },
        Employee: {
            query: this.getUrlMainPath(SystemConstants.MODULE_NAME.SYSTEM) + "SysEmployee/query"
        }
    }

    /**
    * TOOL-SETTING MODULE API URL DEFINITION 
    */

    public ToolSetting = {
        ExchangeRate: {
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCurrencyExchange/GetExchangeRateHistory/Paging",
            getNewest: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCurrencyExchange/GetNewest",
            getBy: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCurrencyExchange/GetExchangeRatesBy",
            updateRate: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCurrencyExchange/UpdateRate",
            getCurrencies: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCurrencyExchange/GetCurrencies",
            convertRate: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCurrencyExchange/ConvertRate"
        },
        CatalogueLogViewer: {
            getCategory: this.getUrlMainPath(SystemConstants.MODULE_NAME.LOG) + "CategoryLog/GetCategory",
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.LOG) + "CategoryLog/Paging"
        }
    }

    public Documentation = {
        Terminology: {
            getShipmentCommonData : this.getUrlMainPath(SystemConstants.MODULE_NAME.Documentation) + "Terminology/getShipmentCommonData",
        },
        CsTransaction: {
            post: this.getUrlMainPath(SystemConstants.MODULE_NAME.Documentation) + "CsTransaction",
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.Documentation) + "CsTransaction/Paging",
            getById: this.getUrlMainPath(SystemConstants.MODULE_NAME.Documentation) + "CsTransaction/"
        },
        CsTransactionDetail: {
            getByJob: this.getUrlMainPath(SystemConstants.MODULE_NAME.Documentation) + "CsTransactionDetail/GetByJob",
            addNew: this.getUrlMainPath(SystemConstants.MODULE_NAME.Documentation) + "CsTransactionDetail/addNew"
        }
    }
}