import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LocationComponent } from './location/location.component';
import { PartnerComponent } from './partner-data/partner.component';
import { WarehouseComponent } from './warehouse/warehouse.component';
import { WarehouseImportComponent } from './warehouse-import/warehouse-import.component';
import { ChargeComponent } from './charge/charge.component';
import { ChargeAddnewComponent } from './charge-addnew/charge-addnew.component';
import { CommodityComponent } from './commodity/commodity.component';
import { PortIndexComponent } from './port-index/port-index.component';
import { PortIndexImportComponent } from './port-index-import/port-index-import.component';
import { StageManagementComponent } from './stage-management/stage-management.component';
import { UnitComponent } from './unit/unit.component';
import { PartnerDataAddnewComponent } from './partner-data-addnew/partner-data-addnew.component';
import { PartnerDataDetailComponent } from './partner-data-detail/partner-data-detail.component';
import { ChargeDetailsComponent } from './charge-details/charge-details.component';
import { CurrencyComponent } from './currency/currency.component';
import { LocationImportComponent } from './location-import/location-import.component';
import { StageImportComponent } from './stage-import/stage-import.component';
import { CommodityImportComponent } from './commodity-import/commodity-import.component';
import { CommodityGroupImportComponent } from './commodity-group-import/commodity-group-import.component';
import { PartnerDataImportComponent } from './partner-data-import/partner-data-import.component';
import { ChargeImportComponent } from './charge-import/charge-import.component';
import { ChargeImportAccountVoucherComponent } from './charge-import-account-voucher/charge-import-account-voucher.component';

const routes: Routes = [

  {
    path:'',
    redirectTo:'location',
    pathMatch:'full'
  },
  {
    path:'location',
    component:LocationComponent,
    data:{
      name:"Location",
      level:2
    }
  },
  {
    path:'location-import',
    component:LocationImportComponent,
    data:{
      name:"Location Import",
      level:3
    }
  },
  {
    path:'ware-house',
    component:WarehouseComponent,
    data:{
      name:"Ware House",
      level:2
    }
  },
  {
    path:'ware-house-import',
    component:WarehouseImportComponent,
    data:{
      name:"Ware House Import",
      level:3
    }
  },
  {
    path:'charge',
    component:ChargeComponent,
    data:{
      name:"Charge",
      level:2
    }
  },
  {
    path:'charge-addnew',
    component:ChargeAddnewComponent,
    data:{
      name:"Addnew Charge",
      level:3
    }
  },
  {
    path:'charge-edit',
    component:ChargeDetailsComponent,
    data:{
      name:"Edit Charge",
      level:3
    }
  },
  {
    path:'charge-import',
    component:ChargeImportComponent,
    data:{
      name:"Edit Import",
      level:3
    }
  },
  {
    path:'charge-import-account-voucher',
    component:ChargeImportAccountVoucherComponent
  },
  {
    path:'commodity',
    component:CommodityComponent,
    data:{
      name:"Commodity",
      level:2
    }
  },
  {
    path:'commodity-import',
    component:CommodityImportComponent,
    data:{
      name:"Commodity Import",
      level:3
    }
  },
  {
    path:'commodity-group-import',
    component:CommodityGroupImportComponent,
    data:{
      name:"Commodity Group Import",
      level:3
    }
  },
  {
    path:'partner-data',
    component:PartnerComponent,
    data:{
      name:"Partner Data",
      level:2
    }
  },
  {
    path: 'partner-data-import',
    component: PartnerDataImportComponent,
    data:{
      name:"Partner Data Import",
      level:3
    }
  },
  {
    path:'port-index',
    component:PortIndexComponent,
    data:{
      name:"Port Index",
      level:2
    }
  },
  {
    path:'port-index-import',
    component:PortIndexImportComponent,
    data:{
      name:"Port Index Import",
      level:3
    }
  },
  {
    path:'stage-management',
    component:StageManagementComponent,
    data:{
      name:"Stage Management",
      level:2
    }
  },
  {
    path:'stage-import',
    component:StageImportComponent,
    data:{
      name:"Stage Import",
      level:3
    }
  },
  {
    path:'unit',
    component:UnitComponent,
    data:{
      name:"Unit",
      level:2
    }
  },
  {
    path:'partner-data-addnew',
    component:PartnerDataAddnewComponent,
    data:{
      name:"Partner Data Addnew",
      level:3
    }
  },
  {
    path:'partner-data-detail',
    component:PartnerDataDetailComponent,
    data:{
      name:"Partner Data Details",
      level:3
    }
  },
  {
    path:'currency',
    component:CurrencyComponent,
    data:{
      name:"Currency",
      level:2
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CatalogueRoutingModule { }
