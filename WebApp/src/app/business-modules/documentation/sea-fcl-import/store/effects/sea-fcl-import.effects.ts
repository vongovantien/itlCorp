import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Action } from '@ngrx/store';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { SeaFCLImportActionTypes, SeaFCLImportGetDetailSuccessAction, SeaFCLImportGetDetailFailAction, SeaFCLImportActions, ContainerActionTypes, ContainerAction, GetContainerSuccessAction, GetContainerFailAction, SeaFCLImportUpdateSuccessAction, SeaFCLImportUpdateFailAction, HouseBillActionTypes, GetDetailHBLSuccessAction, GetDetailHBLFailAction, SeaFCLImportGetProfitSuccessAction, SeaFCLImportGetProfitFailFailAction } from '../actions';
import { map, switchMap, catchError } from 'rxjs/operators';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { IProfit } from '../reducers';

@Injectable()
export class SeaFCLImportEffects {

    constructor(
        private actions$: Actions,
        private _documentRepo: DocumentationRepo
    ) { }

    @Effect()
    getDetailSeaFCLImportEffect$: Observable<Action> = this.actions$.
        pipe(
            ofType<SeaFCLImportActions>(SeaFCLImportActionTypes.GET_DETAIL),
            map((payload: any) => payload.payload),
            switchMap(
                (id: string) => this._documentRepo.getDetailTransaction(id)
                    .pipe(
                        map((data: any) => new SeaFCLImportGetDetailSuccessAction(data)),
                        catchError(err => of(new SeaFCLImportGetDetailFailAction(err)))
                    )
            )
        );

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
                    )
            )
        );

    @Effect()
    updateCSTransaction$: Observable<Action> = this.actions$
        .pipe(
            ofType<ContainerAction>(SeaFCLImportActionTypes.UPDATE),
            map((payload: any) => payload.payload),
            switchMap(
                (param: any) => this._documentRepo.updateCSTransaction(param)
                    .pipe(
                        map((data: CommonInterface.IResult) => new SeaFCLImportUpdateSuccessAction(data.data)),
                        catchError(err => of(new SeaFCLImportUpdateFailAction(err)))
                    )
            )
        );

    @Effect()
    getDetailHBL$: Observable<Action> = this.actions$
        .pipe(
            ofType<ContainerAction>(HouseBillActionTypes.GET_DETAIL),
            map((payload: any) => payload.payload),
            switchMap(
                (id: string) => this._documentRepo.getDetailHbl(id)
                    .pipe(
                        map((data: CommonInterface.IResult) => new GetDetailHBLSuccessAction(data)),
                        catchError(err => of(new GetDetailHBLFailAction(err)))
                    )
            )
        );

    @Effect()
    getTotalProfitShipmentL$: Observable<Action> = this.actions$
        .pipe(
            ofType<ContainerAction>(SeaFCLImportActionTypes.GET_PROFIT),
            map((payload: any) => payload.payload),
            switchMap(
                (jobId: string) => this._documentRepo.getShipmentTotalProfit(jobId)
                    .pipe(
                        map((data: IProfit[]) => new SeaFCLImportGetProfitSuccessAction(data)),
                        catchError(err => of(new SeaFCLImportGetProfitFailFailAction(err)))
                    )
            )
        );


}


