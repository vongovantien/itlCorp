import { Injectable } from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor() { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const authHeader = `Bearer ${localStorage.getItem('access_token')}`;
    const authReq = req.clone({ headers: req.headers.set('Authorization', authHeader), url: req.url });

    
    return next.handle(authReq);
  }
}
