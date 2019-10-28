import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { map, catchError, switchMap } from 'rxjs/operators';
import { SystemRepo } from '../../../../../shared/repositories';
import { LoadCompanyFailureAction, LoadCompanySuccessAction, CompanyAction, CompanyActionTypes } from '../actions';

@Injectable()
export class CompanyEffects {

    constructor(
        private actions$: Actions,
        private _systemRepo: SystemRepo
    ) { }

    @Effect() loadCompany = this.actions$
        .pipe(
            ofType<any>(CompanyActionTypes.LOAD_COMPANY),
            map((action: CompanyAction) => action.payload),
            switchMap(
                (payload: any) => this._systemRepo.getCompany(1, 10, payload)
                    .pipe(
                        map((data: any) => new LoadCompanySuccessAction(data)),
                        catchError(err => of(new LoadCompanyFailureAction(err)))
                    )
            )
        )
}
