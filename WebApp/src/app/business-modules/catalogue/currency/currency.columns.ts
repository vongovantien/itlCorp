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
        dataType: 'boolean2', // TODO tạm thời để kiểu này cho table có 2 cột có checkbox (DO BỊ DÍNH THẰNG ACTIVE TRONG COMPONENT INPUT-TABLE-LAYOUT)
        required: true,
        lookup: ''
    },
    {
        primaryKey: 'active',
        header: 'Status',
        isShow: true,
        dataType: 'boolean',
        required: true,
        lookup: ''
    }
];