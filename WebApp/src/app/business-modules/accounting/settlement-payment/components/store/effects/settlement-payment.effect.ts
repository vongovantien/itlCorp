import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { Action } from '@ngrx/store';
import { AccountingRepo, SystemFileManageRepo } from "@repositories";
import { EMPTY, Observable } from "rxjs";
import { catchError, map, switchMap } from "rxjs/operators";
import { SettlementPaymentActionTypes } from "../actions";
import { IEDocSettleSearch, LoadListEDocSettleSuccess, LoadListSettlePaymentSuccess } from './../actions/settlement-payment.action';

@Injectable()
export class SettlePaymentEffect {

    constructor(
        private actions$: Actions,
        private _accountingRepo: AccountingRepo,
        private _systemFileRepo: SystemFileManageRepo
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

    getListSettleEdocEffect$: Observable<Action> = createEffect(() => this.actions$
        .pipe(
            ofType(SettlementPaymentActionTypes.LOAD_LIST_EDOC),
            switchMap(
                (param: IEDocSettleSearch) => this._systemFileRepo.getEDocByAccountant(param.billingId, param.transactionType)
                    .pipe(
                        catchError(() => EMPTY),
                        map((data: any) => LoadListEDocSettleSuccess(data)),
                    )
            )
        ));


    // getDetailSettlePaymentEffect$: Observable<Action> = createEffect(() => this.actions$
    //     .pipe(
    //         ofType(SettlementPaymentActionTypes.GET_DETAIL),
    //         switchMap(
    //             (p: { id: string }) => this._accountingRepo.getDetailSettlementPayment(p.id)
    //                 .pipe(
    //                     catchError(() => EMPTY),
    //                     map((data) => LoadDetailSettlePaymentSuccess(data))
    //                 )
    //         )
    //     )
    // );
}
