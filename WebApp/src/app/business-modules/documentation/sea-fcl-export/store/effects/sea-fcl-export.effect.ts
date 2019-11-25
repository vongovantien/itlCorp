import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Action } from '@ngrx/store';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { map, switchMap, catchError } from 'rxjs/operators';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { SeaFCLExportLoadSuccessAction, SeaFCLExportActions, SeaFCLExportTypes, SeaFCLExportLoadFailAction, SeaFCLExportGetDetailSuccessAction, SeaFCLExportGetDetailFailAction } from '../actions';


@Injectable()
export class SeaFCLExportEffects {

    constructor(
        private actions$: Actions,
        private _documentRepo: DocumentationRepo
    ) { }

    @Effect()
    getListSeaFCLExportEffect$: Observable<Action> = this.actions$.
        pipe(
            ofType<SeaFCLExportActions>(SeaFCLExportTypes.LOAD_LIST),
            map((payload: any) => payload.payload),
            switchMap(
                (param: any) => this._documentRepo.getListShipmentDocumentation(param.page, param.size, param.dataSearch)
                    .pipe(
                        map((data: any) => new SeaFCLExportLoadSuccessAction(data)),
                        catchError(err => of(new SeaFCLExportLoadFailAction(err)))
                    )
            )
        );

    @Effect()
    getDetailFCLExportEffect: Observable<Action> = this.actions$.
        pipe(
            ofType<SeaFCLExportActions>(SeaFCLExportTypes.GET_DETAIL),
            map((payload: any) => payload.payload),
            switchMap(
                (id: string) => this._documentRepo.getDetailTransaction(id)
                    .pipe(
                        map((data: any) => new SeaFCLExportGetDetailSuccessAction(data)),
                        catchError(err => of(new SeaFCLExportGetDetailFailAction(err)))
                    )
            )
        );

}
