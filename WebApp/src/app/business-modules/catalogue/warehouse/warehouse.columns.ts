import { ColumnSetting } from "src/app/shared/models/layout/column-setting.model";

export const WAREHOUSECOLUMNSETTING: ColumnSetting[] =
[
  {
    primaryKey: 'id',
    header: 'Id',
    dataType: "number",
    lookup: ''
  },
  {
    primaryKey: 'code',
    header: 'Code',
    isShow: true,
    allowSearch: true,
    dataType: "text",
    required: true,
    lookup:''
  },
  {
    primaryKey: 'displayName',
    header: 'Name',
    isShow: true,
    dataType: 'text',
    allowSearch: true,
    required: true,
    lookup: ''
  },
  {
    primaryKey: 'countryName',
    header: 'Country',
    isShow: true,
    allowSearch: true,
    lookup: ''
  },
  {
    primaryKey: 'countryID',
    header: 'Country',
    isShow: false,
    required: true,
    lookup: 'countries'
  },
  {
    primaryKey: 'provinceName',
    header: 'City/ Province',
    isShow: true,
    allowSearch: true,
    lookup: ''
  },
  {
    primaryKey: 'provinceID',
    header: 'City/ Province',
    isShow: false,
    required: true,
    lookup: 'provinces'
  },
  {
    primaryKey: 'districtName',
    header: 'District',
    isShow: true,
    allowSearch: true,
    lookup: ''
  },
  {
    primaryKey: 'districtID',
    header: 'District',
    isShow: false,
    required: true,
    lookup: 'districts'
  },
  {
    primaryKey: 'address',
    header: 'Address',
    isShow: true,
    dataType: 'text',
    allowSearch: true,
    required: true,
    lookup: ''
  },
  {
    primaryKey: 'inactive',
    header: 'Inactive',
    isShow: true,
    dataType: 'boolean',
    allowSearch: true,
    required: true,
    lookup: ''
  }
]