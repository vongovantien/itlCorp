import { SystemConstants } from "./system.const";


export class API_MENU {
    private HOST = {
        Local:"localhost:",
        Test:"test.efms.itlvn.com",
        Staging:"staging.efms.itlvn.com"
    }

    private PORT = {
        System:44360,
        Catalogue:44361,
        Department:44242
    }

    private PROTOCOL = "http://";

    /**
     * Use HOST.Local to run on local environment
     * Use HOST.Test to run on test environment
     * Use HOST.Staging to run on staging environment 
     */
    private CURRENT_HOST : String = this.HOST.Local;

    private getCurrentLanguage(){
        return localStorage.getItem(SystemConstants.CURRENT_LANGUAGE);
    }

    private getCurrentVersion(){
        return localStorage.getItem(SystemConstants.CURRENT_VERSION);
    }
 

    private getUrlMainPath(Module:String){
        
        if(this.CURRENT_HOST == this.HOST.Local){
            return this.PROTOCOL + this.HOST.Local + this.getPort(Module) + "/api" + "/v" + this.getCurrentVersion() + "/" + this.getCurrentLanguage() + "/";
        }
        if(this.CURRENT_HOST == this.HOST.Test){
            return this.PROTOCOL + this.HOST.Test + "/" + this.getCurrentVersion() + "/v" + this.getCurrentLanguage() + "/" ;
        }
        if(this.CURRENT_HOST == this.HOST.Staging){
            return this.PROTOCOL + this.HOST.Staging + "/" + this.getCurrentVersion() + "/v" + this.getCurrentLanguage() + "/";
        }
    }

    private getPort(Module:String){        
        return eval("this.PORT."+Module);
    }


    /**
     * CATALOGUE MODULE API URL DEFINITION 
     */
    public Catalogue = {
        Warehouse:{

        },
        PortIndex:{

        },
        PartnerData:{

        },
        Commodity:{

        },
        Stage_Management:{
            /**
             * Get all stages 
             */
            Get_All: this.getUrlMainPath("Catalogue") + "CatStage/get_all",
            Get_By_Id: this.getUrlMainPath("Catalogue") + "CatStage/get_by_id/",
            Add_New: this.getUrlMainPath("Cataloge") + "CatStage/add_new",
            Update: this.getUrlMainPath("Catalogue") + "CatStage/update",
            Delete: this.getUrlMainPath("Catalogue") + "CatStage/delete/"
        },
        Unit:{

        },
        Location:{

        },
        Charge:{

        }

        
    }

     /**
     * SYSTEM MODULE API URL DEFINITION 
     */
    public System = {
        User_Management:{

        },
        Group:{

        },
        Role:{

        },
        Permission:{

        },
        Department:{
            CatDeparment: this.getUrlMainPath("Department") + "CatDepartment"
        },
        Company_Info:{

        }
    }

 


}