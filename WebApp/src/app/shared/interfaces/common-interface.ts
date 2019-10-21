namespace CommonInterface {
    export interface IHeaderTable {
        title: string;
        field: string;
        sortable?: boolean;
        class?: string;
        width?: number;
        dataType?: CommonType.DataType;
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
        data?: any;
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
        value: any;
        displayName: string;
        fieldName?: string;
    }

    export interface ISortData {
        sortField: string;
        order: boolean;
    }
}



