import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { OperationRoutingModule } from './operation-routing.module';
import { JobMangementComponent } from './job-mangement/job-mangement.component';
import { AssigmentComponent } from './assigment/assigment.component';
import { TruckingAssignmentComponent } from './trucking-assignment/trucking-assignment.component';
import { Daterangepicker } from 'ng2-daterangepicker';
import { SelectModule } from 'ng2-select';

@NgModule({
  imports: [
    CommonModule,
    OperationRoutingModule,
    Daterangepicker,
    SelectModule
  ],
  declarations: [JobMangementComponent, AssigmentComponent, TruckingAssignmentComponent],
})
export class OperationModule { }
