import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormControl } from '@angular/forms';
import { OperationRoutingModule } from './operation-routing.module';
import { JobMangementComponent } from './job-mangement/job-mangement.component';
import { AssigmentComponent } from './assigment/assigment.component';
import { TruckingAssignmentComponent } from './trucking-assignment/trucking-assignment.component';
import { Daterangepicker } from 'ng2-daterangepicker';
import { SelectModule } from 'ng2-select';
import { PagingClientComponent } from 'src/app/shared/paging-client/paging-client.component';

@NgModule({
  imports: [
    CommonModule,
    OperationRoutingModule,
    Daterangepicker,
    SelectModule,
    FormsModule
  ],
  declarations: [JobMangementComponent, AssigmentComponent, TruckingAssignmentComponent,PagingClientComponent],
})
export class OperationModule { }
