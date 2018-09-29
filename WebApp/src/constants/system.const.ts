export class SystemConstants {

    //Local Storage Key
    public static readonly LSK_CURRENT_USER: string = 'CURRENT_USER';
    public static readonly LSK_CURRENT_WORKPLACE_ID: string = 'CURRENT_WORKPLACE_ID';
    public static readonly LSK_CURRENT_LANG: string = 'CURRENT_LANG';
    public static readonly LSL_CACHE_REMEMBER: string = 'CACHE_REMEMBER';

    //Security
    public static readonly PATH_RSA_PUBLIC_KEY = './auth/z_rsa_public.key';
    public static readonly PATH_HOME_PAGE = '/app/main/home';
    public static readonly PATH_LOGIN_PAGE = '/login';
    public static readonly AUTH_ISSUER = 'http://localhost:5000';
    public static readonly AUTH_CLIENT_ID = 'eTMS' ;
    public static readonly AUTH_SCOPE = 'openid profile offline_access etms_scope dnt_api';

    public static CURRENT_USER;

    //Language
    public static CURRENT_LANGUAGE: string = localStorage.getItem(SystemConstants.LSK_CURRENT_LANG);
    public static CURRENT_WORKPLACE_ID: string = localStorage.getItem(SystemConstants.LSK_CURRENT_WORKPLACE_ID);

    public static readonly DEFAULT_LANGUAGE: string = "en-US";    
    public static readonly DEFAULT_HOME_PAGE: string = "/app/main/home";

    //Page size for ngx-pageination options
    public static readonly OPTIONS_PAGE_SIZE: number = 10;
    public static readonly MAX_ITEMS_PER_REQUEST:Number=100;
    
}
