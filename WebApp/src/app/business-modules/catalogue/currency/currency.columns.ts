import { ColumnSetting } from '../../../shared/models/layout/column-setting.model';

export const CURRENCYCOLUMNSETTING: ColumnSetting[] = [
    {
        primaryKey: 'id',
        header: 'Code',
        isShow: true,
        dataType: "text",
        allowSearch: true,
        lookup: ''
    },
    {
        primaryKey: 'currencyName',
        header: 'Name',
        isShow: true,
        dataType: 'text',
        allowSearch: true,
        required: true,
        lookup: ''
    },
    {
        primaryKey: 'isDefault',
        header: 'IsDefault',
        isShow: true,
        dataType: 'boolean',
        required: true,
        lookup: ''
    },
    {
        primaryKey: 'inactive',
        header: 'Inactive',
        isShow: true,
        dataType: 'boolean',
        required: true,
        lookup: ''
    }
];