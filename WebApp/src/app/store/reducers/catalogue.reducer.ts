import { CatalogueActionTypes, CatalogueActions } from "../actions/catalogue.action";
import { PortIndex, Customer, Unit, Commodity, CountryModel, Currency } from "@models";


export interface ICatalogueState {
    ports: PortIndex[];
    carriers: Customer[];
    agents: Customer[];
    units: Unit[];
    customers: Customer[];
    countries: CountryModel[];
    commodities: Commodity[];
    currencies: Currency[];
    isLoading: boolean;

}

const initialState: ICatalogueState = {
    ports: [],
    carriers: [],
    agents: [],
    units: [],
    customers: [],
    commodities: [],
    countries: [],
    currencies: [],
    isLoading: false
};


export function catalogueReducer(state = initialState, action: CatalogueActions): ICatalogueState {
    switch (action.type) {
        case CatalogueActionTypes.GET_PARTNER: {
            return { ...state, isLoading: true };
        }
        case CatalogueActionTypes.GET_PORT: {
            return { ...state, isLoading: true };
        }
        case CatalogueActionTypes.GET_PORT_SUCCESS: {
            return { ...state, isLoading: false, ports: action.payload };
        }
        case CatalogueActionTypes.GET_PORT_FAIL: {
            return { ...state, isLoading: false, };
        }

        case CatalogueActionTypes.GET_CARRIER: {
            return { ...state, isLoading: true };
        }
        case CatalogueActionTypes.GET_CARRIER_SUCCESS: {
            return { ...state, isLoading: false, carriers: action.payload };
        }
        case CatalogueActionTypes.GET_CARRIER_FAIL: {
            return { ...state, isLoading: false, };
        }

        case CatalogueActionTypes.GET_AGENT: {
            return { ...state, isLoading: true };
        }
        case CatalogueActionTypes.GET_AGENT_SUCCESS: {
            return { ...state, isLoading: false, agents: action.payload };
        }
        case CatalogueActionTypes.GET_AGENT_FAIL: {
            return { ...state, isLoading: false, };
        }

        case CatalogueActionTypes.GET_UNIT: {
            return { ...state, isLoading: true };
        }
        case CatalogueActionTypes.GET_UNIT_SUCCESS: {
            return { ...state, isLoading: false, units: action.payload };
        }
        case CatalogueActionTypes.GET_UNIT_FAIL: {
            return { ...state, isLoading: false, };
        }

        case CatalogueActionTypes.GET_COMMODITY: {
            return { ...state, isLoading: true };
        }
        case CatalogueActionTypes.GET_COMMODITY_SUCCESS: {
            return { ...state, isLoading: false, commodities: action.payload };
        }
        case CatalogueActionTypes.GET_COMMODITY_FAIL: {
            return { ...state, isLoading: false, };
        }

        case CatalogueActionTypes.GET_COUNTRY: {
            return { ...state, isLoading: true };
        }
        case CatalogueActionTypes.GET_COUNTRY_SUCCESS: {
            return { ...state, isLoading: false, countries: action.payload };
        }
        case CatalogueActionTypes.GET_COUNTRY_FAIL: {
            return { ...state, isLoading: false, };
        }

        case CatalogueActionTypes.GET_CURRENCY: {
            return { ...state, isLoading: true };
        }
        case CatalogueActionTypes.GET_CURRENCY_SUCCESS: {
            return { ...state, isLoading: false, currencies: action.payload };
        }
        case CatalogueActionTypes.GET_CURRENCY_FAIL: {
            return { ...state, isLoading: false, };
        }

        default: {
            return state;
        }
    }
}
