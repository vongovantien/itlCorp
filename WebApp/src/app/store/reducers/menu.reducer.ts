import { MenuActions, MenuActionTypes } from "../actions";


export interface IMenuState {
    permission: SystemInterface.IUserPermission;
}

const initialState: IMenuState = {
    permission: {
        allowAdd: true,
        access: true,
        export: true,
        menuId: null,
        detail: null,
        delete: null,
        list: null,
        import: true,
        write: null,
        speacialActions: []
    }
};

export function menuReducer(state: IMenuState = initialState, action: MenuActions): IMenuState {
    switch (action.type) {
        case MenuActionTypes.GET_PERMISSION:
            return {
                ...state
            };
        case MenuActionTypes.UPDATE_PERMISSION:
            return {
                ...state, permission: action.payload
            };
        default:
            break;
    }
};
