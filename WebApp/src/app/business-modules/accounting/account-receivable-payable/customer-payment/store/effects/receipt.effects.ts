import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { AccountingRepo } from "@repositories";
import { ReceiptActionTypes, GetInvoiceListSuccess, GetInvoiceListFail, LoadListCustomerPaymentSuccess } from "../actions";
import { mergeMap, map, catchError, switchMap } from "rxjs/operators";
import { LoadListAccountingMngtSuccess } from "src/app/business-modules/accounting/accounting-management/store";
import { EMPTY, Observable, of } from "rxjs";
import { Action } from "@ngrx/store";

@Injectable()
export class ReceiptEffects {

    constructor(
        private actions$: Actions,
        private _accountingRepo: AccountingRepo,
    ) { }

    // GetInvoice$ = this.actions$.pipe(
    //     ofType(ReceiptActionTypes.GET_INVOICE),
    //     mergeMap(
    //         (param) => this._accountingRepo.getInvoiceForReceipt(param)
    //             .pipe(
    //                 map((data: CommonInterface.IResult) => {
    //                     if (data.status) {
    //                         return GetInvoiceListSuccess(data.data);
    //                     }
    //                 }), catchError(() => of(GetInvoiceListFail())),
    //             )
    //     )
    // );
    getListCustomerPaymentEffect$: Observable<Action> = createEffect(() => this.actions$
    .pipe(
        ofType(ReceiptActionTypes.LOAD_LIST),
        switchMap(
            (param: CommonInterface.IParamPaging) => this._accountingRepo.getListCustomerPayment(param.page, param.size, param.dataSearch)
                .pipe(
                    catchError(() => EMPTY),
                    map((data: CommonInterface.IResponsePaging) => LoadListCustomerPaymentSuccess(data)),

                )
        )
    ));
}
