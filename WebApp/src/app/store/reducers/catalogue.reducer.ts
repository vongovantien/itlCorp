import { CatalogueActionTypes, CatalogueActions } from "../actions/catalogue.action";
import { PortIndex, Customer } from "@models";


export interface ICatalogueState {
    ports: PortIndex[];
    carriers: Customer[];
    agents: Customer[];
    isLoading: boolean;

}

const initialState: ICatalogueState = {
    ports: [],
    carriers: [],
    agents: [],
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

        default: {
            return state;
        }
    }
}
