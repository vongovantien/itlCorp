import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { SystemRepo } from './shared/repositories/system.repo';
import { HttpErrorResponse } from '@angular/common/http';

@Injectable()
export class MenuResolveGuard implements Resolve<any> {

    constructor(
        private _systemRepo: SystemRepo,
        private _router: Router
    ) {
    }
    resolve(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
    ): Observable<any> | Promise<any> | any {
        const pathArray = state.url.split("/");
        if (!!pathArray[3]) {
            return this._systemRepo.getUserPermissionByMenu(pathArray[3])
                .pipe()
                .subscribe(
                    (res: SystemInterface.IUserPermission) => {
                        console.log(res);
                        if (!!res && !res.access) {
                            this._router.navigate(["/login"]);
                        }
                    },
                    (err: HttpErrorResponse | any) => {
                        switch (err.status) {
                            // case 403:
                            //     // TODO redirect to forbidden page.
                            //     break;
                            default:
                                this._router.navigate(["/login"]);
                                break;
                        }
                    }
                );
        }

    }
}
