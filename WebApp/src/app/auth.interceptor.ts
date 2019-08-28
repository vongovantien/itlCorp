import { Injectable } from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

@Injectable({
    providedIn: 'root'
})
export class AuthInterceptor implements HttpInterceptor {
    constructor(private _toastService: ToastrService){}
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const authHeader = `Bearer ${localStorage.getItem('access_token')}`;
        const authReq = req.clone({ headers: req.headers.set('Authorization', authHeader), url: req.url });

        return next.handle(authReq).pipe(
            catchError((error: HttpErrorResponse) => {
                let errorMessage = '';
                let title = '';
                if (error.error instanceof ErrorEvent) {
                    errorMessage = `Error: ${error.error.message}`;
                    title = error.statusText;
                } else {
                    errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
                    title = error.statusText;
                }
                this._toastService.error(errorMessage, title);
                return throwError(errorMessage);
            })
        );
    }
}
