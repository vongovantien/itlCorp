import { Injectable } from "@angular/core";
import { Action } from '@ngrx/store';
import { Actions, Effect, ofType } from "@ngrx/effects";

import { DocumentationRepo } from "src/app/shared/repositories";

import { Observable, of } from "rxjs";

import { switchMap, catchError, map } from "rxjs/operators";
import { ContainerAction, ContainerActionTypes, GetContainerSuccessAction, GetContainerFailAction } from "..";

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
                    )
            )
        );
}