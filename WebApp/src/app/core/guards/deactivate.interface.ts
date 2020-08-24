import { Observable } from "rxjs";
import { RouterStateSnapshot, ActivatedRouteSnapshot } from "@angular/router";

export interface ICanComponentDeactivate {
    canDeactivate: (route: ActivatedRouteSnapshot, currenctState: RouterStateSnapshot, nextState: RouterStateSnapshot) => Observable<boolean> | Promise<boolean> | boolean;

    getDeactivateNextState?(): RouterStateSnapshot;

    getDeactivateCurrenctState?(): RouterStateSnapshot;
}
