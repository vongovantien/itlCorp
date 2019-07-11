import { ColumnSetting } from '../../../shared/models/layout/column-setting.model';

export const EXCHANGERATECOLUMNSETTING: ColumnSetting[] = [
    {
        primaryKey: 'datetimeCreated',
        header: 'Exchange rate date',
        isShow: true,
        dataType: "Date",
        format: "EnglishDate"
    },
    {
        primaryKey: 'localCurrency',
        header: 'Local currency',
        isShow: true,
        dataType: 'text',
        allowSearch: true,
        required: true,
        lookup: ''
    },
    {
        primaryKey: 'userModifield',
        header: 'Update by',
        isShow: true,
        dataType: 'text',
        required: true,
        lookup: ''
    },
    {
        primaryKey: 'datetimeUpdated',
        header: 'Update time',
        isShow: true,
        dataType: 'Date',
        required: true,
        lookup: '',
        format: 'date-time'
    }
];