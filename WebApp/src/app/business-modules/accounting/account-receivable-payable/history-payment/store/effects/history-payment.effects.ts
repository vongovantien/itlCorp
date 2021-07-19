import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { AccountingRepo } from "@repositories";
import {HistoryPaymentActionTypes,LoadListHistoryPaymentSuccess } from "../actions";
import { mergeMap, map, catchError, switchMap } from "rxjs/operators";
import { LoadListAccountingMngtSuccess } from "src/app/business-modules/accounting/accounting-management/store";
import { EMPTY, Observable, of } from "rxjs";
import { Action } from "@ngrx/store";

@Injectable()
export class HistoryPaymentEffects {

    constructor(
        private actions$: Actions,
        private _accountingRepo: AccountingRepo,

    ) { }

    getListHistoryPaymentEffect$: Observable<Action> = createEffect(() => this.actions$
    .pipe(
        ofType(HistoryPaymentActionTypes.LOAD_LIST),
        switchMap(
            (param: CommonInterface.IParamPaging) => this._accountingRepo.paymentPaging(param.page, param.size, param.dataSearch)
                .pipe(
                    catchError(() => EMPTY),
                    map((data: CommonInterface.IResponsePaging) => LoadListHistoryPaymentSuccess(data)),

                )
        )
    ));
}
