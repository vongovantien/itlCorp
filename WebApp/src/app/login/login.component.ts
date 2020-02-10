import { Component } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { NgForm } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';

import { SystemConstants } from 'src/constants/system.const';
import { OAuthService, JwksValidationHandler } from 'angular-oauth2-oidc';
import { CookieService } from 'ngx-cookie-service';
import crypto_js from 'crypto-js';
import { authConfig } from '../shared/authenticate/authConfig';
import { BaseService } from 'src/app/shared/services/base.service';

import { RSAHelper } from 'src/helper/RSAHelper';
import { SystemRepo } from '../shared/repositories/system.repo';
import { Observable } from 'rxjs';
import { Company } from '@models';
import $ from 'jquery';
import { share } from 'rxjs/operators';
import { HttpHeaders } from '@angular/common/http';


@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent {
    username: string = "";
    password: string = "";
    remember_me: boolean = false;

    currenURL: string = ''; // * URL before redirect to login.

    company$: Observable<Company[]>;

    selectedCompanyId: any;

    ngAfterViewInit(): void {
        if (this.route.snapshot.paramMap.get("isEndSession")) {
            setTimeout(() => {
                this.baseService.warningToast("Login again to continue, please !", "Expired Session");
            }, 50);
        }

        this.getLoginData();
    }

    constructor(
        private toastr: ToastrService,
        private baseService: BaseService,
        private router: Router,
        private route: ActivatedRoute,
        private oauthService: OAuthService,
        private cookieService: CookieService,
        private _systemRepo: SystemRepo
    ) {
        this.oauthService.setStorage(localStorage);
        this.oauthService.setupAutomaticSilentRefresh();
    }

    private async configureWithNewConfigApi() {
        // this.oauthService.configure(authConfig);
        this.oauthService.tokenValidationHandler = new JwksValidationHandler();
        await this.oauthService.loadDiscoveryDocumentAndTryLogin();
    }


    ngOnInit() {
        // Load company list.
        this.company$ = this._systemRepo.getListCompany().pipe(share());

        this.company$.subscribe(
            (companies: any) => {
                this.selectedCompanyId = companies[0].id;
            }
        );

        if (this.baseService.checkLoginSession()) {
            this.setupLocalInfo();
            this.router.navigateByUrl('/');
        }
    }

    async Login(form: NgForm) {
        if (form.form.status !== "INVALID" && !!this.selectedCompanyId) {
            try {
                this.baseService.spinnerShow();
                this.currenURL = this.route.snapshot.paramMap.get("url") || 'home/dashboard';

                await this.configureWithNewConfigApi();
                const passwordEncoded = RSAHelper.serverEncode(this.password);

                const header: HttpHeaders = new HttpHeaders({
                    companyId: this.selectedCompanyId
                });
                this.oauthService.fetchTokenUsingPasswordFlow(this.username, passwordEncoded, header) // * Request Access Token.
                    .then((resp: any) => {
                        return this.oauthService.loadUserProfile();
                    }).then(() => {
                        const userInfo: SystemInterface.IClaimUser = <any>this.oauthService.getIdentityClaims(); // * Get info User.
                        if (!!userInfo) {
                            localStorage.setItem("currently_userName", userInfo.preferred_username);
                            // localStorage.setItem("currently_userEmail", userInfo['email']);
                            this.setupLocalInfo();
                            this.rememberMe();

                            // * CURRENT_URL: url before into auth guard.
                            if (this.currenURL.includes("login")) {
                                this.currenURL = "home/dashboard";
                            }
                            this.router.navigateByUrl(this.currenURL);
                            this.baseService.spinnerHide();
                            this.toastr.info("Welcome back, " + userInfo.userName.toUpperCase() + " !", "Login Success");
                        }
                    }).catch((err) => {
                        this.baseService.spinnerHide();
                    });
            } catch (error) {
                this.baseService.spinnerHide();
            }
        }
    }

    rememberMe() {
        if (this.remember_me) {
            const userInfo = this.encryptUserInfo(this.username, this.password);
            this.cookieService.set("_u", userInfo.username_encrypt, null, "/", window.location.hostname);
            this.cookieService.set("_p", userInfo.password_encrypt, null, "/", location.hostname);
        } else {
            this.cookieService.deleteAll("/", window.location.hostname);
        }
    }

    encryptUserInfo(username: string | crypto_js.LibWordArray, password: string | crypto_js.LibWordArray) {
        const username_encrypt = crypto_js.AES.encrypt(username, SystemConstants.SECRET_KEY);
        const password_encrypt = crypto_js.AES.encrypt(password, SystemConstants.SECRET_KEY);

        const returnObj = {
            username_encrypt: username_encrypt.toString(),
            password_encrypt: password_encrypt.toString()
        };

        return returnObj;
    }

    getUserName() {
        const username_encypt = this.cookieService.get("_u");
        const bytes = crypto_js.AES.decrypt(username_encypt, SystemConstants.SECRET_KEY);
        const username = bytes.toString(crypto_js.enc.Utf8);
        return username;
    }

    getUserPassword() {
        const pass_encypt = this.cookieService.get("_p");
        const bytes = crypto_js.AES.decrypt(pass_encypt, SystemConstants.SECRET_KEY);
        const password = bytes.toString(crypto_js.enc.Utf8);
        return password;
    }

    private getLoginData() {
        this.username = this.getUserName();
        this.password = this.getUserPassword();
        this.remember_me = (this.username != '' || this.password != '');
    }

    changeLanguage(lang: string) {
        localStorage.setItem(SystemConstants.CURRENT_CLIENT_LANGUAGE, lang);
        if (localStorage.getItem(SystemConstants.CURRENT_CLIENT_LANGUAGE) === "en") {
            localStorage.setItem(SystemConstants.CURRENT_LANGUAGE, "en-US");
            window.location.href = window.location.protocol + "//" + window.location.hostname;
        } else {
            localStorage.setItem(SystemConstants.CURRENT_LANGUAGE, "vi-VN");
            window.location.href = window.location.protocol + "//" + window.location.hostname + "/" + lang + "/";
        }
    }


    setupLocalInfo() {
        if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) == null) {
            localStorage.setItem("CURRENT_LANGUAGE", SystemConstants.DEFAULT_LANGUAGE);
        }
        if (localStorage.getItem(SystemConstants.CURRENT_VERSION) == null) {
            localStorage.setItem("CURRENT_VERSION", "1");
        }
        const current_client_lang = localStorage.getItem(SystemConstants.CURRENT_CLIENT_LANGUAGE);

        if (current_client_lang === null) {
            localStorage.setItem(SystemConstants.CURRENT_CLIENT_LANGUAGE, "en");
        }
    }

}


