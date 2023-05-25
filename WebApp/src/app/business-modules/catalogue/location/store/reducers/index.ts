import { createFeatureSelector, ActionReducerMap, createSelector } from "@ngrx/store";
import { couReducer, ICountryListState } from "./country.reducer";
import { disReducer, IDistrictListState} from "./district.reducer";
import { ciReducer, ICityListState } from "./province.reducer";
import { IWardListState, waReducer } from "./ward.reducer";

//export * from './location.reducer';

export interface ICatLocationState {
    country: ICountryListState;
    city: ICityListState;
    district: IDistrictListState;
    ward: IWardListState;
}
export const reducers: ActionReducerMap<ICatLocationState> = {
    country: couReducer,
    city: ciReducer,
    district: disReducer,
    ward: waReducer  
};
export const locationState = createFeatureSelector<any>('locations');

export const getCountryDataSearch = createSelector(locationState, (state: ICatLocationState) => state && state.country?.dataSearch);
export const getLocationCountryState = createSelector(locationState, (state: ICatLocationState) => state.country?.countries);
export const getLocationCountryLoadingState = createSelector(locationState, (state: ICatLocationState) => state && state.country?.isLoading);

export const getCityDataSearch = createSelector(locationState, (state: ICatLocationState) => state && state.city?.dataSearch);
export const getLocationCityState = createSelector(locationState, (state: ICatLocationState) => state.city?.cities);
export const getLocationCityLoadingState = createSelector(locationState, (state: ICatLocationState) => state && state.city?.isLoading);

export const getDistrictDataSearch = createSelector(locationState, (state: ICatLocationState) => state && state.district?.dataSearch);
export const getLocationDistrictState = createSelector(locationState, (state: ICatLocationState) => state.district?.districts);
export const getLocationDistrictLoadingState = createSelector(locationState, (state: ICatLocationState) => state && state.district?.isLoading);

export const getWardDataSearch = createSelector(locationState, (state: ICatLocationState) => state && state.ward?.dataSearch);
export const getLocationWardState = createSelector(locationState, (state: ICatLocationState) =>  state.ward?.wards);
export const getLocationWardLoadingState = createSelector(locationState, (state: ICatLocationState) => state && state.ward?.isLoading);