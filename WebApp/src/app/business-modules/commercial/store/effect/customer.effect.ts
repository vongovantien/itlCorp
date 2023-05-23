import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { Action } from '@ngrx/store';
import { CatalogueRepo } from "@repositories";
import { EMPTY, Observable } from "rxjs";
import { catchError, map, switchMap } from "rxjs/operators";
import { CustomerActionTypes, LoadListCustomerSuccess } from '../actions/customer.action';

@Injectable()
export class CustomerEffect {

    constructor(
        private actions$: Actions,
        private _catalogueRepo: CatalogueRepo,
    ) { }

    getListCustomerEffect$: Observable<Action> = createEffect(() => this.actions$
        .pipe(
            ofType(CustomerActionTypes.LOAD_LIST),
            switchMap(
                (param: CommonInterface.IParamPaging) => this._catalogueRepo.getListPartner(param.page, param.size, param.dataSearch)
                    .pipe(
                        catchError(() => EMPTY),
                        map((data: CommonInterface.IResponsePaging) => LoadListCustomerSuccess(data)),

                    )
            )
        ));
}
