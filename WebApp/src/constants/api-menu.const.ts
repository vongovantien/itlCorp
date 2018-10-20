import { SystemConstants } from "./system.const";


export class API_MENU {
    private HOST = {
        Local:"localhost:",
        Test:"test.efms.itlvn.com",
        Staging:"staging.efms.itlvn.com"
    }

    private PORT = {
        System:44360,
        Catalogue:44361
    }

    private CURRENT_HOST : String = this.HOST.Local;

    private getCurrentLanguage(){
        return localStorage.getItem(SystemConstants.CURRENT_LANGUAGE);
    }

    private getCurrentVersion(){
        return localStorage.getItem(SystemConstants.CURRENT_VERSION);
    }
 

    private getUrlMainPath(Module:String){
        
        if(this.CURRENT_HOST == this.HOST.Local){
            return this.HOST.Local + this.getPort(Module) + "/" + this.getCurrentVersion() + "/" + this.getCurrentLanguage() + "/";
        }
        if(this.CURRENT_HOST == this.HOST.Test){
            return this.HOST.Test + "/" + this.getCurrentVersion() + "/" + this.getCurrentLanguage() + "/" ;
        }
        if(this.CURRENT_HOST == this.HOST.Staging){
            return this.HOST.Staging + "/" + this.getCurrentVersion() + "/" + this.getCurrentLanguage() + "/";
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
            CatStage: this.getUrlMainPath("Catalogue") + "CatStage",

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

        },
        Company_Info:{
            
        }
    }

 


}