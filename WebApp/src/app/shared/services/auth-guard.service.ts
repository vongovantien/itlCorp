import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { BaseService } from './base.service';
import { Location } from '@angular/common';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class AuthGuardService implements CanActivate {

    canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {
        if (!this._baseService.checkLoginSession()) {
            this._router.navigate(['/login', { isEndSession: true, url: state.url }]);
            return false;
        } else {
            return true;
        }
    }

    constructor(
        private _baseService: BaseService,
        private _router: Router,
    ) { }

}





