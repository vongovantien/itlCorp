import { Injectable } from "@angular/core";
import { Action } from '@ngrx/store';
import { Actions, Effect, ofType } from "@ngrx/effects";

import { DocumentationRepo } from "src/app/shared/repositories";

import { Observable, of } from "rxjs";

import { switchMap, catchError, map } from "rxjs/operators";
import { TransactionActionTypes, TransactionGetProfitSuccessAction, TransactionActions, TransactionGetProfitFailFailAction, ContainerAction, ContainerActionTypes, GetContainerSuccessAction, GetContainerFailAction } from "../actions";
import { ITransactionProfit } from "../reducers";

@Injectable()
export class ShareBussinessEffects {

    constructor(
        private actions$: Actions,
        private _documentRepo: DocumentationRepo
    ) { }

    @Effect()
    getListContainerEffect$: Observable<Action> = this.actions$
        .pipe(
            ofType<ContainerAction>(ContainerActionTypes.GET_CONTAINER),
            map((payload: any) => payload.payload), // jobId
            switchMap(
                (param: any) => this._documentRepo.getListContainersOfJob(param)
                    .pipe(
                        map((data: any) => new GetContainerSuccessAction(data)),
                        catchError(err => of(new GetContainerFailAction(err)))
                    ))
        );

    @Effect()
    getTotalProfitShipmentL$: Observable<Action> = this.actions$
        .pipe(
            ofType<TransactionActions>(TransactionActionTypes.GET_PROFIT),
            map((payload: any) => payload.payload),
            switchMap(
                (jobId: string) => this._documentRepo.getShipmentTotalProfit(jobId)
                    .pipe(
                        map((data: ITransactionProfit[]) => new TransactionGetProfitSuccessAction(data)),
                        catchError(err => of(new TransactionGetProfitFailFailAction(err)))
                    ))
        );

}
