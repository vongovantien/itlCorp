import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { Action } from '@ngrx/store';
import { CatalogueRepo } from "@repositories";
import { EMPTY, Observable } from "rxjs";
import { catchError, map, switchMap } from "rxjs/operators";
import { LoadListPartnerSuccess, PartnerDataActionTypes, getDetailPartnerSuccess } from '../actions/partner.action';

@Injectable()
export class PartnerEffect {

    constructor(
        private actions$: Actions,
        private _catalogueRepo: CatalogueRepo,
    ) { }

    getDetailPartnerEffect$: Observable<Action> = createEffect(() => this.actions$
        .pipe(
            ofType(PartnerDataActionTypes.GET_DETAIL),
            switchMap(
                (param: any) => this._catalogueRepo.getDetailPartner(param.payload)
                    .pipe(
                        catchError(() => EMPTY),
                        map((res: any) => {
                            return getDetailPartnerSuccess({ payload: res });
                        })
                    )
            )
        ));

    getListPartnerEffect$: Observable<Action> = createEffect(() => this.actions$
        .pipe(
            ofType(PartnerDataActionTypes.LOAD_LIST),
            switchMap(
                (param: CommonInterface.IParamPaging) => this._catalogueRepo.getListPartner(param.page, param.size, param.dataSearch)
                    .pipe(
                        catchError(() => EMPTY),
                        map((data: CommonInterface.IResponsePaging) => LoadListPartnerSuccess(data)),

                    )
            )
        ));
}
