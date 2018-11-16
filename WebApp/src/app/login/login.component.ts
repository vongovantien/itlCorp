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
import { Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
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
    private oauthService: OAuthService) { }

  username: string = "";
  password: string = "";
  remember_me: boolean = false;

  ngOnInit() {

  }

  // async Login() {
  //   const response = await this.baseServices.postAsync(this.api_menu.System.User_Management.login, { username: this.username, password: this.password }, true, true);
  //   if (response.status) {
  //     localStorage.setItem(SystemConstants.LOGIN_STATUS, SystemConstants.LOGGED_IN);
  //     this.router.navigateByUrl('/home');

  //   }
  // }
   Login() {
    this.oauthService.fetchTokenUsingPasswordFlow(this.username, this.password).then((resp) => {
      console.log(resp)
      
      // Loading data about the user
      return this.oauthService.loadUserProfile();

    }).then(() => {

      // Using the loaded user data
      let claims = this.oauthService.getIdentityClaims();
      if (claims) console.log(claims);

    })
  }



  /**
   * ng2-select
   */
  public items: Array<string> = ['option 1', 'option 2', 'option 3'];
}
