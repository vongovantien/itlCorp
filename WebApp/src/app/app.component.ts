import { Component, OnInit } from '@angular/core';
import { SystemConstants } from '../constants/system.const';
import { OAuthService } from 'angular-oauth2-oidc';
import { JwksValidationHandler } from 'angular-oauth2-oidc';
import { authConfig } from './shared/authenticate/authConfig';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

  default_current_client_lang = "en";
  ngOnInit(): void {
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) == null) {
      localStorage.setItem("CURRENT_LANGUAGE", SystemConstants.DEFAULT_LANGUAGE);
    }
    if (localStorage.getItem(SystemConstants.CURRENT_VERSION) == null) {
      localStorage.setItem("CURRENT_VERSION", "1");
    }
    const current_client_lang = localStorage.getItem(SystemConstants.CURRENT_CLIENT_LANGUAGE);
    this.default_current_client_lang = current_client_lang;
    if(current_client_lang===null){
      localStorage.setItem(SystemConstants.CURRENT_CLIENT_LANGUAGE, "en");
    }
  }

  constructor(private oauthService: OAuthService) {
    this.configureWithNewConfigApi();
  }

  private configureWithNewConfigApi() {
    this.oauthService.configure(authConfig);
    this.oauthService.tokenValidationHandler = new JwksValidationHandler();
    this.oauthService.loadDiscoveryDocumentAndTryLogin();
  }
  title = 'app';
}
