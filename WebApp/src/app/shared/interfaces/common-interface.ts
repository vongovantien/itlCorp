namespace CommonInterface {
    export interface IHeaderTable {
        title: string;
        field: string;
        sortable?: boolean;
        class?: string;
        width?: number;
    }

    export interface IComboGirdConfig {
        placeholder: string;
        displayFields: any[];
        dataSource: any[];
        selectedDisplayFields: any[];
    }

    export interface IComboGridData {
        field: string;
        value: string;
    }

    export interface IResult {
        data: any;
        message: string;
        status: boolean;
    }

    export interface ICommonTitleValue {
        title: string;
        value: any;
    }

    export interface IError {
        message: string;
        title: string;
        data?: any;
    }

    export interface ICommonShipmentData {
        productServices: IValueDisplay[];
        serviceModes: IValueDisplay[];
        shipmentModes: IValueDisplay[];
    }

    export interface IValueDisplay {
        value: string;
        displayName: string;
        fieldName?: string;
    }

    export interface ISortData {
        sortField: string;
        order: boolean;
    }
}



