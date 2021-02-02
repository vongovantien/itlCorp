import { LoadListSettlePaymentSuccess } from './../actions/settlement-payment.action';
import { Injectable } from "@angular/core";
import { createEffect, ofType, Actions } from "@ngrx/effects";
import { switchMap, map, catchError } from "rxjs/operators";
import { Observable, EMPTY } from "rxjs";
import { AccountingRepo } from "@repositories";
import { Action } from '@ngrx/store';
import { SettlementPaymentActionTypes } from "../actions";

@Injectable()
export class SettlePaymentEffect {

    constructor(
        private actions$: Actions,
        private _accountingRepo: AccountingRepo,
    ) { }

    getListSettlePaymentEffect$: Observable<Action> = createEffect(() => this.actions$
        .pipe(
            ofType(SettlementPaymentActionTypes.LOAD_LIST),
            switchMap(
                (param: CommonInterface.IParamPaging) => this._accountingRepo.getListSettlementPayment(param.page, param.size, param.dataSearch)
                    .pipe(
                        catchError(() => EMPTY),
                        map((data: CommonInterface.IResponsePaging) => LoadListSettlePaymentSuccess(data)),
                    )
            )
        ));
}