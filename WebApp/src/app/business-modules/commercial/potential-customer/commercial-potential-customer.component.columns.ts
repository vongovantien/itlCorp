import { ColumnSetting } from "src/app/shared/models/layout/column-setting.model";

export const POTENTIALCUSTOMERCOLUMNSETTING: ColumnSetting[] =
    [
        {
            primaryKey: 'nameEn',
            header: 'English Name',
            isShow: true,
            dataType: "text",
            lookup: '',
            allowSearch: true
        },
        {
            primaryKey: 'nameLocal',
            header: 'Local Name',
            isShow: true,
            allowSearch: true,
            dataType: "text",
            required: true,
            lookup: ''
        },
        {
            primaryKey: 'taxCode',
            header: 'Tax Code',
            isShow: true,
            dataType: 'text',
            allowSearch: true,
            required: true,
            lookup: ''
        },
        {
            primaryKey: 'address',
            header: 'Address',
            isShow: true,
            dataType: 'text',
            allowSearch: true,
            lookup: ''
        },
        {
            primaryKey: 'tel',
            header: 'Tel',
            isShow: true,
            dataType: 'text',
            allowSearch: true,
            lookup: ''
        },
        {
            primaryKey: 'email',
            header: 'Email',
            isShow: true,
            dataType: 'text',
            allowSearch: true,
            lookup: ''
        },
        {
            primaryKey: 'potentialType',
            header: 'Type',
            isShow: true,
            dataType: 'text',
            allowSearch: true,
            lookup: ''
        },
        {
            primaryKey: 'userCreatedName',
            header: 'Creator',
            isShow: true,
            dataType: 'text',
            allowSearch: true,
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
    ]