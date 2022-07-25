import { SharedModule } from './../../../shared/shared.module';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonComponentModule } from 'src/app/shared/common/common.module';
import { DirectiveModule } from 'src/app/shared/directives/directive.module';
import { FormSearchFileManagementComponent } from './components/form-search-file-management/form-search-file-management.component';
import { SidebarFileManagementComponent } from './components/sidebar-file-management/sidebar-file-management.component';


@NgModule({
    imports: [
        RouterModule,
        CommonModule,
        FormsModule,
        SharedModule,
        CommonComponentModule,
        DirectiveModule,
        ReactiveFormsModule,
    ],
    exports: [SidebarFileManagementComponent, FormSearchFileManagementComponent],
    declarations: [SidebarFileManagementComponent, FormSearchFileManagementComponent],
    providers: [],
})
export class ShareFileManagementModule { }
