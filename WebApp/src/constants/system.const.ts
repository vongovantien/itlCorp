export class SystemConstants {

    //Local Storage Key
    public static readonly CURRENT_CLIENT_LANGUAGE:string = 'CURRENT_CLIENT_LANGUAGE';
    public static readonly CURRENT_LANGUAGE:string = "CURRENT_LANGUAGE";
    public static readonly CURRENT_VERSION:string = "CURRENT_VERSION";
    public static readonly LOGIN_STATUS:string = "LOGIN_STATUS";
    public static readonly LOGGED_IN : string = "LOGGED_IN";
    public static readonly LOGGED_OUT : string = "LOGGED_OUT";

    //Security
    public static readonly SECRET_KEY = "ITL-$EFMS-&SECRET_KEY001";
    public static readonly USER_CLAIMS = 'id_token_claims_obj';

    //Language
    // public static CURRENT_LANGUAGE: string = localStorage.getItem(SystemConstants.LSK_CURRENT_LANG);
    // public static CURRENT_WORKPLACE_ID: string = localStorage.getItem(SystemConstants.LSK_CURRENT_WORKPLACE_ID);

    public static readonly DEFAULT_LANGUAGE: string = "en-US";    
    public static readonly DEFAULT_HOME_PAGE: string = "/app/main/home";

    //Page size for ngx-pageination options
    public static readonly OPTIONS_PAGE_SIZE: number = 15;
    public static readonly OPTIONS_NUMBERPAGES_DISPLAY: number = 10;
    public static readonly MAX_ITEMS_PER_REQUEST:number=100;
    public static readonly ITEMS_PER_PAGE: number[] = [3, 15, 30, 50];
    
    public static readonly MODULE_NAME = {
        CATALOUGE: "Catalogue",
        SYSTEM: "System",
        LOG: "auditlog",
        Documentation: "Documentation",
        Report: "ReportPreview",
        SETTING:"Setting"
    }
    public static readonly LANGUAGES = {
        ENGLISH_API:  "en-US",  //SystemConstants.DEFAULT_LANGUAGE,
        VIETNAM_API: "vi-VN",
        ENGLISH: "en",
        VIETNAM:"vi"
    }

    public static readonly STATUS_BY_LANG ={
        INACTIVE:{
            ENGLISH:"Inactive",
            VIETNAM:"Ngưng Hoạt Động"
        },
        ACTIVE:{
            ENGLISH:"Active",
            VIETNAM:"Đang Hoạt Động"
        }
    }
}
