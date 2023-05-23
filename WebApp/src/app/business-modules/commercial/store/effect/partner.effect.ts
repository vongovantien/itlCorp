import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { Action } from '@ngrx/store';
import { CatalogueRepo } from "@repositories";
import { EMPTY, Observable } from "rxjs";
import { catchError, map, switchMap } from "rxjs/operators";
import { PartnerActionTypes, getDetailPartnerSuccess } from '../actions/partner.action';

@Injectable()
export class PartnerEffect {

    constructor(
        private actions$: Actions,
        private _catalogueRepo: CatalogueRepo,
    ) { }

    getDetailPartnerEffect$: Observable<Action> = createEffect(() => this.actions$
        .pipe(
            ofType(PartnerActionTypes.GET_DETAIL),
            switchMap(
                (param: any) => this._catalogueRepo.getDetailPartner(param)
                    .pipe(
                        catchError(() => EMPTY),
                        map((res: CommonInterface.IResult) => getDetailPartnerSuccess(res.data)),

                    )
            )
        ));
}
