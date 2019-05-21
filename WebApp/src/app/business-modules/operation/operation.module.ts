import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule} from '@angular/forms';
import { OperationRoutingModule } from './operation-routing.module';
import { JobMangementComponent } from './job-mangement/job-mangement.component';
import { AssigmentComponent } from './assigment/assigment.component';
import { TruckingAssignmentComponent } from './trucking-assignment/trucking-assignment.component';
import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SharedModule } from '../../shared/shared.module';
import { OpsModuleBillingJobCreateComponent } from './ops-module-billing-job-create/ops-module-billing-job-create.component';
import { OpsModuleBillingComponent } from './ops-module-billing/ops-module-billing.component';
import { OpsModuleBillingJobEditComponent } from './ops-module-billing-job-edit/ops-module-billing-job-edit.component';


@NgModule({
  imports: [
    CommonModule,
    OperationRoutingModule,
    SelectModule,
    FormsModule,
    SharedModule,
    NgxDaterangepickerMd
  ],
  declarations: [JobMangementComponent, AssigmentComponent, TruckingAssignmentComponent, OpsModuleBillingJobCreateComponent, OpsModuleBillingComponent, OpsModuleBillingJobEditComponent],
})
export class OperationModule { }
