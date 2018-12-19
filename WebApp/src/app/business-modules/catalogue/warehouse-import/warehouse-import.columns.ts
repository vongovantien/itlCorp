import { ColumnSetting } from "src/app/shared/models/layout/column-setting.model";

export const WAREHOUSEIMPORTENCOLUMNSETTING: ColumnSetting[] =
[
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
    primaryKey: 'nameEn',
    header: 'Name(EN)',
    isShow: true,
    dataType: 'text',
    allowSearch: true,
    required: true,
    lookup: ''
  },
  {
    primaryKey: 'nameVn',
    header: 'Name(Local)',
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
    dataType: 'text',
    allowSearch: true,
    lookup: ''
  },
  {
    primaryKey: 'provinceName',
    header: 'City/ Province',
    isShow: true,
    dataType: 'text',
    allowSearch: true,
    lookup: ''
  },
  {
    primaryKey: 'districtName',
    header: 'District',
    isShow: true,
    dataType: 'text',
    allowSearch: true,
    lookup: ''
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
    primaryKey: 'status',
    header: 'Status',
    isShow: true,
    dataType: 'text',
    required: true,
    lookup: ''
  }
]