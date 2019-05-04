import { HttpClient, HttpHeaders, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, ErrorHandler } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NgxSpinnerService } from 'ngx-spinner';
import { language } from 'src/languages/language.en';

@Injectable({
  providedIn: 'root'
})

export class BaseService implements ErrorHandler {

  private headers: HttpHeaders;
  protected baseUrl: string;
  protected showError: boolean;

  public LANG = language;

  constructor(private _http: HttpClient,
    private spinnerService: NgxSpinnerService,
    private toastr: ToastrService,
    private router: Router, ) {

    this.headers = new HttpHeaders({
      'Content-Type': 'application/json',
    });
    this.baseUrl = "";
    this.showError = true;
  }

  /**
   * GET request without handle error and state, 
   * you must handle error or state by yourself
   * @param url 
   */
  public get(url: string) {

    var token = 'Bearer ' + localStorage.getItem("access_token");
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
    // this.checkLoginSession();
    var token = 'Bearer ' + localStorage.getItem("access_token");
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
      return false;
    }
  }

  /**
   * POST request without handle error and state, 
   * you must handle error or state by yourself
   * @param url 
   * @param data 
   */
  public post(url: string, data?: any) {

    var token = 'Bearer ' + localStorage.getItem("access_token");
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
    var token = 'Bearer ' + localStorage.getItem("access_token");
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
      return false;
    }
  }

  /**
   * PUT request without handle error and state, 
   * you must handle error or state by yourself
   * @param url 
   * @param data 
   */
  public put(url: string, data?: any) {
    var token = 'Bearer ' + localStorage.getItem("access_token");
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
    var token = 'Bearer ' + localStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization", token);
    if (display_spinner)
      this.spinnerShow();
    try {
      const res = await this._http.put(url, data, { headers: this.headers }).toPromise();
      console.log(res)
      this.spinnerHide();
      this.handleState(res, display_notify);
      return res;
    }
    catch (error) {
      this.spinnerHide();
      this.handleError(error);
      return false;
    }
  }

  /**
   * DELETE request without handle error and state, 
   * you must handle error or state by yourself
   * @param url 
   */
  public delete(url: string) {
    var token = 'Bearer ' + localStorage.getItem("access_token");
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
    var token = 'Bearer ' + localStorage.getItem("access_token");
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
      return false;
    }
  }
  public async previewfile(url: string, data?: any): Promise<any> {
    var token = 'Bearer ' + localStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization", token);
    this.spinnerShow();
    try {
      this.spinnerHide();
      const res = await this._http.post(url, data, { responseType: "blob" }).toPromise();
      return res;
    } catch (error) {
      console.log({ DOWNLOAD_ERROR_LOG: error });
      this.errorToast(this.LANG.NOTIFI_MESS.FILE_NOT_FOUND, this.LANG.NOTIFI_MESS.DOWNLOAD_ERR);
    }
  }
  public async downloadfile(url: string, saveAsFileName: string): Promise<any> {
    var token = 'Bearer ' + localStorage.getItem("access_token");
    this.headers = this.headers.set("Authorization", token);
    this.spinnerShow();
    try {
      this.spinnerHide();
      const res = await this._http.get(url, { responseType: 'blob' }).toPromise();
      saveAs(res, saveAsFileName);
    } catch (error) {
      console.log({ DOWNLOAD_ERROR_LOG: error });
      this.errorToast(this.LANG.NOTIFI_MESS.FILE_NOT_FOUND, this.LANG.NOTIFI_MESS.DOWNLOAD_ERR);
    }
  }

  uploadfile(url: any, files: any, name: string = null) {
    var token = 'Bearer ' + localStorage.getItem("access_token");
    if (files.length === 0)

      return;
    var formData = new FormData();
    for (let file of files)
      formData.append(name || file.name, file);
    let params = new HttpParams();
    const options = {
      params: params,
      reportProgress: true,
      headers: new HttpHeaders({
        'Authorization': token,
        'accept': 'application/json'
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
      this.successToast(response.message);
    }
  }

  /**
   * Handle and display toast notificaton to show error returned from request 
   * @param error 
   */
  handleError(error: HttpErrorResponse) {
    if (error.status === 400) {
      this.errorToast(error.error.message, this.LANG.NOTIFI_MESS.CLIENT_ERR_TITLE);
    }
    if (error.status === 500) {
      this.errorToast(error.error.error.Message, this.LANG.NOTIFI_MESS.SERVER_ERR_TITLE);
    }
    if (error.status === 0) {
      this.errorToast(this.LANG.NOTIFI_MESS.CHECK_CONNECT, this.LANG.NOTIFI_MESS.UNKNOW_ERR);
    }
    if (error.status === 401) {
      localStorage.clear();
      this.router.navigateByUrl('/login');
      this.reloadPage();
      setTimeout(() => {
        this.warningToast(this.LANG.NOTIFI_MESS.EXPIRED_SESSION_MESS, this.LANG.NOTIFI_MESS.EXPIRED_SESSION_TITLE);
      }, 400);
    }

  }

  public logOut() {
    localStorage.clear();
   
    setTimeout(() => {
      this.warningToast(this.LANG.NOTIFI_MESS.EXPIRED_SESSION_MESS, this.LANG.NOTIFI_MESS.EXPIRED_SESSION_TITLE);
    }, 1000);

    this.reloadPage();
  }

  /**
   * Emit success toast notification at bottom-right conner
   * @param message 
   * @param title 
   */
  successToast(message: string, title = "") {
    this.toastr.success(message, title, { positionClass: 'toast-bottom-right', closeButton: true, timeOut: 5000 });
  }

  /**
   * Emit error toast notification at bottom-right conner
   * @param message 
   * @param title 
   */
  errorToast(message: string, title = "") {
    this.toastr.error(message, title, { positionClass: 'toast-bottom-right', closeButton: true, timeOut: 5000 });
  }

  /**
   * Emit warning toast notification at bottom-right conner
   * @param message 
   * @param title 
   */
  warningToast(message: string, title = "") {
    this.toastr.warning(message, title, { positionClass: 'toast-bottom-right', closeButton: true, timeOut: 5000 });
  }

  spinnerShow() {
    this.spinnerService.show();
  }

  spinnerHide() {
    this.spinnerService.hide();
  }

  checkLoginSession(display_warning = true): boolean {
    if (this.hasValidAccessToken() == false) {
      if (display_warning) {
        this.warningToast(this.LANG.NOTIFI_MESS.EXPIRED_SESSION_MESS, this.LANG.NOTIFI_MESS.EXPIRED_SESSION_TITLE);
      }     
      this.router.navigateByUrl('/login');
      localStorage.clear();
      // this.reloadPage();
      return false;
    } else {
      return true;
    }
  }



  reloadPage() {
    if (window.location.hostname === 'localhost') {
      window.location.href = window.location.protocol + "//" + window.location.hostname + ":" + window.location.port;
    } else {
      window.location.href = window.location.protocol + "//" + window.location.hostname;
    }
  }



  private hasValidAccessToken() {
    if (this.getAccessToken()) {
      var expiresAt = localStorage.getItem('expires_at');
      var now = new Date();
      if (expiresAt && parseInt(expiresAt, 10) < now.getTime()) {
        localStorage.clear();
        return false;
      }
      return true;
    }
    localStorage.clear();
    return false;
  };

  /**
   * Return true if access token will be expire time after 3 minutes
   */
  public remainingExpireTimeToken() :number{
    if (this.getAccessToken()) {
      var expiresAt = localStorage.getItem('expires_at');
      var expTime = +new Date(parseInt(expiresAt,10));
      var nowTime = +new Date();
      const remainingMinutes = new Date(expTime - nowTime).getMinutes();

      return remainingMinutes
    }
    return -1;
  };


  public getAccessToken() {
    return localStorage.getItem('access_token');
  }

}
