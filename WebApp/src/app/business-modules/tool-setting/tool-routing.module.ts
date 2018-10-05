import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { EcusConnectionComponent } from './ecus-connection/ecus-connection.component';
import { IDDefinitionComponent } from './id-definition/id-definition.component';
import { KPIComponent } from './kpi/kpi.component';
import { SupplierComponent } from './supplier/supplier.component';
import { TariffComponent } from './tariff/tariff.component';

const routes: Routes = [
  {
    path:'',
    redirectTo:'ecus-connection',
    pathMatch:'full'
  },
  {
    path:'ecus-connection',
    component:EcusConnectionComponent
  },
  {
    path:'id-definition',
    component:IDDefinitionComponent
  },
  {
    path:'kpi',
    component:KPIComponent
  },
  {
    path:'supplier',
    component:SupplierComponent
  },
  {
    path:'tariff',
    component:TariffComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ToolRoutingModule { }
