import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DocumentFileManagementComponent } from './document-file-management.component';
import { RouterModule, Routes } from '@angular/router';
import { ShareFileManagementModule } from '../share-file-management.module';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { SharedModule } from 'src/app/shared/shared.module';
import { ModalModule } from 'ngx-bootstrap/modal';

const routing: Routes = [
    {
        path: '',
        component: DocumentFileManagementComponent,
        data: { name: '' },
    },
]

@NgModule({
    declarations: [DocumentFileManagementComponent],
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        ShareFileManagementModule,
        TabsModule.forRoot(),
        SharedModule,
        ModalModule,
    ]
})
export class DocumentFileManagementModule { }
