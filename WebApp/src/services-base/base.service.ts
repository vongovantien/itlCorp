import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Injectable, ErrorHandler } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NgxSpinnerService } from 'ngx-spinner';
import { OAuthService } from 'angular-oauth2-oidc';
import { CookieService } from 'ngx-cookie-service';


@Injectable({
  providedIn: 'root'
})

export class BaseService implements ErrorHandler {

  private headers: HttpHeaders;
  protected baseUrl: string;
  protected showError: boolean;

  constructor(private _http: HttpClient,
    private spinnerService: NgxSpinnerService,
    private toastr: ToastrService,
    private oauthService: OAuthService,
    private router: Router,
    private cookieService: CookieService,){

    this.headers = new HttpHeaders({
      'Content-Type': 'application/json',
    });
    this.baseUrl = "";
    this.showError = true; 
  }

  public setBaseUrl(url) {
    this.baseUrl = url;
  }

  /**
   * GET request without handle error and state, 
   * you must handle error or state by yourself
   * @param url 
   */
  public get(url: string) {    
    this.checkLoginSession(); 
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization", token);
    return this._http.get(url, { headers: this.headers });
  }

  /**
   * GET request within handle error and state, if display_notify is true
   * application will display toast notification with message get from handle state,
   * @param url 
   * @param display_error
   * @param display_spinner 
   */
  public async getAsync(url: string, display_error = false, display_spinner = false): Promise<any> {
    this.checkLoginSession();   
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization", token);
    if (display_spinner)
      this.spinnerShow()

    try {
      const res = await this._http.get(url, { headers: this.headers }).toPromise();
      this.spinnerHide();
      return res;
    }
    catch (error) {
      this.spinnerHide();
      if (display_error) {
        this.handleError(error);
      }
      return error;
    }
  }

  /**
   * POST request without handle error and state, 
   * you must handle error or state by yourself
   * @param url 
   * @param data 
   */
  public post(url: string, data?: any) {
    this.checkLoginSession();   
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization", token);
    return this._http.post(url, data, { headers: this.headers });
  }

  /**
   * POST request within handle error and state, if display_notify is true
   * application will display toast notification with message get from handle state,
   * @param url 
   * @param data 
   * @param display_notify 
   * @param display_spinner 
   */
  public async postAsync(url: string, data?: any, display_notify = true, display_spinner = true): Promise<any> {
    this.checkLoginSession();       
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization", token);
    if (display_spinner)
      this.spinnerShow();
    try {
      const res = await this._http.post(url, data, { headers: this.headers }).toPromise();
      this.handleState(res, display_notify);
      this.spinnerHide();
      return res;
    }
    catch (error) {
      this.spinnerHide();
      this.handleError(error);
      return error;
    }
  }

  /**
   * PUT request without handle error and state, 
   * you must handle error or state by yourself
   * @param url 
   * @param data 
   */
  public put(url: string, data?: any) {
    this.checkLoginSession();   
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization", token);
    return this._http.put(url, data, { headers: this.headers });
  }

  /**
   * PUT request within handle error and state, if display_notify is true
   * application will display toast notification with message get from handle state,
   * @param url 
   * @param data 
   * @param display_notify 
   * @param display_spinner 
   */
  public async putAsync(url: string, data?: any, display_notify = true, display_spinner = true): Promise<any> {
    this.checkLoginSession();   
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization", token);
    if (display_spinner)
      this.spinnerShow();
    try {
      const res = await this._http.put(url, data, { headers: this.headers }).toPromise();
      this.spinnerHide();
      this.handleState(res, display_notify);
      return res;
    }
    catch (error) {
      this.spinnerHide();
      this.handleError(error);
      return error;
    }
  }

  /**
   * DELETE request without handle error and state, 
   * you must handle error or state by yourself
   * @param url 
   */
  public delete(url: string) {
    this.checkLoginSession();   
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization", token);
    return this._http.delete(url, { headers: this.headers });
  }

  /**
   * PUT request within handle error and state, if display_notify is true
   * application will display toast notification with message get from handle state,
   * @param url 
   * @param display_notify
   * @param display_spinner 
   */
  public async deleteAsync(url: string, display_notify = true, display_spinner = true): Promise<any> {
    this.checkLoginSession();   
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization", token);
    if (display_spinner)
      this.spinnerShow();
    try {
      const res = await this._http.delete(url, { headers: this.headers }).toPromise();
      this.spinnerHide();
      this.handleState(res, display_notify);
      return res;
    }
    catch (error) {
      this.spinnerHide();
      this.handleError(error);
      return error;
    }
  }


  /**
   * Handle state return from server and display toast notification 
   * @param response 
   * @param display_notify 
   */
  public handleState(response, display_notify = false) {
    if (response.status == true && display_notify == true) {
      this.successToast(response.message);
    }
    if (response.status == false && display_notify == true) {
      this.errorToast(response.message);
    }
  }

  /**
   * Handle and display toast notificaton to show error returned from request 
   * @param error 
   */
  handleError(error: HttpErrorResponse) {
    this.errorToast(error.error.message.toString());
  }

  /**
   * Emit success toast notification at bottom-right conner
   * @param message 
   * @param title 
   */
  successToast(message: string, title = "") {
    this.toastr.success(message, title, { positionClass: 'toast-bottom-right', closeButton: true, timeOut: 4000 });
  }

  /**
   * Emit error toast notification at bottom-right conner
   * @param message 
   * @param title 
   */
  errorToast(message: string, title = "") {
    this.toastr.error(message, title, { positionClass: 'toast-bottom-right', closeButton: true, timeOut: 4000 });
  }

  /**
   * Emit warning toast notification at bottom-right conner
   * @param message 
   * @param title 
   */
  warningToast(message: string, title = "") {
    this.toastr.warning(message, title, { positionClass: 'toast-bottom-right', closeButton: true, timeOut: 4000 });
  }

  spinnerShow() {
    this.spinnerService.show();
  }

  spinnerHide() {
    this.spinnerService.hide();
  }

  checkLoginSession(){
    if(this.oauthService.getAccessToken()==null){
      if(this.cookieService.get("login_status")==="LOGGED_IN"){
        this.warningToast("Login again to continue !");
      }
      this.cookieService.delete("login_status","/",window.location.hostname);
      this.router.navigateByUrl('/login');      
    }
  }


}
