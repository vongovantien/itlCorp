import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { DocumentationRepo } from "@repositories";
import { LoadListWorkOrderSuccess, LoadListWorkOrderFail, WorkOrderActionTypes } from "../actions";
import { map, catchError, switchMap } from "rxjs/operators";
import { Observable, of } from "rxjs";
import { Action } from "@ngrx/store";

@Injectable()
export class WorkOrderEffects {

    constructor(
        private actions$: Actions,
        private _documentationRepo: DocumentationRepo,
    ) { }

    getListWorkOrderEffect$: Observable<Action> = createEffect(() => this.actions$
        .pipe(
            ofType(WorkOrderActionTypes.LOAD_LIST),
            switchMap(
                (param: CommonInterface.IParamPaging) => this._documentationRepo.getListWorkOrder(param.page, param.size, param.dataSearch)
                    .pipe(
                        catchError(() => of(LoadListWorkOrderFail())),
                        map((data: CommonInterface.IResponsePaging) => LoadListWorkOrderSuccess(data)),
                    )
            )
        ));
}
