import { NgModule } from '@angular/core';
import { UserManagementComponent } from './user-management.component';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { SelectModule } from 'ng2-select';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgProgressModule } from '@ngx-progressbar/core';
import { Routes, RouterModule } from '@angular/router';
import { PaginationModule, TabsModule } from 'ngx-bootstrap';
import { UserFormSearchComponent } from './components/form-search-user/form-search-user.component';
import { FormAddUserComponent } from './components/form-add-user/form-add-user/form-add-user.component';
import { UserAddNewComponent } from './addnew/user.addnew.component';

const routing: Routes = [

    { path: '', component: UserManagementComponent, data: { name: "User Management", level: 2 } },

    { path: 'new', component: UserAddNewComponent, data: { name: "Create User", level: 3 } },
];

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        SelectModule,
        FormsModule,
        NgProgressModule,
        PaginationModule.forRoot(),
        TabsModule.forRoot(),
        ReactiveFormsModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    declarations: [UserManagementComponent, UserFormSearchComponent, FormAddUserComponent, UserAddNewComponent],
    providers: [],
})
export class UserManagementModule { }
