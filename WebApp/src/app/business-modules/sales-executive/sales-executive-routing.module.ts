import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { SalesExecutiveComponent } from './sales-executive/sales-executive.component';

const routes: Routes = [
  {
    path:'sales-executive',
    component:SalesExecutiveComponent
  },
  {
    path:'',
    redirectTo:'sales-executive',
    pathMatch:'full'
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SalesExecutiveRoutingModule { }
