import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { JobMangementComponent } from './job-mangement/job-mangement.component';
import { AssigmentComponent } from './assigment/assigment.component';
import { TruckingAssignmentComponent } from './trucking-assignment/trucking-assignment.component';

const routes: Routes = [
  {
    path:'',
    redirectTo:'job-management',
    pathMatch:'full' 
  },
  {
    path:'job-management',
    component:JobMangementComponent
  },
  {
    path:'assigment',
    component:AssigmentComponent
  },
  {
    path:'trucking-assigment',
    component:TruckingAssignmentComponent
  }
 
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OperationRoutingModule { }
