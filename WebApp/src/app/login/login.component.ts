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
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { async } from 'rxjs/internal/scheduler/async';
import { CookieService } from 'ngx-cookie-service';
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
    this.checkLogin();
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
      this.toastr.error(err.error.error_description)
    })
  }


  rememberMe() {
    if (this.remember_me) {
      this.cookieService.set('uSnGTX23NJKLX=', this.username);
      this.cookieService.set('pWNEAExy2HBXS=', this.password);
    } else {
      this.cookieService.deleteAll();
    }
  }

  private getLoginData() {
    this.username = this.cookieService.get('uSnGTX23NJKLX=');
    this.password = this.cookieService.get('pWNEAExy2HBXS=');
    this.remember_me = (this.username != '' || this.password != '');
  }

  /**
   * ng2-select
   */
  // public items: Array<string> = ['option 1', 'option 2', 'option 3'];
}
