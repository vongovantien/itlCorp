import { Injectable } from "@angular/core";
import { User } from "@models";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { Action, ActionType, Store } from "@ngrx/store";
import { SystemRepo } from "@repositories";
import { Observable, of } from "rxjs";
import { GetSystemUserFail, GetSystemUserSuccess, SystemAppActionTypes } from "../actions/system.action";
import { map, catchError, switchMap, withLatestFrom } from "rxjs/operators";
import { ISystemAppState } from "../reducers/system.reducer";
import { getSystemUserState } from "../reducers";


@Injectable()
export class SystemEffects {
    constructor(
        private actions$: Actions,
        private _systemRepo: SystemRepo,
        private _store: Store<ISystemAppState>

    ) { }

    getListUserEffect$: Observable<Action> = createEffect(() => this.actions$.pipe(
        ofType(SystemAppActionTypes.GET_USER),
        withLatestFrom(
            this._store.select(getSystemUserState),
            (action: SystemAppActionTypes, users: User[]) => ({ data: users, action: action })
        ),
        switchMap(
            ((data: { data: User[], action: ActionType<any> }) => {
                if (!!data && data.data && data.data.length) {
                    return of(GetSystemUserSuccess({ data: data.data }));
                }
                return this._systemRepo.getSystemUsers(data.action.payload).pipe(
                    map((data: User[]) => GetSystemUserSuccess({ data })),
                    catchError(err => of(GetSystemUserFail()))
                );
            })
        )
    ));
}