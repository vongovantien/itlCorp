import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Action } from '@ngrx/store';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { map, switchMap, catchError } from 'rxjs/operators';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { SeaFCLExportActions, SeaFCLExportTypes, SeaFCLExportLoadSuccessAction, SeaFCLExportLoadFailAction } from '../actions/sea-fcl-export.action';

@Injectable()
export class SeaFCLExportEffects {

    constructor(
        private actions$: Actions,
        private _documentRepo: DocumentationRepo
    ) { }

    @Effect()
    getDetailSeaFCLImportEffect$: Observable<Action> = this.actions$.
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
}
