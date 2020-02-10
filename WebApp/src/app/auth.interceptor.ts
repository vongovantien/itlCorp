import { Injectable, InjectionToken, Inject } from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, TimeoutError } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { environment } from 'src/environments/environment';

export const DEFAULT_TIMEOUT = new InjectionToken<number>('defaultTimeout');

@Injectable({
    providedIn: 'root'
})
export class AuthInterceptor implements HttpInterceptor {
    constructor(
        private _toastService: ToastrService,
        @Inject(DEFAULT_TIMEOUT) protected defaultTimeout: number,
    ) {
    }

    authReq: HttpRequest<any> = null;
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

        const timeoutValue = req.headers.get('timeout') || this.defaultTimeout;
        let token;
        const authHeader = localStorage.getItem('access_token');
        if (!!authHeader) {
            token = `Bearer ${authHeader}`;
        }

        this.authReq = req.clone(req);

        if (environment.local) {
            if (!!this.authReq && (this.authReq.url.includes("44369") || this.authReq.url.includes("identityserver") || this.authReq.method !== 'GET')) {
                this.authReq = req.clone(Object.assign({}, req, { headers: req.headers.set('Authorization', token), url: req.url }));
            } else {
                this.authReq = req.clone(Object.assign({}, req, { headers: req.headers.delete('Authorization'), url: req.url }));
            }
        } else {
            this.authReq = req.clone(Object.assign({}, req, { headers: req.headers.set('Authorization', token), url: req.url }));
            // this.authReq = req;
        }
        return next.handle(this.authReq).pipe(
            timeout(+timeoutValue),
            catchError((error: HttpErrorResponse) => {

                switch (error.status) {
                    case 401:
                        // window.location.href = '#/login';
                        break;
                }
                let message: string = '';

                if (!!error.error) {
                    if (!!error.error.error) {

                        if (error.error.error === 'invalid_grant') {
                            message = error.error.error_description;
                        } else {
                            message = error.error.error.Message;
                        }
                    } else if (!!error.error.message) {
                        message = `${error.error.message}`;
                    } else {
                        message = `Something wrong!`;
                    }
                } else {
                    message = `Something wrong!`;
                }

                if (error instanceof TimeoutError) {
                    message = error.message;
                }
                // if (error.error instanceof ErrorEvent) {
                //     errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
                //     title = error.statusText;
                // } else if (error.error != null) {
                //     if (!!error.error.error) {
                //         errorMessage = `${error.error.error.Message}`;
                //     } else
                //         if (!!error.error.message) {
                //             errorMessage = `${error.error.message}`;
                //         } else if (error.error.error_description) {
                //             errorMessage = `${error.error.error_description}`;
                //         } else if (error.error.error.Message) {
                //             errorMessage = `${error.error.error.Message}`;
                //         } else {
                //             errorMessage = `Something wrong!`;
                //         }
                //     title = error.statusText;
                // } else {
                //     errorMessage = ` ${error.message}`;
                //     title = error.statusText;
                // }
                this._toastService.error(message);
                return throwError(error);
            })
        );
    }
}
