import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import * as lodash from 'lodash';
import { BaseService } from 'src/services-base/base.service';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { API_MENU } from 'src/constants/api-menu.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgForm } from '@angular/forms';
import { CountryModel } from 'src/app/shared/models/catalogue/country.model';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import * as dataHelper from 'src/helper/data.helper';
import { from } from 'rxjs';
import { SystemConstants } from 'src/constants/system.const';
import { CatUnitModel } from 'src/app/shared/models/catalogue/catUnit.model';
import { reserveSlots } from '@angular/core/src/render3/instructions';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, ActivatedRoute } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { async } from 'rxjs/internal/scheduler/async';
import { CookieService } from 'ngx-cookie-service';
import { toBase64String } from '@angular/compiler/src/output/source_map';
import * as crypto_js from 'crypto-js';
import * as CryptoJS from 'crypto-js';
import { environment } from 'src/environments/environment';
import { url } from 'inspector';
// import {DataHelper} from 'src/helper/data.helper';
declare var $: any;

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  constructor(
    private baseServices: BaseService,
    private toastr: ToastrService,
    private spinnerService: Ng4LoadingSpinnerService,
    private api_menu: API_MENU,
    private el: ElementRef,
    private router: Router,
    private oauthService: OAuthService,
    private cookieService: CookieService) { }

  username: string = "";
  password: string = "";
  remember_me: boolean = false;

  ngOnInit() {
    if(this.cookieService.get("login_status")==="LOGGED_IN"){
      this.router.navigateByUrl('/home');
    }
    //this.checkLogin();
    this.getLoginData();
  }

  checkLogin() {
    console.log({ TOKEN: this.oauthService.getAccessToken() })
    if (this.oauthService.getAccessToken() != null) {     
      this.router.navigateByUrl('/home');
    } else {
      return;
    }
  }

  Login() {
    this.oauthService.fetchTokenUsingPasswordFlow(this.username, this.password).then((resp) => {
      return this.oauthService.loadUserProfile();
    }).then(() => {
      let claims = this.oauthService.getIdentityClaims();
      if (claims) console.log(claims);      
      this.rememberMe();
      this.toastr.success("Login successful !");
      this.router.navigateByUrl('/home');
      this.cookieService.set('login_status', "LOGGED_IN");
    }).catch((err) => {
      console.log(err);
      this.toastr.error(err.error.error_description)
    })
  }


  rememberMe() {
    if (this.remember_me) {  
      const userInfo = this.encryptUserInfo(this.username,this.password);
      this.cookieService.set("_u", userInfo.username_encrypt);
      this.cookieService.set("_p",userInfo.password_encrypt);
    } else {
      this.cookieService.deleteAll();
    }
  }

  encryptUserInfo(username,password){
    const username_encrypt = crypto_js.AES.encrypt(username,SystemConstants.SECRET_KEY);
    const password_encrypt = crypto_js.AES.encrypt(password,SystemConstants.SECRET_KEY);

    const returnObj = {   
      username_encrypt: username_encrypt.toString(),
      password_encrypt:password_encrypt.toString()
    }

    return returnObj;
  }

  getUserName(){
    const username_encypt = this.cookieService.get("_u");
    const bytes = crypto_js.AES.decrypt(username_encypt,SystemConstants.SECRET_KEY);
    const username = bytes.toString(CryptoJS.enc.Utf8);
    return username;
  }

  getUserPassword(){
    const pass_encypt = this.cookieService.get("_p");
    const bytes = crypto_js.AES.decrypt(pass_encypt,SystemConstants.SECRET_KEY);
    const password = bytes.toString(CryptoJS.enc.Utf8);
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
  public items:Array<string> = ['Amsterdam', 'Antwerp', 'Athens', 'Barcelona',
  'Berlin', 'Birmingham', 'Bradford', 'Bremen', 'Brussels', 'Bucharest',
  'Budapest', 'Cologne', 'Copenhagen'];

private value:any = {};
private _disabledV:string = '0';
private disabled:boolean = false;

private get disabledV():string {
  return this._disabledV;
}

private set disabledV(value:string) {
  this._disabledV = value;
  this.disabled = this._disabledV === '1';
}

public selected(value:any):void {
  console.log('Selected value is: ', value);
}

public removed(value:any):void {
  console.log('Removed value is: ', value);
}

public typed(value:any):void {
  console.log('New search input: ', value);
}

public refreshValue(value:any):void {
  this.value = value;
}

changeLanguage(lang){
  console.log(window.location.host);
  window.location.href= "http://test.efms.itlvn.com/"+lang;
}
 
}
