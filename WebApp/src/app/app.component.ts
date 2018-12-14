import { Component, OnInit } from '@angular/core';
import { SystemConstants } from '../constants/system.const';
import { OAuthService } from 'angular-oauth2-oidc';
import { CookieService } from 'ngx-cookie-service';
import { Router } from '@angular/router';
import { BaseService } from 'src/services-base/base.service';
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

  ngOnInit(): void {
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) == null) {
      localStorage.setItem("CURRENT_LANGUAGE", SystemConstants.DEFAULT_LANGUAGE);
    }
    if (localStorage.getItem(SystemConstants.CURRENT_VERSION) == null) {
      localStorage.setItem("CURRENT_VERSION", "1");
    }
    var current_client_lang = localStorage.getItem(SystemConstants.CURRENT_CLIENT_LANGUAGE);
 
    if (current_client_lang === null) {
      localStorage.setItem(SystemConstants.CURRENT_CLIENT_LANGUAGE, "en");
    }

    /**
     * Check login status 
     */
    this.checkLoginSession();
  }

  constructor(
    private oauthService: OAuthService,
    private router: Router,
    private cookieService: CookieService,
    private baseService:BaseService) {
  }

  checkLoginSession(){
    if(this.oauthService.getAccessToken()==null){
      if(this.cookieService.get("login_status")==="LOGGED_IN"){
        this.baseService.warningToast("Login again to continue !");
      }
      this.cookieService.delete("login_status","/",window.location.hostname);
      this.router.navigateByUrl('/login');      
    }else{
      this.router.navigateByUrl('/home');
    }
  }

}
