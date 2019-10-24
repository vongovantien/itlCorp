import { Injectable } from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { environment } from 'src/environments/environment';

@Injectable({
    providedIn: 'root'
})
export class AuthInterceptor implements HttpInterceptor {
    constructor(private _toastService: ToastrService) { }

    authReq: HttpRequest<any> = null;
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const authHeader = `Bearer ${localStorage.getItem('access_token')}`;
        this.authReq = req.clone(req);
        if (environment.local) {
            if (!!this.authReq && (this.authReq.url.includes("44369") || this.authReq.url.includes("identityserver") || this.authReq.method !== 'GET')) {
                this.authReq = req.clone(Object.assign({}, req, { headers: req.headers.set('Authorization', authHeader), url: req.url }));
            } else {
                this.authReq = req.clone(Object.assign({}, req, { headers: req.headers.delete('Authorization'), url: req.url }));
            }
        } else {
            this.authReq = req.clone(Object.assign({}, req, { headers: req.headers.set('Authorization', authHeader), url: req.url }));
        }
        return next.handle(this.authReq).pipe(
            catchError((error: HttpErrorResponse) => {
                switch (error.status) {
                    case 401:
                        // window.location.href = '#/login';
                        break;
                }
                let errorMessage = '';
                let title = '';
                if (error.error instanceof ErrorEvent) {
                    errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
                    title = error.statusText;
                } else if (error.error != null) {
                    if (!!error.error.message) {
                        errorMessage = `Error: ${error.error.message}`;
                    } else if (error.error.error_description) {
                        errorMessage = `Error: ${error.error.error_description}`;
                    } else {
                        errorMessage = `Error: ${error.error.error}`;
                    }
                    title = error.statusText;
                } else {
                    errorMessage = `Error: ${error.message}`;
                    title = error.statusText;
                }
                this._toastService.error(errorMessage);
                return throwError(error);
            })
        );
    }
}
