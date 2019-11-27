import { Injectable } from "@angular/core";
import { Action } from '@ngrx/store';
import { Actions, Effect, ofType } from "@ngrx/effects";

import { DocumentationRepo } from "src/app/shared/repositories";

import { Observable, of } from "rxjs";

import { switchMap, catchError, map } from "rxjs/operators";
import { TransactionActionTypes, TransactionGetProfitSuccessAction, TransactionActions, TransactionGetProfitFailFailAction, ContainerAction, ContainerActionTypes, GetContainerSuccessAction, GetContainerFailAction, HBLActions, HBLActionTypes, GetDetailHBLSuccessAction, GetDetailHBLFailAction, GetProfitHBLSuccessAction, GetProfitHBLAction, GetContainersHBLSuccessAction, GetContainersHBLFailAction } from "../actions";
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

    @Effect()
    getDetailHBL$: Observable<Action> = this.actions$
        .pipe(
            ofType<HBLActions>(HBLActionTypes.GET_DETAIL),
            map((payload: any) => payload.payload),
            switchMap(
                (id: string) => this._documentRepo.getDetailHbl(id)
                    .pipe(
                        map((data: any) => new GetDetailHBLSuccessAction(data)),
                        catchError(err => of(new GetDetailHBLFailAction(err)))
                    )
            )
        );

    @Effect()
    getHBLProfit$: Observable<Action> = this.actions$
        .pipe(
            ofType<HBLActions>(HBLActionTypes.GET_PROFIT),
            map((payload: any) => payload.payload),
            switchMap(
                (hblid: string) => this._documentRepo.getHBLTotalProfit(hblid)
                    .pipe(
                        map((data: any) => new GetProfitHBLSuccessAction(data)),
                        catchError(err => of(new GetProfitHBLAction(err)))
                    )
            )
        );

    @Effect()
    getListHBLContainerEffect$: Observable<Action> = this.actions$
        .pipe(
            ofType<HBLActions>(HBLActionTypes.GET_CONTAINERS),
            map((payload: any) => payload.payload), // jobId
            switchMap(
                (param: any) => this._documentRepo.getListContainersOfJob(param)
                    .pipe(
                        map((data: any) => new GetContainersHBLSuccessAction(data)),
                        catchError(err => of(new GetContainersHBLFailAction(err)))
                    ))
        );

}
