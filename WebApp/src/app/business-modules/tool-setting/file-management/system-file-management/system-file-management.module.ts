import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { SystemFileManagementComponent } from './system-file-management.component';
import { ShareFileManagementModule } from '../share-file-management.module';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { SharedModule } from 'src/app/shared/shared.module';
import { ModalModule } from 'ngx-bootstrap/modal';

const routing: Routes = [
  {
    path: '',
    component: SystemFileManagementComponent,
    data: { name: '' },
  },
]

@NgModule({
  declarations: [SystemFileManagementComponent],
  imports: [
    CommonModule,
		RouterModule.forChild(routing),
		ShareFileManagementModule,
		TabsModule.forRoot(),
		SharedModule,
		ModalModule,
  ]
})
export class SystemFileManagementModule { }
