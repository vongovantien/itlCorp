import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AssigmentComponent } from './assigment/assigment.component';
import { TruckingAssignmentComponent } from './trucking-assignment/trucking-assignment.component';
import { OpsModuleBillingJobCreateComponent } from './ops-module-billing-job-create/ops-module-billing-job-create.component';
import { OpsModuleBillingComponent } from './ops-module-billing/ops-module-billing.component';

const routes: Routes = [
    {
        path: '',
        redirectTo: 'job-management',
        pathMatch: 'full'
    },
    //   {
    //     path:'job-management',
    //     component:JobMangementComponent
    //   },
    {
        path: 'job-management',
        component: OpsModuleBillingComponent
    },
    {
        path: 'job-create',
        component: OpsModuleBillingJobCreateComponent
    },
    {
        path: 'assigment',
        component: AssigmentComponent
    },
    {
        path: 'trucking-assigment',
        component: TruckingAssignmentComponent
    }

];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class OperationRoutingModule { }
