import { Injectable } from "@angular/core";
import { ofType, Actions, Effect, createEffect } from "@ngrx/effects";
import { switchMap, map, catchError, withLatestFrom } from "rxjs/operators";
import { EMPTY, Observable, of } from "rxjs";
import { CatalogueRepo } from "@repositories";
import { Action, Store } from '@ngrx/store';
import { CityActionTypes, CountryActionTypes, DistrictActionTypes,  LoadListCitySuccess,  LoadListCountrySuccess,  LoadListDistrictSuccess, LoadListWardSuccess, WardActionTypes } from "../actions";

@Injectable()
export class LocationEffect {
    constructor(
        private actions$: Actions,
        private _catalogueRepo: CatalogueRepo
    ) { }

    getListDistrictEffect$: Observable<Action> = createEffect(() => this.actions$
        .pipe(
            ofType(DistrictActionTypes.LOAD_LIST_DISTRICT),
            switchMap(
                (param: CommonInterface.IParamPaging) => this._catalogueRepo.getListDistrict(param.page, param.size, param.dataSearch)
                    .pipe(
                        catchError(() => EMPTY),
                        map((data: CommonInterface.IResponsePaging) =>LoadListDistrictSuccess(data))
                    )
            )
        ));

        getListCountryEffect$: Observable<Action> = createEffect(() => this.actions$
        .pipe(
            ofType(CountryActionTypes.LOAD_LIST_COUNTRY),
            switchMap(
                (param: CommonInterface.IParamPaging) => this._catalogueRepo.getCountry(param.page, param.size, param.dataSearch)
                    .pipe(
                        catchError(() => EMPTY),
                        map((data: CommonInterface.IResponsePaging) =>LoadListCountrySuccess(data))
                    )
            )
        ));
        getListCityEffect$: Observable<Action> = createEffect(() => this.actions$
        .pipe(
            ofType(CityActionTypes.LOAD_LIST_CITY),
            switchMap(
                (param: CommonInterface.IParamPaging) => this._catalogueRepo.getAllProvince(param.page, param.size, param.dataSearch)
                    .pipe(
                        catchError(() => EMPTY),
                        map((data: CommonInterface.IResponsePaging) =>LoadListCitySuccess(data))
                    )
            )
        ));

        getListWardEffect$: Observable<Action> = createEffect(() => this.actions$
        .pipe(
            ofType(WardActionTypes.LOAD_LIST_WARD),
            switchMap(
                (param: CommonInterface.IParamPaging) => this._catalogueRepo.getAllWards(param.page, param.size, param.dataSearch)
                    .pipe(
                        catchError(() => EMPTY),
                        map((data: CommonInterface.IResponsePaging) =>LoadListWardSuccess(data))
                    )
            )
        ));

}