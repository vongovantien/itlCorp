import { Component, OnInit, AfterViewInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { SystemConstants } from 'src/constants/system.const';
import { Router } from '@angular/router';
import { OAuthService, JwksValidationHandler } from 'angular-oauth2-oidc';
import { CookieService } from 'ngx-cookie-service';
import * as crypto_js from 'crypto-js';
import { NgForm } from '@angular/forms';
import { authConfig } from '../shared/authenticate/authConfig';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, AfterViewInit {
  ngAfterViewInit(): void {
    if (this.cookieService.get("login_status") === "LOGGED_IN") {
      this.router.navigateByUrl('/home');
    }
    this.getLoginData();
  }

  constructor(
    private toastr: ToastrService,
    private router: Router,
    private oauthService: OAuthService,
    private cookieService: CookieService,
    private spinnerService: Ng4LoadingSpinnerService) {

  }

  private async configureWithNewConfigApi() {
    this.oauthService.configure(authConfig);
    this.oauthService.tokenValidationHandler = new JwksValidationHandler();
    await this.oauthService.loadDiscoveryDocumentAndTryLogin();
    this.oauthService.setupAutomaticSilentRefresh();
  }

  username: string = "";
  password: string = "";
  remember_me: boolean = false;

  ngOnInit() {

  }

  async Login(form: NgForm) {
    this.spinnerService.show();
    await this.configureWithNewConfigApi();
    if (form.form.status !== "INVALID") {
      this.oauthService.fetchTokenUsingPasswordFlow(this.username, this.password).then((resp) => {
        return this.oauthService.loadUserProfile();
      }).then(() => {
        let claims = this.oauthService.getIdentityClaims();
        if (claims) {
        
          console.log(claims);
          this.rememberMe();
          this.toastr.success("Welcome back, "+claims['preferred_username'].toUpperCase()+" !", "", { positionClass: 'toast-bottom-right' });
          this.router.navigateByUrl('/home');
          this.cookieService.set('login_status', "LOGGED_IN", null, "/", window.location.hostname);
          this.spinnerService.hide();
        }
      }).catch((err) => {
        this.toastr.error(err.error.error_description, "", { positionClass: 'toast-bottom-right' })
        this.spinnerService.hide();
      })
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

  encryptUserInfo(username, password) {
    const username_encrypt = crypto_js.AES.encrypt(username, SystemConstants.SECRET_KEY);
    const password_encrypt = crypto_js.AES.encrypt(password, SystemConstants.SECRET_KEY);

    const returnObj = {
      username_encrypt: username_encrypt.toString(),
      password_encrypt: password_encrypt.toString()
    }

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

  /**
   * ng2-select
   */
  public items: Array<string> = ['Amsterdam', 'Antwerp', 'Athens', 'Barcelona',
    'Berlin', 'Birmingham', 'Bradford', 'Bremen', 'Brussels', 'Bucharest',
    'Budapest', 'Cologne', 'Copenhagen'];

  private value: any = {};
  private _disabledV: string = '0';
  private disabled: boolean = false;

  private get disabledV(): string {
    return this._disabledV;
  }

  private set disabledV(value: string) {
    this._disabledV = value;
    this.disabled = this._disabledV === '1';
  }

  public selected(value: any): void {
    console.log('Selected value is: ', value);
  }

  public removed(value: any): void {
    console.log('Removed value is: ', value);
  }

  public typed(value: any): void {
    console.log('New search input: ', value);
  }

  public refreshValue(value: any): void {
    this.value = value;
  }

  changeLanguage(lang) {
    localStorage.setItem(SystemConstants.CURRENT_CLIENT_LANGUAGE, lang);
    if (localStorage.getItem(SystemConstants.CURRENT_CLIENT_LANGUAGE) === "en") {
      window.location.href = window.location.protocol + "//" + window.location.hostname;
    } else {
      window.location.href = window.location.protocol + "//" + window.location.hostname + "/" + lang + "/";
    }
  }

}
