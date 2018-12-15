import { HttpClient, HttpHeaders, HttpResponse, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, ErrorHandler, ViewContainerRef } from '@angular/core';
import { Http, Response, Headers } from '@angular/http';
import { Router } from '@angular/router';
import { Observable, throwError, Observer } from 'rxjs';
import { environment } from '../environments/environment.prod';
import { String } from 'typescript-string-operations';
import { SystemConstants } from 'src/constants/system.const';
import { promise } from 'protractor';
import { error } from 'util';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { OAuthService } from 'angular-oauth2-oidc';
import { resolve } from 'path';



@Injectable({
  providedIn: 'root'
})

export class BaseService implements ErrorHandler {

  private headers: HttpHeaders;
  protected baseUrl: string;
  protected showError: boolean;

  constructor(public _http: HttpClient, public _router: Router, private toastr: ToastrService, private spinnerService: Ng4LoadingSpinnerService,private oauthService: OAuthService) {
    this.headers = new HttpHeaders({
      'Content-Type': 'application/json',
    //  'Authorization': 'Bearer ' + sessionStorage.getItem("access_token") //this.oauthService.getAccessToken()
    });
    this.baseUrl = "";
    this.showError = true;
  }

  private formatURL(url: string): any {
    if (this.baseUrl === "") {
      return url;
    }
    return String.Format("{0}{1}/{2}", this.baseUrl, SystemConstants.CURRENT_LANGUAGE, url);
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
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization",token);
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
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization",token);
    if (display_spinner)
      this.spinnerService.show();

    try {
      const res = await this._http.get(url, { headers: this.headers }).toPromise();
      this.spinnerService.hide();
      return res;
    }
    catch (error) {
      this.spinnerService.hide();
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
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization",token);
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
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization",token);
    if (display_spinner)
      this.spinnerService.show();
    try {
      const res = await this._http.post(url, data, { headers: this.headers }).toPromise();     
      this.handleState(res, display_notify);
      this.spinnerService.hide();
      return res;      
    }
    catch (error) {      
      this.spinnerService.hide();
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
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization",token);  
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
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization",token);   
    if (display_spinner)
      this.spinnerService.show();
    try {
      const res = await this._http.put(url, data, { headers: this.headers }).toPromise();
      this.spinnerService.hide();
      this.handleState(res, display_notify);
      return res;
    }
    catch (error) {
      this.spinnerService.hide();
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
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization",token);
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
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization",token);
    if (display_spinner)
      this.spinnerService.show();
    try {
      const res = await this._http.delete(url, { headers: this.headers }).toPromise();
      this.spinnerService.hide();
      this.handleState(res, display_notify);
      return res;
    }
    catch (error) {
      this.spinnerService.hide();
      this.handleError(error);
      return error;
    }
  }
  
  downloadfile(url: string) {
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization",token);
    return this._http.get(url, {responseType: 'blob'});
  }

  uploadfile( url: any,files: any, name:string=null) {
    var token = 'Bearer ' + sessionStorage.getItem("access_token");
    if (files.length === 0)

    return;
    var formData = new FormData();
    for (let file of files)
      formData.append(name||file.name,file);
    let params = new HttpParams();
    const options = {
      params: params,
      reportProgress: true,
      headers:new HttpHeaders({
        'Authorization': token,
        'accept':'application/json'
      })
    };
    return this._http.post(url, formData, options);

   
  }
  /**
   * Handle state return from server and display toast notification 
   * @param response 
   * @param display_notify 
   */
  public handleState(response, display_notify = false) {
    if (response.status == true && display_notify == true) {
      this.toastr.success(response.message,"",{positionClass:'toast-bottom-right',closeButton:true,timeOut:3000});
    }
    if (response.status == false && display_notify == true) {
      this.toastr.error(response.message,"",{positionClass:'toast-bottom-right',closeButton:true,timeOut:3000});
    }
  }

  /**
   * Handle and display toast notificaton to show error returned from request 
   * @param error 
   */
  handleError(error: HttpErrorResponse) {
    this.toastr.error(error.error.message.toString(), "", { positionClass: 'toast-bottom-right', closeButton: true, timeOut: 3000 });
  }

  /**
   * Emit success toast notification at bottom-right conner
   * @param message 
   * @param title 
   */
  successToast(message:string,title=""){
    this.toastr.success(message,title,{positionClass:'toast-bottom-right',closeButton:true,timeOut:3000});
  }

  /**
   * Emit error toast notification at bottom-right conner
   * @param message 
   * @param title 
   */
  errorToast(message:string,title=""){
    this.toastr.error(message,title,{positionClass:'toast-bottom-right',closeButton:true,timeOut:3000});
  }

  /**
   * Emit warning toast notification at bottom-right conner
   * @param message 
   * @param title 
   */
  warningToast(message:string,title=""){
    this.toastr.warning(message,title,{positionClass:'toast-bottom-right',closeButton:true,timeOut:3000});
  }

  spinnerShow(){
    this.spinnerService.show();
  }

  spinnerHide(){
    this.spinnerService.hide();
  }


}
