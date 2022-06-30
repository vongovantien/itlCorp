import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccoutingFileManagementComponent } from './accouting-file-management.component';
import { RouterModule, Routes } from '@angular/router';
import { ShareFileManagementModule } from '../share-file-management.module';
import { ModalModule } from 'ngx-bootstrap/modal';
import { SharedModule } from 'src/app/shared/shared.module';
import { TabsModule } from 'ngx-bootstrap/tabs';

const routing: Routes = [
	{
		path: '',
		data: { name: '' },
		children: [
			{
				path: '', component: AccoutingFileManagementComponent
			},
		]
	},
]
@NgModule({
	declarations: [AccoutingFileManagementComponent],
	imports: [
		CommonModule,
		RouterModule.forChild(routing),
		ShareFileManagementModule,
		TabsModule.forRoot(),
		SharedModule,
		ModalModule,
	]
})
export class AccoutingFileManagementModule { }
