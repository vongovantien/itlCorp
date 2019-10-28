import { ColumnSetting } from "src/app/shared/models/layout/column-setting.model";

export const PORTINDEXCOLUMNSETTING: ColumnSetting[] =
  [
    {
      primaryKey: 'id',
      header: 'Id',
      dataType: "number",
      lookup: ''
    },
    {
      primaryKey: 'code',
      header: 'Port Code',
      isShow: true,
      allowSearch: true,
      dataType: "text",
      required: true,
      lookup: ''
    },
    {
      primaryKey: 'nameEn',
      header: 'Name (EN)',
      isShow: true,
      dataType: 'text',
      allowSearch: true,
      required: true,
      lookup: ''
    },
    {
      primaryKey: 'nameVn',
      header: 'Name (Local)',
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
      primaryKey: 'countryID',
      header: 'Country',
      isShow: false,
      required: true,
      lookup: 'countries'
    },
    {
      primaryKey: 'areaName',
      header: 'Zone',
      isShow: true,
      dataType: 'text',
      allowSearch: true,
      lookup: ''
    },
    {
      primaryKey: 'areaID',
      header: 'Zone',
      isShow: false,
      required: true,
      lookup: 'provinces'
    },
    {
      primaryKey: 'modeOfTransport',
      header: 'Mode',
      isShow: true,
      dataType: 'text',
      allowSearch: true,
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
  ]