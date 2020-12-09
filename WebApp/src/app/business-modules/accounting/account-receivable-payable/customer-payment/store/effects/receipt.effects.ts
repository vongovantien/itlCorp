import { Injectable } from "@angular/core";
import { Actions, ofType } from "@ngrx/effects";
import { AccountingRepo } from "@repositories";
import { ReceiptActionTypes, GetInvoiceListSuccess, GetInvoiceListFail } from "../actions";
import { mergeMap, map, catchError } from "rxjs/operators";
import { LoadListAccountingMngtSuccess } from "src/app/business-modules/accounting/accounting-management/store";
import { EMPTY, of } from "rxjs";

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
}
