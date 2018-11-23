import { SystemConstants } from "./system.const";
import {environment} from 'src/environments/environment';

export class API_MENU {
    private HOST = {
        Local: "localhost:",
        Test: "test.api-efms.itlvn.com",
        Staging: "staging.api-efms.itlvn.com"
    }

    private PORT = {
        System: 44360,
        Catalogue: 44361,
        Department: 44242
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
        },
        PartnerData: {
            query: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/Query",
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/Paging",
            customerPaging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/PagingCustomer" ,
            getById: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/",
            add: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/Add",
            update: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/",
            delete: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/",
            getDepartments: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartner/GetDepartments"
        },
        partnerGroup: {
            getAll: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatPartnerGroup"
        },
        Commodity: {
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommonity/Paging",
            getById: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommonity/",
            add: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) +  "CatCommonity/add",
            update: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommonity/",
            delete: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommonity/"
        },
        CommodityGroup: {
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommodityGroup/Paging",
            getById: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommodityGroup/",
            add: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) +  "CatCommodityGroup/add",
            update: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommodityGroup/",
            delete: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommodityGroup/",
            getAllByLanguage: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCommodityGroup/GetByLanguage"
        },
        Stage_Management: {
            getAll: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatStage/getAll",
            getById: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatStage/getById/",
            addNew: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatStage/addnew",
            update: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatStage/update",
            delete: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatStage/delete/"
        },
        Unit: {
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatUnit/Paging",
            getById: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatUnit/",
            addNew: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatUnit/Add",
            update: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatUnit/Update",
            delete: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatUnit/Delete/",
            getAll: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatUnit"
        },    
        Charge: {
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCharge/Paging",
            addNew: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCharge/addnew",
            delete:this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCharge/delete/",
            getById:this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCharge/getById/",
            update:this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCharge/update",
        },
        Country:{
            getAll: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/getAll",
            paging: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/paging",
            getById: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/getById/",
            addNew: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/addNew",
            update: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/update",
            delete: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/delete/",
            getAllByLanguage: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCountry/GetByLanguage"
        },
        Area: {
            getAllByLanguage: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatArea/GetByLanguage"
        },
        Currency:{
            getAll:this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCurrency/getAll",
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
            getBy: this.getUrlMainPath(SystemConstants.MODULE_NAME.CATALOUGE) + "CatCurrencyExchange/GetExchangeRatesBy"
        }
    }

}