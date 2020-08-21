import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Action } from '@ngrx/store';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { OPSActions, OPSActionTypes, OPSTransactionGetDetailSuccessAction, OPSTransactionGetDetailFailAction, OPSTransactionLoadListSuccessAction, OPSTransactionLoadListFailAction } from '../actions/operation.action';
import { map, mergeMap, catchError } from 'rxjs/operators';
import { DocumentationRepo } from '@repositories';
import { OpsTransaction } from '@models';

@Injectable()
export class OperationEffects {

    @Effect()
    getDetailShipmentOperationEffect$: Observable<Action> = this.actions$.
        pipe(
            ofType<OPSActions>(OPSActionTypes.GET_DETAIL),
            map((payload: any) => payload.payload),
            mergeMap(
                (id: string) => this._documentRepo.getDetailShipment(id)
                    .pipe(
                        map((data: any) => new OPSTransactionGetDetailSuccessAction(new OpsTransaction(data))),
                        catchError(err => of(new OPSTransactionGetDetailFailAction(err)))
                    )
            )
        );

    @Effect()
    getListOPSShipmentEffect$: Observable<Action> = this.actions$.
        pipe(
            ofType<OPSActions>(OPSActionTypes.LOAD_LIST),
            map((payload: any) => payload.payload),
            mergeMap(
                (param: any) => this._documentRepo.getListShipment(param.page, param.size, param.dataSearch)
                    .pipe(
                        map((data: any) => new OPSTransactionLoadListSuccessAction(data)),
                        catchError(err => of(new OPSTransactionLoadListFailAction(err)))
                    )
            )
        );
    constructor(
        private actions$: Actions,
        private _documentRepo: DocumentationRepo
    ) { }



}