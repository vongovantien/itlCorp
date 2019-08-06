namespace CommonInterface {
    export interface IHeaderTable {
        title: string;
        field: string;
        sortable?: boolean;
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
        value: string;
    }
}



