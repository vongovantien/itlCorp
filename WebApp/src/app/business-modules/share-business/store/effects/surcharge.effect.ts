import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Action } from '@ngrx/store';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { map, switchMap, catchError, finalize } from 'rxjs/operators';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { SurchargeActionTypes, GetBuyingSurchargeSuccessAction, SurchargeAction, GetBuyingSurchargeFailAction, GetSellingSurchargeSuccessAction, GetSellingSurchargeFailAction, GetOBHSurchargeSuccessAction, GetOBHSurchargeFailAction, GetProfitSuccessAction, GetProfitFailFailAction } from '../actions';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable()
export class SurchargeEffects {

    constructor(
        private actions$: Actions,
        private _documentRepo: DocumentationRepo,
        private _spinnerService: NgxSpinnerService
    ) { }

    @Effect()
    getBuyingSurchargeByHBL$: Observable<Action> = this.actions$
        .pipe(
            ofType<SurchargeAction>(SurchargeActionTypes.GET_BUYING),
            map((payload: any) => payload.payload),
            switchMap(
                (param: any) => this._documentRepo.getSurchargeByHbl(param.type, param.hblId)
                    .pipe(
                        map((data: any) => new GetBuyingSurchargeSuccessAction(data)),
                        catchError(err => of(new GetBuyingSurchargeFailAction(err)))
                    )
            )
        );

    @Effect()
    getSellingSurchargeByHBL$: Observable<Action> = this.actions$
        .pipe(
            ofType<SurchargeAction>(SurchargeActionTypes.GET_SELLING),
            map((payload: any) => payload.payload),
            switchMap(
                (param: any) => this._documentRepo.getSurchargeByHbl(param.type, param.hblId)
                    .pipe(
                        map((data: any) => new GetSellingSurchargeSuccessAction(data)),
                        catchError(err => of(new GetSellingSurchargeFailAction(err)))
                    )
            )
        );

    @Effect()
    getOBHgSurchargeByHBL$: Observable<Action> = this.actions$
        .pipe(
            ofType<SurchargeAction>(SurchargeActionTypes.GET_OBH),
            map((payload: any) => payload.payload),
            switchMap(
                (param: any) => this._documentRepo.getSurchargeByHbl(param.type, param.hblId)
                    .pipe(
                        map((data: any) => new GetOBHSurchargeSuccessAction(data)),
                        catchError(err => of(new GetOBHSurchargeFailAction(err)))
                    )
            )
        );

    @Effect()
    getHBLProfit$: Observable<Action> = this.actions$
        .pipe(
            ofType<SurchargeAction>(SurchargeActionTypes.GET_PROFIT),
            map((payload: any) => payload.payload),
            switchMap(
                (hblid: string) => this._documentRepo.getHBLTotalProfit(hblid)
                    .pipe(
                        map((data: any) => new GetProfitSuccessAction(data)),
                        catchError(err => of(new GetProfitFailFailAction(err)))
                    )
            )
        );

}


