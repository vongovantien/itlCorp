import { NgModule } from '@angular/core';
import { ModalModule } from 'ngx-bootstrap/modal';
import { Router, RouterModule, Routes } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { FileManagementComponent } from './file-management.component';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { ShareFileManagementModule } from './share-file-management.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

const routing: Routes = [
    {
        path: '', loadChildren: () => import('./accounting-file-management/accounting-file-management.module').then(m => m.AccountingFileManagementModule),
        data: { name: 'Accounting', title: 'Accounting' }
    },
    {
        path:'accounting', redirectTo : '' , pathMatch : 'full'
    },
    {
        path: 'document', loadChildren: () => import('./document-file-management/document-file-management.module').then(m => m.DocumentFileManagementModule),
        data: { name: 'Document', title: 'Document' }
    },
    {
        path: 'catalogue', loadChildren: () => import('./catalogue-file-management/catalogue-file-management.module').then(m => m.CatalogueFileManagementModule),
        data: { name: 'Catalogue', title: 'Catalogue' }
    },
    {
        path: 'system', loadChildren: () => import('./system-file-management/system-file-management.module').then(m => m.SystemFileManagementModule),
        data: { name: 'System', title: 'System' }
    }
]

@NgModule({
    declarations: [
        FileManagementComponent,
    ],

    imports: [
        TabsModule.forRoot(),
        RouterModule.forChild(routing),
        SharedModule,
        ModalModule,
        FormsModule,
        ReactiveFormsModule,
        ShareFileManagementModule
    ],
    exports: [RouterModule]

})
export class FilesManagementModule { }
