import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Action } from '@ngrx/store';
import { Actions, Effect, ofType } from '@ngrx/effects';
import { SeaFCLImportActionTypes, SeaFCLImportGetDetailSuccessAction, SeaFCLImportGetDetailFailAction, SeaFCLImportActions, ContainerActionTypes, ContainerAction, GetContainerSuccessAction, GetContainerFailAction } from '../actions';
import { map, switchMap, catchError } from 'rxjs/operators';
import { DocumentationRepo } from 'src/app/shared/repositories';

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
        )

}


